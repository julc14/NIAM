using NameItAfterMe.Application.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Infrastructure;

internal class ImageHandler : IImageHandler
{
    private readonly string _root = "wwwroot/Images";
    private readonly TimeSpan _imageExpiresAfter = TimeSpan.FromHours(24);

    public async Task<string> SaveAsync(string name, Stream content)
    {
        static string GetDateTag() => DateTime.UtcNow.ToShortDateString().Replace("/", "-");

        var localName = name + "-" + GetDateTag() + ".jpg";
        var imagePath = Path.Join(_root, localName);

        var fileStream = new FileStream(imagePath, FileMode.Create);
        await content.CopyToAsync(fileStream);

        return "Images/" + localName;
    }

    public bool TrySearch(string fileName, [MaybeNullWhen(false)] out string path)
    {
        path = null;

        var fileSearch =
            from filePath in Directory.EnumerateFiles(_root, $"*{fileName}*", SearchOption.TopDirectoryOnly)
            let fileInfo = new FileInfo(filePath)
            orderby fileInfo.LastWriteTimeUtc descending
            select fileInfo;

        var firstMatchingFile = fileSearch.FirstOrDefault();

        if (firstMatchingFile is null
            || DateTime.UtcNow.Subtract(firstMatchingFile.LastAccessTimeUtc) > _imageExpiresAfter)
        {
            return false;
        }

        path = "Images/" + firstMatchingFile.Name;
        return true;
    }
}
