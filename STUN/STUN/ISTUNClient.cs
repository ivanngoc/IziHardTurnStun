using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.STUN.STUN
{
    public interface ISTUNClient
    {

    }

    public interface IPipedClient
    {
        Task StartWriterLoop();
        ValueTask<ReadResult> ReadAsync(CancellationToken token = default);
    }

    public interface ITurnClient
    {
        Task ConnectAsync();
    }

    public interface IClientTcp : IClientNetwork
    {

    }

    public interface IClientUdp : IClientNetwork
    {

    }

    public interface IClientNetwork : IClientSocket
    {

    }

    public interface IClientSocket : IClientStream
    {

    }
    public interface IClientStream: IClientPiped
    {
        
    }

    public interface IClientPiped
    {
        ValueTask<ReadResult> ReadAsync();
    }
}