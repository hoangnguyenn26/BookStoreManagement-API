
using Bookstore.Application.Dtos.Addresses;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<AddressDto?> GetAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
        Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressDto createAddressDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto updateAddressDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
        Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
    }
}