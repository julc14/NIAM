using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Application.Infrastructure.Files;

// TODO: Writing to disk is not great for clould-based hosting
// switch to blob storage later
internal class ImageHandler : IImageHandler
{
    private readonly string _root = "wwwroot";
    public virtual string LocalFolder { get; set; } = "Images";

    public string SearchPath => Path.Join(_root, LocalFolder);

    /// <inheritdoc/>
    public bool TrySearch(string fileName, [MaybeNullWhen(false)] out ImageMetadata image)
    {
        var fileSearch =
            from filePath in Directory.EnumerateFiles(SearchPath, $"*{fileName}*", SearchOption.TopDirectoryOnly)
            let fileInfo = new FileInfo(filePath)
            orderby fileInfo.LastWriteTimeUtc descending
            select new ImageMetadata(
                fileInfo.Name,
                fileInfo.FullName,
                DateOnly.FromDateTime(fileInfo.LastAccessTimeUtc),
                Path.Join(LocalFolder, fileInfo.Name).Replace(@"\", "/"));

        image = fileSearch.FirstOrDefault();

        return image is not null;
    }

    /// <inheritdoc/>
    public async Task<ImageMetadata> SaveAsync(string name, Func<Task<Stream>> getStreamContent)
    {
        var uniqueName = GetUniqueFileName(name);
        var imagePath = Path.Join(SearchPath, uniqueName);

        await using var fileStream = new FileStream(imagePath, FileMode.Create);
        await using var stream = await getStreamContent();
        await stream.CopyToAsync(fileStream);

        return new ImageMetadata(
            uniqueName,
            "",
            DateOnly.FromDateTime(DateTime.UtcNow),
            Path.Join(LocalFolder, uniqueName).Replace(@"\", "/"));
    }

    private static string GetUniqueFileName(string name) => name + Guid.NewGuid() + ".jpg";
}
