using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Options;
using MLD.Application.FileManagement;
using MLD.Application.Settings;
using System.Text;

namespace MLD.Application.Integration_Tests;

public class FileServiceTests : IDisposable
{
    private readonly IFileService _fileService;
    private readonly string _testDirectory;

    public FileServiceTests()
    {
        // Create a temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        // Configure the FileService with the test directory
        var options = Options.Create(new FileServiceOptions { BasePath = _testDirectory });
        _fileService = new FileService(options);
    }

    [Fact]
    public void GetFiles_ShouldReturnAllFilesInDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "file1.txt"), "Test content 1");
        File.WriteAllText(Path.Combine(_testDirectory, "file2.txt"), "Test content 2");

        // Act
        var files = _fileService.GetFiles();

        // Assert
        using (new AssertionScope())
        {
            files.Should().HaveCount(2);
            files.Should().Contain(f => f.Name == "file1.txt");
            files.Should().Contain(f => f.Name == "file2.txt");
        }
    }

    [Fact]
    public void GetFile_ShouldReturnFileStreamForExistingFile()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "file.txt"), "Test content");
        var expectedContent = "Test content";

        // Act
        using (var stream = _fileService.GetFile("file.txt"))
        using (var reader = new StreamReader(stream))
        {
            var actualContent = reader.ReadToEnd();

            // Assert
            actualContent.Should().Be(expectedContent);
        }
    }

    [Fact]
    public void GetFile_ShouldThrowFileNotFoundExceptionForNonExistingFile()
    {
        // Arrange

        // Act & Assert
        Action action = () => _fileService.GetFile("nonexisting.txt");
        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void UploadFile_ShouldCreateNewFile()
    {
        // Arrange
        var fileName = "uploaded.txt";
        var fileContent = "Uploaded test content";
        var formFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)), 0, fileContent.Length, fileName, fileName);

        // Act
        _fileService.UploadFile(formFile);

        // Assert
        var filePath = Path.Combine(_testDirectory, fileName);
        using (new AssertionScope())
        {
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Be(fileContent);
        }
    }

    [Fact]
    public void DeleteFile_ShouldDeleteExistingFile()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "file.txt"), "Test content");

        // Act
        _fileService.DeleteFile("file.txt");

        // Assert
        var filePath = Path.Combine(_testDirectory, "file.txt");
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public void DeleteFile_ShouldThrowFileNotFoundExceptionForNonExistingFile()
    {
        // Arrange

        // Act & Assert
        Action action = () => _fileService.DeleteFile("nonexisting.txt");
        action.Should().Throw<FileNotFoundException>();
    }

    // Clean up the temporary test directory after all tests have executed
    public void Dispose()
    {
        Directory.Delete(_testDirectory, true);
    }
}