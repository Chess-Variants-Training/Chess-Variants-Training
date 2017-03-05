using ChessDotNet;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.MemoryRepositories.Variant960;
using ChessVariantsTraining.Models.Variant960.SocketMessages;
using ChessVariantsTraining.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Models.Variant960
{
    public class GameSocketHandler
    {
        WebSocket ws;
        GamePlayer client;
        IGameRepoForSocketHandlers gameRepository;
        IGameSocketHandlerRepository handlerRepository;
        IMoveCollectionTransformer moveCollectionTransformer;
        IUserRepository userRepository;
        Game subject;

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
        public bool GameExists
        {
            get
            {
                return subject != null;
            }
        }
        public bool Disposed
        {
            get;
            private set;
        }

        public GameSocketHandler(WebSocket socket, GamePlayer _client, IGameRepoForSocketHandlers _gameRepository, IGameSocketHandlerRepository _handlerRepository, IMoveCollectionTransformer _moveCollectionTransformer, IUserRepository _userRepository, string gameId)
        {
            ws = socket;
            client = _client;
            gameRepository = _gameRepository;
            handlerRepository = _handlerRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
            userRepository = _userRepository;
            subject = gameRepository.Get(gameId);
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
            GameSocketMessage preprocessed = new GameSocketMessage(text);
            if (!preprocessed.Okay)
            {
                await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                return;
            }
            switch (preprocessed.Type)
            {
                case "move":
                case "premove":
                    MoveSocketMessage moveMessage = new MoveSocketMessage(preprocessed);
                    if (!moveMessage.Okay)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                        return;
                    }
                    if ((subject.ChessGame.WhoseTurn == Player.White && !subject.White.Equals(client)) ||
                        (subject.ChessGame.WhoseTurn == Player.Black && !subject.Black.Equals(client)))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    if (!Regex.IsMatch(moveMessage.Move, "[a-h][1-8]-[a-h][1-8](-[qrnbk])?"))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid message format\"}");
                        return;
                    }
                    string[] moveParts = moveMessage.Move.Split('-');
                    Move move;
                    if (moveParts.Length == 2)
                    {
                        move = new Move(moveParts[0], moveParts[1], subject.ChessGame.WhoseTurn);
                    }
                    else
                    {
                        move = new Move(moveParts[0], moveParts[1], subject.ChessGame.WhoseTurn, moveParts[2][0]);
                    }
                    if (subject.ChessGame.IsValidMove(move))
                    {
                        gameRepository.RegisterMove(subject, move);
                    }
                    else if (moveMessage.Type == "move")
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid move\"}");
                    } // for premoves, invalid moves can be silently ignored as mostly the problem is just a situation change on the board

                    string outcome = null;
                    if (subject.ChessGame.IsWinner(Player.White))
                    {
                        outcome = "1-0, white wins";
                        gameRepository.RegisterGameOutcome(subject, Game.Outcomes.WHITE_WINS);
                    }
                    else if (subject.ChessGame.IsWinner(Player.Black))
                    {
                        outcome = "0-1, black wins";
                        gameRepository.RegisterGameOutcome(subject, Game.Outcomes.BLACK_WINS);
                    }
                    else if (subject.ChessGame.IsDraw())
                    {
                        outcome = "½-½, draw";
                        gameRepository.RegisterGameOutcome(subject, Game.Outcomes.DRAW);
                    }

                    Dictionary<string, object> messageForPlayerWhoseTurnItIs = new Dictionary<string, object>();
                    Dictionary<string, object> messageForOthers = new Dictionary<string, object>();
                    messageForPlayerWhoseTurnItIs["t"] = messageForOthers["t"] = "moved";
                    messageForPlayerWhoseTurnItIs["fen"] = messageForOthers["fen"] = subject.ChessGame.GetFen();
                    messageForPlayerWhoseTurnItIs["dests"] = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(subject.ChessGame.GetValidMoves(subject.ChessGame.WhoseTurn));
                    messageForPlayerWhoseTurnItIs["lastMove"] = messageForOthers["lastMove"] = new string[] { moveParts[0], moveParts[1] };
                    messageForPlayerWhoseTurnItIs["turnColor"] = messageForOthers["turnColor"] = subject.ChessGame.WhoseTurn.ToString().ToLowerInvariant();
                    messageForPlayerWhoseTurnItIs["plies"] = messageForOthers["plies"] = subject.ChessGame.Moves.Count;
                    Dictionary<string, double> clockDictionary = new Dictionary<string, double>();
                    clockDictionary["white"] = subject.ClockWhite.GetSecondsLeft();
                    clockDictionary["black"] = subject.ClockBlack.GetSecondsLeft();
                    messageForPlayerWhoseTurnItIs["clock"] = messageForOthers["clock"] = clockDictionary;
                    if (outcome != null)
                    {
                        messageForPlayerWhoseTurnItIs["outcome"] = messageForOthers["outcome"] = outcome;
                    }
                    messageForOthers["dests"] = new Dictionary<object, object>();
                    string jsonPlayersMove = JsonConvert.SerializeObject(messageForPlayerWhoseTurnItIs);
                    string jsonSpectatorsMove = JsonConvert.SerializeObject(messageForOthers);
                    await handlerRepository.SendAll(jsonPlayersMove, jsonSpectatorsMove, p => (subject.White.Equals(p) && subject.ChessGame.WhoseTurn == Player.White) || (subject.Black.Equals(p) && subject.ChessGame.WhoseTurn == Player.Black));

                    break;
                case "chat":
                    ChatSocketMessage chatSocketMessage = new ChatSocketMessage(preprocessed);
                    if (!chatSocketMessage.Okay)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                        return;
                    }

                    int? senderUserId = null;
                    string displayName = null;
                    if (client is RegisteredPlayer)
                    {
                        senderUserId = (client as RegisteredPlayer).UserId;
                        displayName = userRepository.FindById(senderUserId.Value).Username;
                    }
                    else
                    {
                        if (client.Equals(subject.White))
                        {
                            displayName = "[white]";
                        }
                        else if (client.Equals(subject.Black))
                        {
                            displayName = "[black]";
                        }
                    }

                    if (displayName == null)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }

                    ChatMessage chatMessage = new ChatMessage(senderUserId, displayName, chatSocketMessage.Content);
                    Dictionary<string, string> forPlayers = null;
                    Dictionary<string, string> forSpectators = null;
                    string jsonPlayersChat = null;
                    string jsonSpectatorsChat = null;
                    if (chatSocketMessage.Channel == "player")
                    {
                        gameRepository.RegisterPlayerChatMessage(subject, chatMessage);
                        forPlayers = new Dictionary<string, string>() { { "t", "chat" }, { "channel", "player" }, { "msg", chatMessage.GetHtml() } };
                        jsonPlayersChat = JsonConvert.SerializeObject(forPlayers);
                    }
                    else if (chatSocketMessage.Channel == "spectator")
                    {
                        gameRepository.RegisterSpectatorChatMessage(subject, chatMessage);
                        forSpectators = new Dictionary<string, string>() { { "t", "chat" }, { "channel", "spectator" }, { "msg", chatMessage.GetHtml() } };
                        jsonSpectatorsChat = JsonConvert.SerializeObject(forSpectators);
                        if (subject.Outcome != Game.Outcomes.ONGOING)
                        {
                            jsonPlayersChat = jsonSpectatorsChat;
                        }
                    }
                    await handlerRepository.SendAll(jsonPlayersChat, jsonSpectatorsChat, p => subject.White.Equals(p) || subject.Black.Equals(p));
                    break;
                case "syncClock":
                    Dictionary<string, object> syncedClockDict = new Dictionary<string, object>()
                    {
                        { "t", "clock" },
                        { "white", subject.ClockWhite.GetSecondsLeft() },
                        { "black", subject.ClockBlack.GetSecondsLeft() }
                    };
                    await Send(JsonConvert.SerializeObject(syncedClockDict));

                    break;
                case "flag":
                    if (subject.Outcome != Game.Outcomes.ONGOING)
                    {
                        return;
                    }
                    FlagSocketMessage flagMessage = new FlagSocketMessage(preprocessed);
                    if (!flagMessage.Okay)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                        return;
                    }
                    double secondsLeft = flagMessage.Player == "white" ? subject.ClockWhite.GetSecondsLeft() : subject.ClockBlack.GetSecondsLeft();
                    if (secondsLeft <= 0)
                    {
                        gameRepository.RegisterGameOutcome(subject, flagMessage.Player == "white" ? Game.Outcomes.BLACK_WINS : Game.Outcomes.WHITE_WINS);
                        Dictionary<string, string> flagVerificationResponse = new Dictionary<string, string>()
                        {
                            { "t", "outcome" },
                            { "outcome", flagMessage.Player == "white" ? "0-1, black wins" : "1-0, white wins" }
                        };
                        await handlerRepository.SendAll(JsonConvert.SerializeObject(flagVerificationResponse), null, x => true);
                    }
                    break;
                case "syncChat":
                    Dictionary<string, object> syncedChat = new Dictionary<string, object>();
                    syncedChat["t"] = "chatSync";
                    if (subject.White.Equals(client) || subject.Black.Equals(client))
                    {
                        syncedChat["player"] = subject.PlayerChats.Select(x => x.GetHtml());
                    }
                    if (subject.Outcome != Game.Outcomes.ONGOING || !(subject.White.Equals(client) || subject.Black.Equals(client)))
                    {
                        syncedChat["spectator"] = subject.SpectatorChats.Select(x => x.GetHtml());
                    }
                    await Send(JsonConvert.SerializeObject(syncedChat));
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
