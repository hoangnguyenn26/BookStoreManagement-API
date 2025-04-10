// src/Bookstore.Api/Controllers/v1/AuthController.cs
using Bookstore.Application.Dtos;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories; // Cần IUserRepository, IRoleRepository (hoặc DbContext trực tiếp cho role)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần cho DbContext nếu dùng trực tiếp
using Bookstore.Infrastructure.Persistence; // Namespace DbContext

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")] // Không cần versioning cho Auth thường là chấp nhận được
    // [ApiVersion("1.0")] // Hoặc vẫn để versioning nếu muốn
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context; // Inject DbContext để lấy Role (hoặc tạo IRoleRepository)

        public AuthController(IUserRepository userRepository, ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
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
    }
}