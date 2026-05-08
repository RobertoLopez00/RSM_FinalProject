using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class AddressesController : ControllerBase
{
    private readonly IAddressValidationService _addressValidationService;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(IAddressValidationService addressValidationService, ILogger<AddressesController> logger)
    {
        _addressValidationService = addressValidationService;
        _logger = logger;
    }

    /// <summary>
    /// Validate and geocode an address using Google Maps Address Validation API
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidatedAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidatedAddressDto>> ValidateAddress([FromBody] AddressValidationDto addressDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(addressDto.Address) || string.IsNullOrWhiteSpace(addressDto.City))
                return BadRequest("Address and City are required fields");

            var validatedAddress = await _addressValidationService.ValidateAddressAsync(addressDto);
            
            if (validatedAddress == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Address validation failed. Please try again.");

            return Ok(validatedAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating address");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error validating address");
        }
    }
}
