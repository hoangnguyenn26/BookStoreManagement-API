
using Bookstore.Application.Dtos; 
using Bookstore.Domain.Interfaces.Repositories; 
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization; // Sẽ dùng sau

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    // [Authorize] // Sẽ thêm sau khi implement Auth
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        // private readonly IMapper _mapper; // Sẽ inject sau khi cấu hình AutoMapper

        // Inject IUserRepository thông qua constructor
        public UsersController(IUserRepository userRepository /*, IMapper mapper*/)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            // _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // GET: api/v1/users
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        // [Authorize(Roles = "Admin")] // Sẽ cần quyền Admin sau này
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(CancellationToken cancellationToken)
        {
            // Sử dụng phương thức từ Repository (dùng ListAsync hoặc GetAllAsync nếu bạn implement)
            var users = await _userRepository.ListAsync(cancellationToken: cancellationToken);

            // --- Mapping thủ công (Tạm thời trước khi dùng AutoMapper) ---
            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
                // Map Roles nếu cần
            }).ToList();
            // --- Kết thúc Mapping thủ công ---

            // --- Sử dụng AutoMapper (Sau khi cấu hình ở Ngày 7 hoặc Tuần 2) ---
            // var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            // --- Kết thúc AutoMapper ---

            return Ok(userDtos);
        }

        // Các Actions khác (GET by Id, POST, PUT, DELETE) sẽ được thêm sau
    }
}