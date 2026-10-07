using System.ComponentModel.DataAnnotations;

namespace Brokerage.Models.DTOs.Properties;

public class AddPropertyAmenitiesRequest
{
    [Required]
    public List<int> AmenityIDs { get; set; } = [];
}