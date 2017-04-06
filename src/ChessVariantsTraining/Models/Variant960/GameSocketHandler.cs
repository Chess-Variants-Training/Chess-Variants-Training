using ChessDotNet;
using ChessDotNet.Variants.ThreeCheck;
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
        string gameId;

        public string SubjectID
        {
            get
            {
                return gameId;
            }
        }
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
                return Subject != null;
            }
        }
        public bool Disposed
        {
            get;
            private set;
        }
        
        Game Subject
        {
            get
            {
                return gameRepository.Get(gameId);
            }
        }

        public GameSocketHandler(WebSocket socket, GamePlayer _client, IGameRepoForSocketHandlers _gameRepository, IGameSocketHandlerRepository _handlerRepository, IMoveCollectionTransformer _moveCollectionTransformer, IUserRepository _userRepository, string _gameId)
        {
            ws = socket;
            client = _client;
            gameRepository = _gameRepository;
            handlerRepository = _handlerRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
            userRepository = _userRepository;
            gameId = _gameId;
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
                    if ((Subject.ChessGame.WhoseTurn == Player.White && !Subject.White.Equals(client)) ||
                        (Subject.ChessGame.WhoseTurn == Player.Black && !Subject.Black.Equals(client)))
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
                        move = new Move(moveParts[0], moveParts[1], Subject.ChessGame.WhoseTurn);
                    }
                    else
                    {
                        move = new Move(moveParts[0], moveParts[1], Subject.ChessGame.WhoseTurn, moveParts[2][0]);
                    }
                    if (Subject.ChessGame.IsValidMove(move))
                    {
                        gameRepository.RegisterMove(Subject, move);
                    }
                    else if (moveMessage.Type == "move")
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid move\"}");
                    } // for premoves, invalid moves can be silently ignored as mostly the problem is just a situation change on the board

                    string outcome = null;
                    if (Subject.ChessGame.IsWinner(Player.White))
                    {
                        outcome = "1-0, white wins";
                        gameRepository.RegisterGameOutcome(Subject, Game.Outcomes.WHITE_WINS);
                    }
                    else if (Subject.ChessGame.IsWinner(Player.Black))
                    {
                        outcome = "0-1, black wins";
                        gameRepository.RegisterGameOutcome(Subject, Game.Outcomes.BLACK_WINS);
                    }
                    else if (Subject.ChessGame.IsDraw())
                    {
                        outcome = "½-½, draw";
                        gameRepository.RegisterGameOutcome(Subject, Game.Outcomes.DRAW);
                    }

                    Dictionary<string, object> messageForPlayerWhoseTurnItIs = new Dictionary<string, object>();
                    Dictionary<string, object> messageForOthers = new Dictionary<string, object>();
                    messageForPlayerWhoseTurnItIs["t"] = messageForOthers["t"] = "moved";
                    messageForPlayerWhoseTurnItIs["fen"] = messageForOthers["fen"] = Subject.ChessGame.GetFen();
                    messageForPlayerWhoseTurnItIs["dests"] = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(Subject.ChessGame.GetValidMoves(Subject.ChessGame.WhoseTurn));
                    messageForPlayerWhoseTurnItIs["lastMove"] = messageForOthers["lastMove"] = new string[] { moveParts[0], moveParts[1] };
                    messageForPlayerWhoseTurnItIs["turnColor"] = messageForOthers["turnColor"] = Subject.ChessGame.WhoseTurn.ToString().ToLowerInvariant();
                    messageForPlayerWhoseTurnItIs["plies"] = messageForOthers["plies"] = Subject.ChessGame.Moves.Count;
                    Dictionary<string, double> clockDictionary = new Dictionary<string, double>();
                    clockDictionary["white"] = Subject.ClockWhite.GetSecondsLeft();
                    clockDictionary["black"] = Subject.ClockBlack.GetSecondsLeft();
                    messageForPlayerWhoseTurnItIs["clock"] = messageForOthers["clock"] = clockDictionary;
                    if (outcome != null)
                    {
                        messageForPlayerWhoseTurnItIs["outcome"] = messageForOthers["outcome"] = outcome;
                    }
                    if (Subject.ChessGame is ThreeCheckChessGame)
                    {
                        ThreeCheckChessGame tccg = Subject.ChessGame as ThreeCheckChessGame;
                        messageForPlayerWhoseTurnItIs["additional"] = messageForOthers["additional"] = string.Format("White delivered {0} check(s), black delivered {1}.", tccg.ChecksByWhite, tccg.ChecksByBlack);
                    }
                    messageForOthers["dests"] = new Dictionary<object, object>();
                    string jsonPlayersMove = JsonConvert.SerializeObject(messageForPlayerWhoseTurnItIs);
                    string jsonSpectatorsMove = JsonConvert.SerializeObject(messageForOthers);
                    await handlerRepository.SendAll(gameId, jsonPlayersMove, jsonSpectatorsMove, p => (Subject.White.Equals(p) && Subject.ChessGame.WhoseTurn == Player.White) || (Subject.Black.Equals(p) && Subject.ChessGame.WhoseTurn == Player.Black));

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
                        if (client.Equals(Subject.White))
                        {
                            displayName = "[white]";
                        }
                        else if (client.Equals(Subject.Black))
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
                        gameRepository.RegisterPlayerChatMessage(Subject, chatMessage);
                        forPlayers = new Dictionary<string, string>() { { "t", "chat" }, { "channel", "player" }, { "msg", chatMessage.GetHtml() } };
                        jsonPlayersChat = JsonConvert.SerializeObject(forPlayers);
                    }
                    else if (chatSocketMessage.Channel == "spectator")
                    {
                        gameRepository.RegisterSpectatorChatMessage(Subject, chatMessage);
                        forSpectators = new Dictionary<string, string>() { { "t", "chat" }, { "channel", "spectator" }, { "msg", chatMessage.GetHtml() } };
                        jsonSpectatorsChat = JsonConvert.SerializeObject(forSpectators);
                        if (Subject.Outcome != Game.Outcomes.ONGOING)
                        {
                            jsonPlayersChat = jsonSpectatorsChat;
                        }
                    }
                    await handlerRepository.SendAll(gameId, jsonPlayersChat, jsonSpectatorsChat, p => Subject.White.Equals(p) || Subject.Black.Equals(p));
                    break;
                case "syncClock":
                    Dictionary<string, object> syncedClockDict = new Dictionary<string, object>()
                    {
                        { "t", "clock" },
                        { "white", Subject.ClockWhite.GetSecondsLeft() },
                        { "black", Subject.ClockBlack.GetSecondsLeft() }
                    };
                    await Send(JsonConvert.SerializeObject(syncedClockDict));

                    break;
                case "flag":
                    if (Subject.Outcome != Game.Outcomes.ONGOING)
                    {
                        return;
                    }
                    FlagSocketMessage flagMessage = new FlagSocketMessage(preprocessed);
                    if (!flagMessage.Okay)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                        return;
                    }
                    double secondsLeft = flagMessage.Player == "white" ? Subject.ClockWhite.GetSecondsLeft() : Subject.ClockBlack.GetSecondsLeft();
                    if (secondsLeft <= 0)
                    {
                        gameRepository.RegisterGameOutcome(Subject, flagMessage.Player == "white" ? Game.Outcomes.BLACK_WINS : Game.Outcomes.WHITE_WINS);
                        Dictionary<string, string> flagVerificationResponse = new Dictionary<string, string>()
                        {
                            { "t", "outcome" },
                            { "outcome", flagMessage.Player == "white" ? "0-1, black wins" : "1-0, white wins" }
                        };
                        await handlerRepository.SendAll(gameId, JsonConvert.SerializeObject(flagVerificationResponse), null, x => true);
                    }
                    break;
                case "syncChat":
                    Dictionary<string, object> syncedChat = new Dictionary<string, object>();
                    syncedChat["t"] = "chatSync";
                    if (Subject.White.Equals(client) || Subject.Black.Equals(client))
                    {
                        syncedChat["player"] = Subject.PlayerChats.Select(x => x.GetHtml());
                    }
                    if (Subject.Outcome != Game.Outcomes.ONGOING || !(Subject.White.Equals(client) || Subject.Black.Equals(client)))
                    {
                        syncedChat["spectator"] = Subject.SpectatorChats.Select(x => x.GetHtml());
                    }
                    await Send(JsonConvert.SerializeObject(syncedChat));
                    break;
                case "rematch-offer":
                case "rematch-yes":
                    bool isWhite = false;
                    bool isBlack = false;
                    if (!(isWhite = Subject.White.Equals(client)) && !(isBlack = Subject.Black.Equals(client)))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    bool createRematch = false;
                    if (isWhite)
                    {
                        gameRepository.RegisterWhiteRematchOffer(Subject);
                        if (Subject.BlackWantsRematch)
                        {
                            createRematch = true;
                        }
                    }
                    else
                    {
                        gameRepository.RegisterBlackRematchOffer(Subject);
                        if (Subject.WhiteWantsRematch)
                        {
                            createRematch = true;
                        }
                    }
                    if (createRematch)
                    {
                        Game newGame = new Game(gameRepository.GenerateId(),
                            Subject.Black,
                            Subject.White,
                            Subject.ShortVariantName,
                            Subject.FullVariantName,
                            Subject.PositionWhite,
                            Subject.PositionBlack,
                            Subject.TimeControl,
                            DateTime.UtcNow);
                        gameRepository.Add(newGame);
                        string rematchJson = "{\"t\":\"rematch\",\"d\":\"" + newGame.ID + "\"}";
                        await Send(rematchJson);
                        await handlerRepository.SendAll(gameId, rematchJson, null, x => true);
                    }
                    else
                    {
                        string rematchOfferJson = "{\"t\":\"rematch-offer\"}";
                        await handlerRepository.SendAll(gameId, rematchOfferJson, null, x => x.Equals(isWhite ? Subject.Black : Subject.White));
                    }
                    break;
                case "rematch-no":
                    bool isWhite_ = false;
                    bool isBlack_ = false;
                    if (!(isWhite_ = Subject.White.Equals(client)) && !(isBlack_ = Subject.Black.Equals(client)))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    gameRepository.ClearRematchOffers(Subject);
                    await handlerRepository.SendAll(gameId, "{\"t\":\"rematch-decline\"}", null, x => x.Equals(isWhite_ ? Subject.Black : Subject.White));
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
