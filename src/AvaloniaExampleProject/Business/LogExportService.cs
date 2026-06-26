using System.IO.Compression;
using Darp.Utils.Assets;

namespace AvaloniaExampleProject.Business;

public interface ILogExportService
{
    Task ExportAsync(Stream destinationStream, CancellationToken cancellationToken);
}

public sealed class LogExportService(IAssetsFactory assetService) : ILogExportService
{
    private readonly IReadOnlyAssetsService _appDataService = assetService.GetReadOnlyAssets(
        Bootstrapper.AppDataAssets
    );

    public async Task ExportAsync(Stream destinationStream, CancellationToken cancellationToken)
    {
        string logPath = Path.Join(_appDataService.BasePath, "logs");
        await using var archive = new ZipArchive(destinationStream, ZipArchiveMode.Create, leaveOpen: true);
        if (!Directory.Exists(logPath))
            return;

        foreach (string filePath in Directory.EnumerateFiles(logPath, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddFileAsync(archive, logPath, filePath, cancellationToken);
        }
    }

    private async Task AddFileAsync(
        ZipArchive archive,
        string sourceDirectory,
        string filePath,
        CancellationToken cancellationToken
    )
    {
        string entryName = Path.GetRelativePath(sourceDirectory, filePath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');

        await using var sourceStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 81920,
            useAsync: true
        );
        ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using Stream entryStream = await entry.OpenAsync(cancellationToken);
        await sourceStream.CopyToAsync(entryStream, cancellationToken);
    }
}
