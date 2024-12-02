#define REFERENCE_EXISTS

using System.Diagnostics;
using ExceptionManager;
using Serilog;

namespace TestProject.BLL;

public class ExceptionManagerTest : IDisposable
{
#if REFERENCE_EXISTS
    [Fact]
    public void LogTest()
    {
        // Arrange
        LogConfig.ConfigureLogging();
        
        // Act
        Log.Information("Test log Message");
        Log.Warning("Test Warning Message");
        Log.Error("Test Error Message");
        Log.Fatal("Test Fatal Message");
        
        string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"log{DateTime.Now:yyyyMMdd}.log");
        
        // Assert
        Assert.True(WaitForFile(logFilePath, TimeSpan.FromSeconds(5)), $"Log file does not exist at {logFilePath}");
        
        string logContent = ReadFileWithRetry(logFilePath, maxRetries: 3);
        Assert.Contains("Test log Message", logContent);
        Assert.Contains("Test Warning Message", logContent);
        Assert.Contains("Test Error Message", logContent);
        Assert.Contains("Test Fatal Message", logContent);
        
        // Clean up
        Log.CloseAndFlush();
        
        File.Delete(logFilePath);
    }
    
    private bool WaitForFile(string filePath, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            if (File.Exists(filePath))
            {
                return true;
            }
            Thread.Sleep(100);
        }
        return false;
    }
    
    private string ReadFileWithRetry(string filePath, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (IOException)
            {
                if (i == maxRetries - 1)
                    throw;
                Thread.Sleep(100 * (i + 1));  // 점진적으로 대기 시간 증가
            }
        }
        throw new InvalidOperationException("Unexpected code path");
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
#endif
}