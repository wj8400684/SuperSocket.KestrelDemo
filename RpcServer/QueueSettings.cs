using SuperSocket.Kestrel.IOCP;
using System.Buffers;
using System.IO.Pipelines;

namespace Server;

internal class QueueSettings
{
    public required PipeScheduler Scheduler { get; init; }

    public required PipeOptions InputOptions { get; init; }

    public required PipeOptions OutputOptions { get; init; }

    public required SocketSenderPool SocketSenderPool { get; init; }

    public required MemoryPool<byte> MemoryPool { get; init; }
}

