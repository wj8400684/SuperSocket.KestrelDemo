namespace SuperSocket.Kestrel;

public interface IKestrelPipeChannel
{
    ValueTask WaitHandleClosingAsync();
}
