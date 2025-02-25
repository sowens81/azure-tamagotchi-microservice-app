namespace Tamagotchi.Backend.SharedLibrary.Models;

public class SbusMessage<T>
{
    public required string MessageType { get; set; }
    public required string TransactionId { get; set; }
    public required T Payload { get; set; }
}
