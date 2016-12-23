using ChessVariantsTraining.MemoryRepositories.Variant960;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Models.Variant960
{
    public class LobbySocketHandler : IDisposable
    {
        WebSocket ws;
        CancellationToken ct = new CancellationToken(false);
        int? clientUser;
        string clientId;
        ILobbySocketHandlerRepository handlerRepository;
        ILobbySeekRepository seekRepository;

        public bool Closed
        {
            get
            {
                return ws.State == WebSocketState.Closed;
            }
        }
        public bool Open
        {
            get
            {
                return ws.State == WebSocketState.Open;
            }
        }

        public LobbySocketHandler(WebSocket socket, int? _clientUser, string _clientId, ILobbySocketHandlerRepository _handlerRepository, ILobbySeekRepository _seekRepository)
        {
            ws = socket;
            clientUser = _clientUser;
            handlerRepository = _handlerRepository;
            seekRepository = _seekRepository;
            clientId = _clientId;
        }

        public async Task LobbyLoop()
        {
            byte[] buffer = new byte[4096];
            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                byte[] message = new byte[result.Count];
                Array.Copy(buffer, message, result.Count);
                buffer = new byte[4096];
                await HandleReceived(Encoding.ASCII.GetString(message));
            }
        }

        async Task HandleReceived(string text)
        {
            SocketMessage message = new SocketMessage(text);
            if (!message.Okay)
            {
                await Send("{\"t\":\"error\",\"d:\":\"invalid message\"}");
                return;
            }
            switch (message.Type)
            {
                case "create":
                    LobbySeek seek;
                    bool isValid = LobbySeek.TryParse(message.Data, clientUser, clientId, out seek);
                    if (!isValid)
                    {
                        await Send("{\"t\":\"error\",\"d:\":\"invalid seek\"}");
                        return;
                    }
                    string seekId = await seekRepository.Add(seek);
                    await Send("{\"t\":\"ack\",\"d:\":\"" + seekId + "\"}");
                    break;
                case "remove":
                    await seekRepository.Remove(message.Data, clientUser, clientId);
                    break;
                case "bump":
                    seekRepository.Bump(message.Data, clientUser, clientId);
                    break;
                default:
                    await Send("{\"t\":\"error\",\"d:\":\"invalid message\"}");
                    break;
            }
        }

        public async Task Send(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            int length = buffer.Length;
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
            await ws.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        public void Dispose()
        {
            ws.Dispose();
        }
    }
}
