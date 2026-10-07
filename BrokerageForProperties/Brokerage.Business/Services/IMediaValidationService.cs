using Microsoft.AspNetCore.Http;

namespace Brokerage.Business.Services;

public interface IMediaValidationService
{
    string? ValidateImage(
        IFormFile file,
        long maxSize);

    string? ValidateVideo(
        IFormFile file,
        long maxSize);

    string? ValidateDocument(
        IFormFile file,
        long maxSize);
}