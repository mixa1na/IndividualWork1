using IndividualWork1.Models;

namespace IndividualWork1.Services;

public interface IPipelineService
{
    Task<bool> ExecutePipelineAsync(string configPath);
}