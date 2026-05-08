using System.Net.Http.Json;
using System.Text.Json;
using backend.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public interface IAddressValidationService
{
    Task<ValidatedAddressDto?> ValidateAddressAsync(AddressValidationDto addressDto);
}

public class AddressValidationService : IAddressValidationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AddressValidationService> _logger;
    private const string GoogleAddressValidationUrl = "https://addressvalidation.googleapis.com/v1:validateAddress";

    public AddressValidationService(HttpClient httpClient, IConfiguration configuration, ILogger<AddressValidationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ValidatedAddressDto?> ValidateAddressAsync(AddressValidationDto addressDto)
    {
        try
        {
            var apiKey = _configuration["GoogleMaps:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Google Maps API key not configured, using fallback address");
                return BuildFallbackAddress(addressDto);
            }

            // Build the request payload for Google Address Validation API
            var requestPayload = new
            {
                address = new
                {
                    regionCode = addressDto.Country ?? "US",
                    postalCode = addressDto.PostalCode,
                    administrativeArea = addressDto.Region,
                    locality = addressDto.City,
                    addressLines = new[] { addressDto.Address }
                }
            };

            var url = $"{GoogleAddressValidationUrl}?key={apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, requestPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Google API error: {response.StatusCode} - {errorContent}. Using fallback address.");
                return BuildFallbackAddress(addressDto);
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            if (!doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                _logger.LogWarning("No validation result from Google API, using fallback address");
                return BuildFallbackAddress(addressDto);
            }

            string formattedAddress = string.Empty;
            decimal latitude = 0;
            decimal longitude = 0;

            if (resultElement.TryGetProperty("address", out var addressElement) &&
                addressElement.TryGetProperty("formattedAddress", out var formattedAddressElement) &&
                formattedAddressElement.ValueKind == JsonValueKind.String)
            {
                formattedAddress = formattedAddressElement.GetString() ?? string.Empty;
            }

            if (resultElement.TryGetProperty("geocode", out var geocodeElement) &&
                geocodeElement.TryGetProperty("location", out var locationElement))
            {
                if (locationElement.TryGetProperty("latitude", out var latitudeElement) && latitudeElement.TryGetDecimal(out var lat))
                {
                    latitude = lat;
                }

                if (locationElement.TryGetProperty("longitude", out var longitudeElement) && longitudeElement.TryGetDecimal(out var lng))
                {
                    longitude = lng;
                }
            }

            if (string.IsNullOrWhiteSpace(formattedAddress))
            {
                _logger.LogWarning("No formatted address in response, using fallback address");
                return BuildFallbackAddress(addressDto);
            }

            return new ValidatedAddressDto
            {
                FormattedAddress = formattedAddress,
                Latitude = latitude,
                Longitude = longitude,
                IsFallback = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating address with Google Maps API, using fallback address");
            return BuildFallbackAddress(addressDto);
        }
    }

    private static ValidatedAddressDto BuildFallbackAddress(AddressValidationDto addressDto)
    {
        var formattedAddress = string.Join(", ",
            new[]
            {
                addressDto.Address?.Trim(),
                addressDto.City?.Trim(),
                addressDto.Region?.Trim(),
                addressDto.PostalCode?.Trim(),
                addressDto.Country?.Trim()
            }
            .Where(part => !string.IsNullOrWhiteSpace(part))
        );

        return new ValidatedAddressDto
        {
            FormattedAddress = formattedAddress,
            Latitude = 0,
            Longitude = 0,
            IsFallback = true
        };
    }
}

// Intentionally parsing Google response via JsonDocument to tolerate response shape changes.
