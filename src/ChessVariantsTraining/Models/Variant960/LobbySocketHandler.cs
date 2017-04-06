using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.MemoryRepositories.Variant960;
using ChessVariantsTraining.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Models.Variant960
{
    public class LobbySocketHandler : IDisposable
    {
        WebSocket ws;
        GamePlayer client;
        ILobbySocketHandlerRepository handlerRepository;
        ILobbySeekRepository seekRepository;
        IGameRepository gameRepository;
        IRandomProvider randomProvider;
        IUserRepository userRepository;

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
        public GamePlayer Client
        {
            get
            {
                return client;
            }
        }
        public bool Disposed { get; private set; }

        public LobbySocketHandler(WebSocket socket, GamePlayer _client, ILobbySocketHandlerRepository _handlerRepository, ILobbySeekRepository _seekRepository, IGameRepository _gameRepository, IRandomProvider _randomProvider, IUserRepository _userRepository)
        {
            ws = socket;
            client = _client;
            handlerRepository = _handlerRepository;
            seekRepository = _seekRepository;
            gameRepository = _gameRepository;
            randomProvider = _randomProvider;
            userRepository = _userRepository;
            Disposed = false;
        }

        public async Task LobbyLoop()
        {
            byte[] buffer = new byte[4096];
            while (ws.State == WebSocketState.Open && !Disposed)
            {
                WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType != WebSocketMessageType.Close)
                {
                    byte[] message = new byte[result.Count];
                    Array.Copy(buffer, message, result.Count);
                    buffer = new byte[4096];
                    await HandleReceived(Encoding.ASCII.GetString(message));
                }
                else
                {
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "close requested", CancellationToken.None);
                    Dispose();
                }
            }
        }

        async Task HandleReceived(string text)
        {
            SocketMessage message = new SocketMessage(text);
            if (!message.Okay)
            {
                await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                return;
            }
            switch (message.Type)
            {
                case "create":
                    LobbySeek seek;
                    bool isValid = LobbySeek.TryParse(message.Data, client, out seek);
                    if (!isValid)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid seek\"}");
                        return;
                    }
                    string seekId = await seekRepository.Add(seek);
                    await Send("{\"t\":\"ack\",\"d\":\"" + seekId + "\"}");
                    break;
                case "remove":
                    await seekRepository.Remove(message.Data, client);
                    break;
                case "bump":
                    seekRepository.Bump(message.Data, client);
                    break;
                case "join":
                    LobbySeek joined = seekRepository.Get(message.Data);
                    if (joined == null)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"seek does not exist\"}");
                        return;
                    }
                    await seekRepository.Remove(joined.ID, joined.Owner);
                    bool hostIsWhite = randomProvider.RandomBool();
                    int nWhite;
                    int nBlack;
                    int max = joined.Variant != "RacingKings" ? 960 : 1440;
                    nWhite = randomProvider.RandomPositiveInt(max);
                    if (joined.Symmetrical)
                    {
                        nBlack = nWhite;
                    }
                    else
                    {
                        nBlack = randomProvider.RandomPositiveInt(max);
                    }
                    Game game = new Game(gameRepository.GenerateId(), hostIsWhite ? joined.Owner : client, hostIsWhite ? client : joined.Owner, joined.Variant, joined.FullVariantName, nWhite, nBlack, joined.Symmetrical, joined.TimeControl, DateTime.UtcNow, 0);
                    gameRepository.Add(game);
                    string redirectJson = "{\"t\":\"redirect\",\"d\":\"" + game.ID + "\"}";
                    await Send(redirectJson);
                    await handlerRepository.SendTo(joined.Owner, redirectJson);
                    break;
                case "init":
                    List<LobbySeek> seeks = seekRepository.GetShallowCopy();
                    foreach (LobbySeek s in seeks)
                    {
                        Dictionary<string, object> msg = new Dictionary<string, object>();
                        msg.Add("t", "add");
                        msg.Add("d", s.SeekJson(userRepository));
                        await Send(JsonConvert.SerializeObject(msg));
                    }
                    break;
                default:
                    await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                    break;
            }
        }

        public async Task Send(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            int length = buffer.Length;
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
            if (Open)
            {
                try
                {
                    await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch { } // just in case the web socket closes after if(Open) but before SendAsync
            }
        }

        public void Dispose()
        {
            Disposed = true;
            ws.Dispose();
        }
    }
}
