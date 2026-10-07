using Brokerage.Business.Services;

namespace Brokerage.API.Services;

public class MediaValidationService : IMediaValidationService
{
    public string? ValidateImage(
        IFormFile file,
        long maxSize)
    {
        if (file.Length == 0)
        {
            return "Image file is empty.";
        }

        if (file.Length > maxSize)
        {
            return "Image size exceeds the allowed limit.";
        }

        string extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        string[] allowedExtensions =
        [
            ".jpg",
            ".jpeg",
            ".png"
        ];

        if (!allowedExtensions.Contains(extension))
        {
            return "Only JPG, JPEG and PNG images are allowed.";
        }

        return null;
    }

    public string? ValidateVideo(
        IFormFile file,
        long maxSize)
    {
        if (file.Length == 0)
        {
            return "Video file is empty.";
        }

        if (file.Length > maxSize)
        {
            return "Video size exceeds the allowed limit.";
        }

        string extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (extension != ".mp4")
        {
            return "Only MP4 videos are allowed.";
        }

        return null;
    }

    public string? ValidateDocument(
        IFormFile file,
        long maxSize)
    {
        if (file.Length == 0)
        {
            return "Document file is empty.";
        }

        if (file.Length > maxSize)
        {
            return "Document size exceeds the allowed limit.";
        }

        string extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        string[] allowedExtensions =
        [
            ".pdf",
            ".doc",
            ".docx"
        ];

        if (!allowedExtensions.Contains(extension))
        {
            return "Only PDF, DOC and DOCX documents are allowed.";
        }

        return null;
    }
}