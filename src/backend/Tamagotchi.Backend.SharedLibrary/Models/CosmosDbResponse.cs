namespace Tamagotchi.Backend.SharedLibrary.Models;

public class CosmosDbResponse<T>
{
    public bool IsSuccess { get; set; }
    public int ResponseCode { get; set; }
    public T? Entity { get; set; }
    public Exception? Exception { get; set; }
}
