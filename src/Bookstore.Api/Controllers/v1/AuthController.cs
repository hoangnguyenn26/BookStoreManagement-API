
using Bookstore.Application.Dtos;
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bookstore.Infrastructure.Persistence;
using Bookstore.Domain.Interfaces.Services;
using Bookstore.Application.Interfaces;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public AuthController(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
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

                var userDto = new UserDto 
                {
                    Id = newUser.Id,
                    UserName = newUser.UserName,
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    PhoneNumber = newUser.PhoneNumber,
                    IsActive = newUser.IsActive
                };

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

            if (userRoles == null || !userRoles.Any())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "User role configuration error.");
            }

            var tokenString = _tokenService.CreateToken(user, userRoles?.ToList() ?? new List<string>());
            var expiration = DateTime.UtcNow.AddMinutes(60);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            };

            return Ok(new LoginResponseDto
            {
                Token = tokenString,
                Expiration = expiration,
                User = userDto
            });
        }
    }
}