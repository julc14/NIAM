namespace NameItAfterMe.Application.Infrastructure.Files;

public class ImageMetadata
{
    public string Name { get; }
    public string Url { get; }
    public DateOnly DownloadDate { get; }
    public string LocalRootPath { get; }

    public ImageMetadata(string name, string url, DateOnly downloadDate, string localRootPath)
    {
        Name = name;
        Url = url;
        DownloadDate = downloadDate;
        LocalRootPath = localRootPath;
    }
}