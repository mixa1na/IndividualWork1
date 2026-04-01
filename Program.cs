using IndividualWork1.Services;

namespace IndividualWork1;

class Program
{
    static async Task Main(string[] args)
    {
        // Проверка аргументов
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: IndividualWork1 <config_path> <target_directory>");
            Console.WriteLine("Example: IndividualWork1 pipeline.json C:\\Projects\\MyApp");
            return;
        }

        var configPath = args[0];
        var targetDirectory = args[1];

        // Проверка существования директории
        if (!Directory.Exists(targetDirectory))
        {
            Console.WriteLine($"Error: Directory not found: {targetDirectory}");
            return;
        }

        // Инициализация сервисов
        var workDirName = new DirectoryInfo(targetDirectory).Name;
        using ILoggerService logger = new LoggerService(targetDirectory, workDirName);
        IPipelineService pipeline = new PipelineService(targetDirectory, logger);

        // Запуск пайплайна
        var success = await pipeline.ExecutePipelineAsync(configPath);
        Environment.ExitCode = success ? 0 : 1;

        if (success)
            Console.WriteLine("Pipeline completed successfully");
        else
            Console.WriteLine("Pipeline failed");
    }
}