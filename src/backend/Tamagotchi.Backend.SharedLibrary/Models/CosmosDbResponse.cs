namespace Tamagotchi.Backend.SharedLibrary.Models;

public class CosmosDbResponse<T>
{
    public bool IsSuccess { get; set;}
    public T? Entity { get; set;}
    public string? Message  { get; set;}

    public Exception? Exception { get; set;}
}
