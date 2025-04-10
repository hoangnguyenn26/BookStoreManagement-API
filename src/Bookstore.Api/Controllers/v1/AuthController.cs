// src/Bookstore.Api/Controllers/v1/AuthController.cs
using Bookstore.Application.Dtos;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories; // Cần IUserRepository, IRoleRepository (hoặc DbContext trực tiếp cho role)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần cho DbContext nếu dùng trực tiếp
using Bookstore.Infrastructure.Persistence;
using Bookstore.Domain.Interfaces.Services; // Namespace DbContext

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")] // Không cần versioning cho Auth thường là chấp nhận được
    // [ApiVersion("1.0")] // Hoặc vẫn để versioning nếu muốn
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;// Inject DbContext để lấy Role (hoặc tạo IRoleRepository)

        public AuthController(IUserRepository userRepository, ApplicationDbContext context, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto, CancellationToken cancellationToken)
        {
            // --- Validation Cơ bản ---
            // ModelState đã tự động được kiểm tra nhờ [ApiController]
            // if (!ModelState.IsValid)
            // {
            //     return BadRequest(ModelState);
            // }

            // --- Kiểm tra User/Email đã tồn tại chưa ---
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(registerDto.UserName, cancellationToken);
            if (existingUserByUsername != null)
            {
                // Trả về lỗi chung chung để tránh lộ thông tin user nào đã tồn tại
                return BadRequest(new { Errors = new[] { "Username or Email already exists." } });
            }

            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email, cancellationToken);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { Errors = new[] { "Username or Email already exists." } });
            }

            // --- Lấy Role "User" ---
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
            if (userRole == null)
            {
                // Lỗi nghiêm trọng: Role "User" không tồn tại trong CSDL (cần được seed)
                // Log lỗi này và trả về lỗi server
                // Log.Error("Critical: 'User' role not found in database during registration.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }

            // --- Tạo Entity User mới ---
            var newUser = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                IsActive = true, // Mặc định là active
                // Hash mật khẩu
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            // --- Thêm User và UserRole vào Context và Lưu ---
            // Sử dụng Transaction để đảm bảo cả hai được lưu hoặc không lưu gì cả
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _userRepository.AddAsync(newUser, cancellationToken); // AddAsync chỉ thêm vào context, chưa lưu DB
                await _context.SaveChangesAsync(cancellationToken); // Lưu User để có UserId

                // Tạo bản ghi UserRole
                var newUserRole = new UserRole { UserId = newUser.Id, RoleId = userRole.Id };
                _context.UserRoles.Add(newUserRole);
                await _context.SaveChangesAsync(cancellationToken); // Lưu UserRole

                await transaction.CommitAsync(cancellationToken); // Hoàn tất transaction

                // --- Chuẩn bị Response ---
                // Không trả về mật khẩu hoặc thông tin nhạy cảm
                // Có thể trả về UserDto hoặc chỉ là thông báo thành công
                var userDto = new UserDto // Tạo UserDto thủ công
                {
                    Id = newUser.Id,
                    UserName = newUser.UserName,
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    PhoneNumber = newUser.PhoneNumber,
                    IsActive = newUser.IsActive
                };

                // Trả về 201 Created với thông tin user (hoặc URL để lấy user)
                return CreatedAtAction(nameof(UsersController.GetUserById), "Users", new { id = newUser.Id, version = "1.0" }, userDto);
                // return Ok(new { Message = "User registered successfully." }); // Hoặc đơn giản hơn
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                // Log lỗi chi tiết (ex)
                // Log.Error(ex, "Error occurred during user registration for {Username}", registerDto.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during registration.");
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Sai input
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Sai login/pass hoặc user inactive
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginDto, CancellationToken cancellationToken)
        {
            // --- Tìm User ---
            // Thử tìm bằng Email trước (phổ biến hơn)
            var user = await _userRepository.GetByEmailAsync(loginDto.LoginIdentifier, cancellationToken);
            // Nếu không thấy bằng Email, thử tìm bằng Username
            if (user == null)
            {
                user = await _userRepository.GetByUsernameAsync(loginDto.LoginIdentifier, cancellationToken);
            }

            // --- Kiểm tra User tồn tại và Active ---
            if (user == null || !user.IsActive)
            {
                // Không tìm thấy user hoặc user bị khóa -> Unauthorized
                return Unauthorized(new { Message = "Invalid login attempt." }); // Trả lỗi chung chung
            }

            // --- Kiểm tra Mật khẩu ---
            // Sử dụng BCrypt để so sánh mật khẩu nhập vào với hash trong CSDL
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid login attempt." }); // Sai mật khẩu -> Unauthorized
            }

            // --- Lấy Roles của User ---
            // Cần join UserRoles và Roles
            var userRoles = await _context.UserRoles
                                        .Where(ur => ur.UserId == user.Id)
                                        .Include(ur => ur.Role) // Include thông tin Role
                                        .Select(ur => ur.Role.Name) // Chỉ lấy tên Role
                                        .ToListAsync(cancellationToken);

            if (userRoles == null || !userRoles.Any())
            {
                // Log lỗi: User không có role nào được gán?
                // Log.Warning("User {UserId} logged in but has no roles assigned.", user.Id);
                // Vẫn có thể cho đăng nhập nếu logic cho phép, hoặc trả lỗi nếu role là bắt buộc
                // return StatusCode(StatusCodes.Status500InternalServerError, "User role configuration error.");
            }


            // --- Tạo JWT Token ---
            var tokenString = _tokenService.CreateToken(user, userRoles ?? new List<string>()); // Truyền user và list tên role
            var expiration = DateTime.UtcNow.AddMinutes(60); // Lấy expiration từ JwtSettings sẽ chính xác hơn

            // --- Tạo User DTO cho Response ---
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
                // Roles = userRoles ?? new List<string>() // Có thể thêm roles vào UserDto nếu muốn
            };

            // --- Trả về Response ---
            return Ok(new LoginResponseDto
            {
                Token = tokenString,
                Expiration = expiration,
                User = userDto
            });
        }
    }
}