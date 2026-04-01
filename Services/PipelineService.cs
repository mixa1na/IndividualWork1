using IndividualWork1.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace IndividualWork1.Services;

public class PipelineService : IPipelineService
{
    private readonly string _workingDirectory;
    private readonly ILoggerService _logger;

    public PipelineService(string targetDirectory, ILoggerService logger)
    {
        _workingDirectory = targetDirectory;
        _logger = logger;
    }

    public async Task<bool> ExecutePipelineAsync(string configPath)
    {
        try
        {
            // 1. Загрузка конфигурации
            var config = await LoadConfigurationAsync(configPath);
            if (config?.Stages == null || !config.Stages.Any())
            {
                _logger.Error("No pipeline stages configured");
                return false;
            }

            _logger.Info($"Loaded {config.Stages.Count} stages from configuration");

            // 2. Выполнение этапов
            foreach (var stage in config.Stages)
            {
                _logger.Info($"Starting stage: {stage.Name} ({stage.Command} {stage.Arguments})");

                var exitCode = await ExecuteCommandAsync(stage.Command, stage.Arguments);
                var isSuccess = exitCode == 0;

                _logger.LogStageResult(stage.Name, exitCode, isSuccess);

                // 3. Проверка необходимости остановки
                if (!isSuccess && stage.StopOnFailure)
                {
                    _logger.Error($"Critical stage failed. Stopping pipeline.");
                    return false;
                }
            }

            _logger.Success("Pipeline completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Pipeline execution failed: {ex.Message}");
            return false;
        }
    }

    // Публичный метод для тестирования логики продолжения
    public bool ShouldContinue(bool stageSuccess, bool stopOnFailure)
    {
        return stageSuccess || !stopOnFailure;
    }

    // Публичный статический метод для тестирования генерации имени лога
    public static string GenerateLogFileName(string targetDir, string workDirName)
    {
        var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        return Path.Combine(targetDir, $"CICD_{workDirName}_{timestamp}.log");
    }

    // Внутренние методы (можно сделать public virtual для тестов)
    protected virtual async Task<PipelineConfig?> LoadConfigurationAsync(string configPath)
    {
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"Configuration file not found: {configPath}");

        var json = await File.ReadAllTextAsync(configPath);
        return JsonSerializer.Deserialize<PipelineConfig>(json);
    }

    protected virtual async Task<int> ExecuteCommandAsync(string command, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };

            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            // Выводим результат в консоль для наглядности
            if (output.Length > 0) Console.WriteLine(output.ToString());
            if (error.Length > 0) Console.Error.WriteLine(error.ToString());

            return process.ExitCode;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to execute command: {ex.Message}");
            return -1;
        }
    }
}