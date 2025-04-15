
using AutoMapper;
using Bookstore.Application.Dtos.Addresses;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressDto createAddressDto, CancellationToken cancellationToken = default)
        {
            var addressEntity = _mapper.Map<Address>(createAddressDto);
            addressEntity.UserId = userId;
            if (addressEntity.IsDefault)
            {
                await _unitOfWork.AddressRepository.UnsetDefaultAddressesAsync(userId, null, cancellationToken);
            }

            var createdAddress = await _unitOfWork.AddressRepository.AddAsync(addressEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<AddressDto>(createdAddress);
        }

        public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
        {
            var address = await _unitOfWork.AddressRepository.GetByIdAsync(addressId, cancellationToken);
            // Quan trọng: Kiểm tra xem địa chỉ này có thuộc về đúng user không
            if (address == null || address.UserId != userId)
            {
                return false;
            }
            if (address.IsDefault)
            {
                throw new ValidationException("Cannot delete the default address. Please set another address as default first.");
            }


            await _unitOfWork.AddressRepository.DeleteAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<AddressDto?> GetAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
        {
            var address = await _unitOfWork.AddressRepository.GetByIdAsync(addressId, cancellationToken);
            if (address == null || address.UserId != userId)
            {
                return null;
            }
            return _mapper.Map<AddressDto>(address);
        }

        public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var addresses = await _unitOfWork.AddressRepository.GetAddressesByUserIdAsync(userId, cancellationToken);
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }

        public async Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
        {
            var addressToSet = await _unitOfWork.AddressRepository.GetByIdAsync(addressId, cancellationToken);
            if (addressToSet == null || addressToSet.UserId != userId)
            {
                return false;
            }

            if (addressToSet.IsDefault)
            {
                return true;
            }

            await _unitOfWork.AddressRepository.UnsetDefaultAddressesAsync(userId, addressId, cancellationToken);
            addressToSet.IsDefault = true;
            await _unitOfWork.AddressRepository.UpdateAsync(addressToSet, cancellationToken); // Chỉ đánh dấu Modified

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto updateAddressDto, CancellationToken cancellationToken = default)
        {
            var addressToUpdate = await _unitOfWork.AddressRepository.GetByIdAsync(addressId, cancellationToken);

            if (addressToUpdate == null || addressToUpdate.UserId != userId)
            {
                return false;
            }

            if (updateAddressDto.IsDefault && !addressToUpdate.IsDefault)
            {
                await _unitOfWork.AddressRepository.UnsetDefaultAddressesAsync(userId, addressId, cancellationToken);
            }

            _mapper.Map(updateAddressDto, addressToUpdate);

            await _unitOfWork.AddressRepository.UpdateAsync(addressToUpdate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}