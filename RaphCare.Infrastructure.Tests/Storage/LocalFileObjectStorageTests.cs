using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using RaphCare.Infrastructure.Storage;
using Xunit;

namespace RaphCare.Infrastructure.Tests.Storage;

public sealed class LocalFileObjectStorageTests
{
    [Fact]
    public async Task SaveReadAndDeleteByPrefixRoundTripBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), "raphcare-object-store-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var storage = new LocalFileObjectStorage(new FakeHost(root, Environments.Development));
            await using var payload = new MemoryStream([10, 20, 30, 40]);
            await storage.SaveAsync("patient-photos", "abc123.jpg", payload, "image/jpeg", CancellationToken.None);

            var read = await storage.OpenReadAsync("patient-photos", "abc123.jpg", CancellationToken.None);
            Assert.NotNull(read);
            await using (read!.Content)
            {
                using var copy = new MemoryStream();
                await read.Content.CopyToAsync(copy);
                Assert.Equal(new byte[] { 10, 20, 30, 40 }, copy.ToArray());
            }

            Assert.Equal("image/jpeg", read.ContentType);

            await storage.DeleteByPrefixAsync("patient-photos", "abc123", CancellationToken.None);
            var missing = await storage.OpenReadAsync("patient-photos", "abc123.jpg", CancellationToken.None);
            Assert.Null(missing);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ConstructorThrowsOutsideDevelopment()
    {
        var root = Path.Combine(Path.GetTempPath(), "raphcare-object-store-tests", Guid.NewGuid().ToString("N"));
        Assert.Throws<InvalidOperationException>(() =>
            new LocalFileObjectStorage(new FakeHost(root, Environments.Staging)));
    }
}

file sealed class FakeHost(string contentRoot, string environmentName) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;
    public string ApplicationName { get; set; } = "tests";
    public string ContentRootPath { get; set; } = contentRoot;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
