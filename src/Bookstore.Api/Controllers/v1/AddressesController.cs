// src/Bookstore.Api/Controllers/v1/AddressesController.cs
using Bookstore.Application.Dtos.Addresses;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/user/addresses")]
    [ApiVersion("1.0")]
    [Authorize]
    public class AddressesController : BaseApiController
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
        }

        // GET: api/v1/user/addresses
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetAddresses(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var addresses = await _addressService.GetUserAddressesAsync(userId, cancellationToken);
            return Ok(addresses);
        }

        // GET: api/v1/user/addresses/{addressId}
        [HttpGet("{addressId:guid}")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AddressDto>> GetAddress(Guid addressId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var address = await _addressService.GetAddressByIdAsync(userId, addressId, cancellationToken);
            if (address == null)
            {
                return NotFound();
            }
            return Ok(address);
        }

        // POST: api/v1/user/addresses
        [HttpPost]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressDto createAddressDto, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();

            var createdAddress = await _addressService.CreateAddressAsync(userId, createAddressDto, cancellationToken);
            // Trả về 201 Created với action để lấy địa chỉ vừa tạo
            return CreatedAtAction(nameof(GetAddress), new { addressId = createdAddress.Id, version = "1.0" }, createdAddress);
        }

        // PUT: api/v1/user/addresses/{addressId}
        [HttpPut("{addressId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] UpdateAddressDto updateAddressDto, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var success = await _addressService.UpdateAddressAsync(userId, addressId, updateAddressDto, cancellationToken);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/v1/user/addresses/{addressId}
        [HttpDelete("{addressId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAddress(Guid addressId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            try
            {
                var success = await _addressService.DeleteAddressAsync(userId, addressId, cancellationToken);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log lỗi
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the address.");
            }
        }

        // POST: api/v1/user/addresses/{addressId}/setdefault
        [HttpPost("{addressId:guid}/setdefault")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetDefaultAddress(Guid addressId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var success = await _addressService.SetDefaultAddressAsync(userId, addressId, cancellationToken);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}