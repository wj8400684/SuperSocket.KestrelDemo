using SuperSocket.Channel;
using SuperSocket.Kestrel.IOCP;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.IOCPTcpChannel;

public class IOCPTcpPipeChannel<TPackageInfo> : TcpPipeChannel<TPackageInfo>
{
    private Socket? _socket;
    private SocketSender? _sender;
    private readonly bool _waitForData;
    private readonly SocketReceiver _receiver;
    private readonly SocketSenderPool _socketSenderPool;
    private readonly CancellationTokenSource _cts;

    public IOCPTcpPipeChannel(Socket socket,
                              IPipelineFilter<TPackageInfo> pipelineFilter,
                              ChannelOptions options,
                              SocketSenderPool socketSenderPool,
                              PipeScheduler? socketScheduler = default,
                              bool waitForData = true) :
        base(socket, pipelineFilter, options)
    {
        socketScheduler ??= PipeScheduler.ThreadPool;

        _socket = socket;
        _waitForData = waitForData;
        _receiver = new SocketReceiver(socketScheduler);
        _socketSenderPool = socketSenderPool;

        const string fieldName = "_cts";

        var field = GetType().BaseType!.BaseType!.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        _cts = (CancellationTokenSource)field!.GetValue(this)!;
    }

    public override async ValueTask CloseAsync(CloseReason closeReason)
    {
        base.Close();

        await base.CloseAsync(closeReason);
    }

    /// <summary>
    /// 从socket中接受数据流然后写入memory
    /// </summary>
    /// <param name="memory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //if (_waitForData)
        //{
        //    // Wait for data before allocating a buffer.
        //    var waitForDataResult = await _receiver.WaitForDataAsync(_socket!).ConfigureAwait(false);

        //    if (waitForDataResult.HasError)
        //        throw waitForDataResult.SocketError;
        //}

        //var receiveResult = await _receiver.ReceiveAsync(_socket!, memory).ConfigureAwait(false);

        //if (receiveResult.HasError)
        //    throw receiveResult.SocketError;

        //return receiveResult.BytesTransferred;
    }

    /// <summary>
    /// 发送数据至socket
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //_sender = _socketSenderPool.Rent();

        //var transferResult = await _sender.SendAsync(_socket!, buffer).ConfigureAwait(false);

        //if (transferResult.HasError)
        //{
        //    if (IsConnectionResetError(transferResult.SocketError.SocketErrorCode))
        //        throw transferResult.SocketError;

        //    if (IsConnectionAbortError(transferResult.SocketError.SocketErrorCode))
        //        throw transferResult.SocketError;
        //}

        //_socketSenderPool.Return(_sender);

        //_sender = null;

        //return transferResult.BytesTransferred;
    }

    /// <summary>
    /// 从pipeline中读取数据然后发送至socket
    /// </summary>
    /// <returns></returns>
    protected override async Task ProcessSends()
    {
        var output = Out.Reader;
        var socket = _socket!;

        while (true)
        {
            var result = await output.ReadAsync().ConfigureAwait(false);

            var completed = result.IsCompleted;

            var buffer = result.Buffer;
            var end = buffer.End;

            if (!buffer.IsEmpty)
            {
                try
                {
                    _sender = _socketSenderPool.Rent();

                    var transferResult = await _sender.SendAsync(socket, buffer).ConfigureAwait(false);

                    if (transferResult.HasError)
                    {
                        if (IsConnectionResetError(transferResult.SocketError.SocketErrorCode))
                            throw transferResult.SocketError;

                        if (IsConnectionAbortError(transferResult.SocketError.SocketErrorCode))
                            throw transferResult.SocketError;
                    }

                    _socketSenderPool.Return(_sender);

                    _sender = null;

                    LastActiveTime = DateTimeOffset.Now;
                }
                catch (Exception e)
                {
                    _cts.Cancel(false);

                    if (!IsIgnorableException(e))
                        OnError("Exception happened in SendAsync", e);

                    break;
                }
            }

            output.AdvanceTo(end);

            if (completed)
                break;
        }

        output.Complete();
    }

    /// <summary>
    /// 从socket中读取数据流然后写入 pipeline
    /// </summary>
    /// <param name="writer"></param>
    /// <returns></returns>
    protected override async Task FillPipeAsync(PipeWriter writer)
    {
        var cts = _cts;
        var socket = _socket!;

        while (!cts.IsCancellationRequested)
        {
            try
            {
                var bufferSize = Options.ReceiveBufferSize;

                if (bufferSize <= 0)
                    bufferSize = 1024 * 4; //4k

                var memory = writer.GetMemory(bufferSize);

                if (_waitForData)
                {
                    // Wait for data before allocating a buffer.
                    var waitForDataResult = await _receiver.WaitForDataAsync(socket).ConfigureAwait(false);

                    if (waitForDataResult.HasError)
                        throw waitForDataResult.SocketError;
                }

                var receiveResult = await _receiver.ReceiveAsync(socket, memory).ConfigureAwait(false);

                if (receiveResult.HasError)
                    throw receiveResult.SocketError;

                var bytesRead = receiveResult.BytesTransferred;

                if (bytesRead == 0)
                {
                    if (!CloseReason.HasValue)
                        CloseReason = Channel.CloseReason.RemoteClosing;

                    break;
                }

                LastActiveTime = DateTimeOffset.Now;

                // Tell the PipeWriter how much was read
                writer.Advance(bytesRead);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                {
                    if (e is not OperationCanceledException)
                        OnError("Exception happened in ReceiveAsync", e);

                    if (!CloseReason.HasValue)
                    {
                        CloseReason = cts.IsCancellationRequested
                            ? Channel.CloseReason.LocalClosing : Channel.CloseReason.SocketError;
                    }
                }
                else if (!CloseReason.HasValue)
                {
                    CloseReason = Channel.CloseReason.RemoteClosing;
                }

                break;
            }

            // Make the data available to the PipeReader
            var result = await writer.FlushAsync().ConfigureAwait(false);

            if (result.IsCompleted)
                break;
        }

        // Signal to the reader that we're done writing
        await writer.CompleteAsync().ConfigureAwait(false);
        // And don't allow writing data to outgoing pipeline
        await Out.Writer.CompleteAsync().ConfigureAwait(false);
    }

    protected override void OnClosed()
    {
        _cts.Dispose();
        _socket?.Dispose();
        _sender?.Dispose();
        _receiver.Dispose();
        base.OnClosed();
    }

    private static bool IsConnectionResetError(SocketError errorCode)
    {
        return errorCode == SocketError.ConnectionReset ||
               errorCode == SocketError.Shutdown ||
               (errorCode == SocketError.ConnectionAborted && OperatingSystem.IsWindows());
    }

    private static bool IsConnectionAbortError(SocketError errorCode)
    {
        // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
        return errorCode == SocketError.OperationAborted ||
               errorCode == SocketError.Interrupted ||
               (errorCode == SocketError.InvalidArgument && !OperatingSystem.IsWindows());
    }
}