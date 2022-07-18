using System.Diagnostics.CodeAnalysis;

namespace NameItAfterMe.Application.Abstractions;

public interface IImageHandler
{
    bool TrySearch(string fileName, [MaybeNullWhen(false)] out string filePath);
    Task<string> SaveAsync(string name, Stream content);
}
