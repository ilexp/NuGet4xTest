using System;
using System.Threading.Tasks;
using NuGet.Common;

namespace Duality.Editor.PackageManagement
{
    public class ConsoleLogger : ILogger
    {
        public void LogDebug(string data)
        {
            Console.WriteLine($"DEBUG: {data}");
        }

        public void LogVerbose(string data)
        {
            Console.WriteLine($"VERBOSE: {data}");
        }

        public void LogInformation(string data)
        {
            Console.WriteLine($"INFO: {data}");
        }

        public void LogMinimal(string data)
        {
            Console.WriteLine($"MIN: {data}");
        }

        public void LogWarning(string data)
        {
            Console.WriteLine($"WARN: {data}");
        }

        public void LogError(string data)
        {
            Console.WriteLine($"ERROR: {data}");
        }

        public void LogInformationSummary(string data)
        {
            Console.WriteLine($"INFO: {data}");
        }

        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    LogDebug(data);
                    break;
                case LogLevel.Verbose:
                    LogVerbose(data);
                    break;
                case LogLevel.Information:
                    LogInformation(data);
                    break;
                case LogLevel.Minimal:
                    LogMinimal(data);
                    break;
                case LogLevel.Warning:
                    LogWarning(data);
                    break;
                case LogLevel.Error:
                    LogError(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public async Task LogAsync(LogLevel level, string data)
        {
            Log(level, data);
            await Task.Yield();
        }

        public void Log(ILogMessage message)
        {
            Log(message.Level, message.Message);
        }

        public async Task LogAsync(ILogMessage message)
        {
            await LogAsync(message.Level, message.Message);
        }
    }
}