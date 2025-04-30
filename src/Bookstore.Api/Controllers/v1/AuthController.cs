
using AutoMapper;
using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthController(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto, CancellationToken cancellationToken)
        {
            var existingUserByUsername = await _unitOfWork.UserRepository.GetByUsernameAsync(registerDto.UserName, cancellationToken);
            if (existingUserByUsername != null)
            {
                return BadRequest(new { Errors = new[] { "Username or Email already exists." } });
            }

            var existingUserByEmail = await _unitOfWork.UserRepository.GetByEmailAsync(registerDto.Email, cancellationToken);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { Errors = new[] { "Username or Email already exists." } });
            }

            var userRole = await _unitOfWork.RoleRepository.GetByNameAsync("User", cancellationToken);
            if (userRole == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }

            var newUser = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                UserRoles = new List<UserRole> { new UserRole { RoleId = userRole.Id } }
            };
            newUser.UserRoles.Add(new UserRole { Role = userRole });

            try
            {
                await _unitOfWork.UserRepository.AddAsync(newUser, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var userDto = _mapper.Map<UserDto>(newUser);

                return CreatedAtAction(nameof(UsersController.GetUserById), "Users", new { id = newUser.Id, version = "1.0" }, userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during registration.");
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginDto, CancellationToken cancellationToken)
        {
            // --- Tìm User ---
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(loginDto.LoginIdentifier, cancellationToken);
            if (user == null)
            {
                user = await _unitOfWork.UserRepository.GetByUsernameAsync(loginDto.LoginIdentifier, cancellationToken);
            }

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { Message = "Invalid login attempt." });
            }


            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid login attempt." });
            }

            var userRoles = await _unitOfWork.RoleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);

            if (userRoles == null) userRoles = new List<string>();

            var tokenString = _tokenService.CreateToken(user, userRoles);
            var expiration = DateTime.UtcNow.AddMinutes(60);

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new LoginResponseDto
            {
                Token = tokenString,
                Expiration = expiration,
                User = userDto
            });
        }
    }
}