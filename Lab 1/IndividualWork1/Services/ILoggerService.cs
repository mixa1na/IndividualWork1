namespace IndividualWork1.Services;

public interface ILoggerService : IDisposable
{
    void Info(string message);
    void Error(string message);
    void Success(string message);
    void LogStageResult(string stageName, int exitCode, bool isSuccess);
}