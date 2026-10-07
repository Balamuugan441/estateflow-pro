using Brokerage.Web.Models.ApiModels;

namespace Brokerage.Web.Models.ApiResponses;

public class PropertyReviewApiResponse
{
    public PropertyApiModel? Property { get; set; }

    public List<AmenityApiModel> Amenities { get; set; } = [];

    public List<PropertyMediaApiModel> Media { get; set; } = [];
}