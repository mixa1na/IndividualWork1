using IndividualWork1.Models;
using IndividualWork1.Services;
using Moq;
using Xunit;

namespace IndividualWork1.Tests;

public class PipelineServiceTests
{
    [Fact]
    public void GenerateLogFileName_ShouldReturnCorrectFormat()
    {
        var targetDir = "C:\\temp\\test";
        var workDirName = "MyProject";

        var logPath = PipelineService.GenerateLogFileName(targetDir, workDirName);

        Assert.StartsWith(targetDir, logPath);
        Assert.Contains("CICD_MyProject_", logPath);
        Assert.EndsWith(".log", logPath);
        Assert.Contains(DateTime.Now.Year.ToString(), logPath);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void ShouldContinue_DependingOnExitCodeAndSettings_ReturnsCorrect(bool stageSuccess, bool stopOnFailure, bool expected)
    {
        var mockLogger = new Mock<ILoggerService>();
        var service = new PipelineService("C:\\temp", mockLogger.Object);

        var result = service.ShouldContinue(stageSuccess, stopOnFailure);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task LoadConfig_ShouldDeserializeFullConfigCorrectly()
    {
        var json = @"{
            ""pipeline"": [
                {
                    ""name"": ""Build"",
                    ""command"": ""dotnet"",
                    ""args"": ""build"",
                    ""stopOnFailure"": true
                },
                {
                    ""name"": ""Test"",
                    ""command"": ""dotnet"",
                    ""args"": ""test"",
                    ""stopOnFailure"": false
                }
            ]
        }";

        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, json);

        var mockLogger = new Mock<ILoggerService>();
        var service = new TestablePipelineService("C:\\temp", mockLogger.Object);

        var config = await service.LoadConfigurationAsync(tempFile);

        Assert.NotNull(config);
        Assert.Equal(2, config.Stages.Count);
        Assert.Equal("Build", config.Stages[0].Name);
        Assert.Equal("dotnet", config.Stages[0].Command);
        Assert.True(config.Stages[0].StopOnFailure);
        Assert.Equal("Test", config.Stages[1].Name);
        Assert.False(config.Stages[1].StopOnFailure);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task LoadConfig_WithEmptyPipeline_ReturnsEmptyList()
    {
        var json = @"{ ""pipeline"": [] }";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, json);

        var mockLogger = new Mock<ILoggerService>();
        var service = new TestablePipelineService("C:\\temp", mockLogger.Object);

        var config = await service.LoadConfigurationAsync(tempFile);

        Assert.NotNull(config);
        Assert.Empty(config.Stages);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task LoadConfig_WithMissingFields_UsesDefaultValues()
    {
        var json = @"{ ""pipeline"": [{ ""name"": ""Build"", ""command"": ""dotnet"" }] }";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, json);

        var mockLogger = new Mock<ILoggerService>();
        var service = new TestablePipelineService("C:\\temp", mockLogger.Object);

        var config = await service.LoadConfigurationAsync(tempFile);

        Assert.NotNull(config);
        Assert.Single(config.Stages);
        Assert.Equal("Build", config.Stages[0].Name);
        Assert.Equal("dotnet", config.Stages[0].Command);
        Assert.Equal(string.Empty, config.Stages[0].Arguments);
        Assert.True(config.Stages[0].StopOnFailure);

        File.Delete(tempFile);
    }

    private class TestablePipelineService : PipelineService
    {
        public TestablePipelineService(string targetDir, ILoggerService logger)
            : base(targetDir, logger) { }

        public new Task<PipelineConfig?> LoadConfigurationAsync(string configPath)
            => base.LoadConfigurationAsync(configPath);
    }
}