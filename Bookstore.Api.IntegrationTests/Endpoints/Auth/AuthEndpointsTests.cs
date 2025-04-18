using Bookstore.Application.Dtos;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
namespace Bookstore.Api.IntegrationTests.Endpoints.Auth
{
    public class AuthEndpointsTests : IClassFixture<BookstoreApiFactory>
    {
        private readonly HttpClient _client;

        public AuthEndpointsTests(BookstoreApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            // Arrange (Chuẩn bị)
            var registerDto = new RegisterRequestDto
            {
                UserName = $"testuser_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Integration",
                LastName = "Test"
            };

            // Act (Thực hiện)
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert (Kiểm chứng)
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
            createdUser.Should().NotBeNull(); // Hoặc Assert.NotNull(createdUser);
            createdUser!.UserName.Should().Be(registerDto.UserName);
            createdUser!.Email.Should().Be(registerDto.Email);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            // Bước 1: Đăng ký user đầu tiên thành công (cần đảm bảo user này tồn tại)
            var initialUser = new RegisterRequestDto { UserName = "existing_user_test", Email = "unique1@example.com", Password = "p", ConfirmPassword = "p" };
            await _client.PostAsJsonAsync("/api/auth/register", initialUser);

            var registerDto = new RegisterRequestDto
            {
                UserName = "existing_user_test",
                Email = $"unique2_{Guid.NewGuid()}@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

    }
}