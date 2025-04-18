
using Bookstore.Application.Dtos.Suppliers;
namespace Bookstore.Application.Interfaces.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);
        Task<SupplierDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateSupplierAsync(Guid id, UpdateSupplierDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default);
    }
}