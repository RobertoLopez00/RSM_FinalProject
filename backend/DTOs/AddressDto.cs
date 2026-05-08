namespace backend.DTOs;

public class AddressValidationDto
{
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

public class ValidatedAddressDto
{
    public string FormattedAddress { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsFallback { get; set; }
}
