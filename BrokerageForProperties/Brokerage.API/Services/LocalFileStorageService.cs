using Brokerage.Business.Services;

namespace Brokerage.API.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string folder)
    {
        string uploadFolder =
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                folder);

        Directory.CreateDirectory(uploadFolder);

        string safeFileName =
           Path.GetFileName(fileName);

        string uniqueFileName =
            $"{Guid.NewGuid()}_{safeFileName}";

        string fullPath =
            Path.Combine(
                uploadFolder,
                uniqueFileName);

        using FileStream outputStream =
            new FileStream(
                fullPath,
                FileMode.Create);

        await fileStream.CopyToAsync(
            outputStream);

        return $"/uploads/{folder}/{uniqueFileName}";
    }
}