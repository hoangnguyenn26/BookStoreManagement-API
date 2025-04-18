
using AutoMapper;
using Bookstore.Application.Dtos.Suppliers;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SupplierService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new supplier: {SupplierName}", createDto.Name);

            var supplierEntity = _mapper.Map<Supplier>(createDto);
            var createdSupplier = await _unitOfWork.SupplierRepository.AddAsync(supplierEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Supplier {SupplierName} created with Id {SupplierId}", createdSupplier.Name, createdSupplier.Id);
            return _mapper.Map<SupplierDto>(createdSupplier);
        }

        public async Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to delete supplier with Id {SupplierId}", id);
            var supplier = await _unitOfWork.SupplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
            {
                _logger.LogWarning("Supplier {SupplierId} not found for deletion.", id);
                return false;
            }
            //Có thể thêm ràng buộc trước khi xoá

            await _unitOfWork.SupplierRepository.DeleteAsync(supplier, cancellationToken); // Giả sử xóa vật lý
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Supplier {SupplierId} deleted successfully.", id);
            return true;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all suppliers.");
            var suppliers = await _unitOfWork.SupplierRepository.ListAsync(orderBy: q => q.OrderBy(s => s.Name), isTracking: false, cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching supplier with Id {SupplierId}", id);
            var supplier = await _unitOfWork.SupplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
            {
                _logger.LogWarning("Supplier {SupplierId} not found.", id);
                return null;
            }
            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<bool> UpdateSupplierAsync(Guid id, UpdateSupplierDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to update supplier {SupplierId}", id);
            var supplierToUpdate = await _unitOfWork.SupplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplierToUpdate == null)
            {
                _logger.LogWarning("Supplier {SupplierId} not found for update.", id);
                return false;
            }

            // (Optional) Kiểm tra email trùng nếu email được thay đổi
            // if (supplierToUpdate.Email != updateDto.Email) { ... check existing email ... }

            _mapper.Map(updateDto, supplierToUpdate); // Map DTO vào entity đã tải
            await _unitOfWork.SupplierRepository.UpdateAsync(supplierToUpdate, cancellationToken); // Đánh dấu modified
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Supplier {SupplierId} updated successfully.", id);
            return true;
        }
    }
}