using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Application.Infrastructure.Files;

// TODO: Writing to disk is not great for clould-based hosting
// switch to blob storage later
internal class StaticImageHandler : IImageHandler
{
    private static readonly string[] _imageExtentions = new string[] { ".jpg", ".jpeg", ".png", ".jfif" };
    private List<ImageMetadata>? _cache;

    public virtual string LocalFolder { get; set; } = "Images";
    public string SearchPath => Path.Join("wwwroot", LocalFolder);

    /// <inheritdoc/>
    public bool TrySearch(string fileName, [MaybeNullWhen(false)] out ImageMetadata image)
    {
        VerifySearchPathExists();
        fileName = CheckParsePattern(fileName);

        image = EnumerateImages(fileName).OrderByDescending(x => x.DownloadDate).FirstOrDefault();
        return image is not null;
    }
    public IEnumerable<ImageMetadata> EnumerateImages(string? searchPattern = null)
    {
        VerifySearchPathExists();
        searchPattern = CheckParsePattern(searchPattern);

        var images =
            from filePath in Directory.EnumerateFiles(SearchPath, searchPattern, SearchOption.TopDirectoryOnly)
            let fileInfo = new FileInfo(filePath)
            where _imageExtentions.Contains(fileInfo.Extension, StringComparer.OrdinalIgnoreCase)
            select new ImageMetadata(
                fileInfo.Name,
                fileInfo.FullName,
                fileInfo.LastWriteTimeUtc.AsDateOnly(),
                Path.Join(LocalFolder, fileInfo.Name).Replace(@"\", "/"));

        _cache ??= images.ToList();

        return _cache;
    }

    /// <inheritdoc/>
    public async Task<ImageMetadata> SaveAsync(string name, Func<Task<Stream>> getStreamContent)
    {
        VerifySearchPathExists();

        var uniqueName = GetUniqueFileName(name);
        var imagePath = Path.Join(SearchPath, uniqueName);

        await using var fileStream = new FileStream(imagePath, FileMode.Create);
        await using var stream = await getStreamContent();
        await stream.CopyToAsync(fileStream);

        var image = new ImageMetadata(
            uniqueName,
            "",
            DateTime.UtcNow.AsDateOnly(),
            Path.Join(LocalFolder, uniqueName).Replace(@"\", "/"));

        _cache?.Add(image);

        return image;
    }

    private void VerifySearchPathExists()
    {
        if (!Directory.Exists(SearchPath))
        {
            Directory.CreateDirectory(SearchPath);
        }
    }

    private static string CheckParsePattern(string? pattern) => pattern switch
    {
        null => "*",
        var s when !s.Contains('*') => $"*{pattern}*",
        _ => pattern
    };

    private static string GetUniqueFileName(string name) => name + Guid.NewGuid() + ".jpg";
}
