namespace Tamagotchi.Backend.SharedLibrary.Logging;

public interface ISuperLogger<T>
{
    void LogInformation(string message, string transactionId, params object[] args);

    void LogWarning(string message, string transactionId, params object[] args);

    void LogError(Exception exception, string message, string transactionId, params object[] args);

    void LogDebug(string message, string transactionId, params object[] args);

    void LogCritical(Exception exception, string transactionId, string message, params object[] args);
}
