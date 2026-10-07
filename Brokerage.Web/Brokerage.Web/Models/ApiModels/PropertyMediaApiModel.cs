namespace Brokerage.Web.Models.ApiModels;

public class PropertyMediaApiModel
{
    public int MediaID { get; set; }

    public int PropertyID { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long? FileSizeBytes { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}