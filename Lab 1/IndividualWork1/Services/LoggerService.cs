using System.Text;

namespace IndividualWork1.Services;

public class LoggerService : ILoggerService
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public LoggerService(string targetDir, string workDirName)
    {
        var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        var logFileName = $"CICD_{workDirName}_{timestamp}.log";
        _logFilePath = Path.Combine(targetDir, logFileName);

        Directory.CreateDirectory(targetDir);
        File.WriteAllText(_logFilePath, string.Empty, Encoding.UTF8);

        Info($"Log file created: {_logFilePath}");
    }

    public void Info(string message) => Log("INFO", message);
    public void Error(string message) => Log("ERROR", message);
    public void Success(string message) => Log("SUCCESS", message);

    public void LogStageResult(string stageName, int exitCode, bool isSuccess)
    {
        var status = isSuccess ? "SUCCESS" : "ERROR";
        Log(status, $"Stage '{stageName}' finished with ExitCode {exitCode}");
    }

    private void Log(string level, string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (_lock)
        {
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
            Console.WriteLine(logEntry); // Также выводим в консоль
        }
    }

    public void Dispose() { }
}