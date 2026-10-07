using Microsoft.AspNetCore.Http;

namespace Brokerage.Models.DTOs.Properties;

public class MediaUploadRequest
{
    public IFormFile? CoverPhoto { get; set; }

    public List<IFormFile> GalleryImages { get; set; } = [];

    public List<IFormFile> Videos { get; set; } = [];

    public List<IFormFile> FloorPlans { get; set; } = [];

    public List<IFormFile> Documents { get; set; } = [];
    public List<int> RemovedMediaIds { get; set; } = [];

    public List<int> GalleryReplacementIds { get; set; } = [];

    public List<int> VideoReplacementIds { get; set; } = [];

    public List<int> FloorPlanReplacementIds { get; set; } = [];

    public List<int> DocumentReplacementIds { get; set; } = [];
}