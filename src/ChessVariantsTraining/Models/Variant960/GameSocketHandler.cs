using ChessDotNet;
using ChessDotNet.Pieces;
using ChessDotNet.Variants.Crazyhouse;
using ChessDotNet.Variants.ThreeCheck;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Extensions;
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
        IRandomProvider randomProvider;
        IGameConstructor gameConstructor;
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

        public GameSocketHandler(WebSocket socket, GamePlayer _client, IGameRepoForSocketHandlers _gameRepository, IGameSocketHandlerRepository _handlerRepository, IMoveCollectionTransformer _moveCollectionTransformer, IUserRepository _userRepository, IRandomProvider _randomProvider, IGameConstructor _gameConstructor, string _gameId)
        {
            ws = socket;
            client = _client;
            gameRepository = _gameRepository;
            handlerRepository = _handlerRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
            userRepository = _userRepository;
            randomProvider = _randomProvider;
            gameId = _gameId;
            gameConstructor = _gameConstructor;
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
                    if (Subject.Result != Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"The game is not ongoing.\"}");
                        return;
                    }
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
                    bool flagged = await HandlePotentialFlag(Subject.ChessGame.WhoseTurn.ToString().ToLowerInvariant());
                    if (flagged) return;
                    if (!Regex.IsMatch(moveMessage.Move, "([a-h][1-8]-[a-h][1-8](-[qrnbk])?|[PNBRQ]@[a-h][1-8])"))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid message format\"}");
                        return;
                    }
                    string[] moveParts;
                    MoveType mt = MoveType.Move;

                    if (!moveMessage.Move.Contains("@"))
                    {
                        moveParts = moveMessage.Move.Split('-');
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
                            mt = await gameRepository.RegisterMoveAsync(Subject, move);
                        }
                        else if (moveMessage.Type == "move")
                        {
                            await Send("{\"t\":\"error\",\"d\":\"invalid move\"}");
                            return;
                        }
                        else
                        {
                            return; // for premoves, invalid moves can be silently ignored as mostly the problem is just a situation change on the board
                        }
                    }
                    else
                    {
                        CrazyhouseChessGame zhGame = Subject.ChessGame as CrazyhouseChessGame;
                        if (zhGame == null)
                        {
                            await Send("{\"t\":\"error\",\"d\":\"invalid move\"}");
                            return;
                        }

                        string[] typeAndPos = moveMessage.Move.Split('@');

                        Position pos = new Position(typeAndPos[1]);
                        Piece piece = Subject.ChessGame.MapPgnCharToPiece(typeAndPos[0][0], Subject.ChessGame.WhoseTurn);
                        Drop drop = new Drop(piece, pos, piece.Owner);

                        if (zhGame.IsValidDrop(drop))
                        {
                            await gameRepository.RegisterDropAsync(Subject, drop);
                        }
                        else
                        {
                            await Send("{\"t\":\"invalidDrop\",\"pos\":\"" + pos + "\"}");
                        }

                        moveParts = new string[] { pos.ToString().ToLowerInvariant(), pos.ToString().ToLowerInvariant() };
                    }

                    string check = null;
                    if (Subject.ChessGame.IsInCheck(Subject.ChessGame.WhoseTurn))
                    {
                        check = Subject.ChessGame.WhoseTurn.ToString().ToLowerInvariant();
                    }

                    string outcome = null;
                    if (Subject.ChessGame.IsWinner(Player.White))
                    {
                        outcome = "1-0, white wins";
                        await gameRepository.RegisterGameResultAsync(Subject, Game.Results.WHITE_WINS, Game.Terminations.NORMAL);
                    }
                    else if (Subject.ChessGame.IsWinner(Player.Black))
                    {
                        outcome = "0-1, black wins";
                        await gameRepository.RegisterGameResultAsync(Subject, Game.Results.BLACK_WINS, Game.Terminations.NORMAL);
                    }
                    else if (Subject.ChessGame.IsDraw() || Subject.ChessGame.DrawCanBeClaimed)
                    {
                        outcome = "½-½, draw";
                        await gameRepository.RegisterGameResultAsync(Subject, Game.Results.DRAW, Game.Terminations.NORMAL);
                    }

                    Dictionary<string, object> messageForPlayerWhoseTurnItIs = new Dictionary<string, object>();
                    Dictionary<string, object> messageForOthers = new Dictionary<string, object>();
                    messageForPlayerWhoseTurnItIs["t"] = messageForOthers["t"] = "moved";
                    messageForPlayerWhoseTurnItIs["fen"] = messageForOthers["fen"] = Subject.ChessGame.GetFen();
                    messageForPlayerWhoseTurnItIs["pgn"] = messageForOthers["pgn"] = Subject.PGN;
                    messageForPlayerWhoseTurnItIs["dests"] = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(Subject.ChessGame.GetValidMoves(Subject.ChessGame.WhoseTurn));
                    messageForPlayerWhoseTurnItIs["lastMove"] = messageForOthers["lastMove"] = new string[] { moveParts[0], moveParts[1] };
                    messageForPlayerWhoseTurnItIs["turnColor"] = messageForOthers["turnColor"] = Subject.ChessGame.WhoseTurn.ToString().ToLowerInvariant();
                    messageForPlayerWhoseTurnItIs["plies"] = messageForOthers["plies"] = Subject.ChessGame.Moves.Count;
                    Dictionary<string, double> clockDictionary = new Dictionary<string, double>
                    {
                        ["white"] = Subject.ClockWhite.GetSecondsLeft(),
                        ["black"] = Subject.ClockBlack.GetSecondsLeft()
                    };
                    messageForPlayerWhoseTurnItIs["clock"] = messageForOthers["clock"] = clockDictionary;
                    messageForPlayerWhoseTurnItIs["check"] = messageForOthers["check"] = check;
                    messageForPlayerWhoseTurnItIs["isCapture"] = messageForOthers["isCapture"] = mt.HasFlag(MoveType.Capture);
                    if (outcome != null)
                    {
                        messageForPlayerWhoseTurnItIs["outcome"] = messageForOthers["outcome"] = outcome;
                        messageForPlayerWhoseTurnItIs["termination"] = messageForOthers["termination"] = Game.Terminations.NORMAL;
                    }
                    if (Subject.ChessGame is ThreeCheckChessGame)
                    {
                        ThreeCheckChessGame tccg = Subject.ChessGame as ThreeCheckChessGame;
                        messageForPlayerWhoseTurnItIs["additional"] = messageForOthers["additional"] = string.Format("White delivered {0} check(s), black delivered {1}.", tccg.ChecksByWhite, tccg.ChecksByBlack);
                    }
                    messageForPlayerWhoseTurnItIs["pocket"] = messageForOthers["pocket"] = Subject.ChessGame.GenerateJsonPocket();
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
                        displayName = (await userRepository.FindByIdAsync(senderUserId.Value)).Username;
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
                        await Send("{\"t\":\"error\",\"d\":\"Anonymous users cannot use the Spectators' chat.\"}");
                        return;
                    }

                    ChatMessage chatMessage = new ChatMessage(senderUserId, displayName, chatSocketMessage.Content);
                    Dictionary<string, string> forPlayers = null;
                    Dictionary<string, string> forSpectators = null;
                    string jsonPlayersChat = null;
                    string jsonSpectatorsChat = null;
                    if (chatSocketMessage.Channel == "player")
                    {
                        await gameRepository.RegisterPlayerChatMessageAsync(Subject, chatMessage);
                        forPlayers = new Dictionary<string, string>() { { "t", "chat" }, { "channel", "player" }, { "msg", chatMessage.GetHtml() } };
                        jsonPlayersChat = JsonConvert.SerializeObject(forPlayers);
                    }
                    else if (chatSocketMessage.Channel == "spectator")
                    {
                        if (!(client is RegisteredPlayer))
                        {
                            await Send("{\"t\":\"error\",\"d\":\"Anonymous users cannot use the Spectators' chat.\"}");
                            return;
                        }
                        await gameRepository.RegisterSpectatorChatMessageAsync(Subject, chatMessage);
                        forSpectators = new Dictionary<string, string>() { { "t", "chat" }, { "channel", "spectator" }, { "msg", chatMessage.GetHtml() } };
                        jsonSpectatorsChat = JsonConvert.SerializeObject(forSpectators);
                        if (Subject.Result != Game.Results.ONGOING)
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
                        { "black", Subject.ClockBlack.GetSecondsLeft() },
                        { "run", Subject.ChessGame.Moves.Count > 1 && Subject.Result == Game.Results.ONGOING },
                        { "whoseTurn", Subject.ChessGame.WhoseTurn.ToString().ToLowerInvariant() }
                    };
                    await Send(JsonConvert.SerializeObject(syncedClockDict));

                    break;
                case "flag":
                    FlagSocketMessage flagMessage = new FlagSocketMessage(preprocessed);
                    if (!flagMessage.Okay)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                        return;
                    }
                    await HandlePotentialFlag(flagMessage.Player);
                    break;
                case "syncChat":
                    Dictionary<string, object> syncedChat = new Dictionary<string, object>
                    {
                        ["t"] = "chatSync"
                    };
                    if (Subject.White.Equals(client) || Subject.Black.Equals(client))
                    {
                        syncedChat["player"] = Subject.PlayerChats.Select(x => x.GetHtml());
                    }
                    if (Subject.Result != Game.Results.ONGOING || !(Subject.White.Equals(client) || Subject.Black.Equals(client)))
                    {
                        syncedChat["spectator"] = Subject.SpectatorChats.Select(x => x.GetHtml());
                    }
                    await Send(JsonConvert.SerializeObject(syncedChat));
                    break;
                case "rematch-offer":
                case "rematch-yes":
                    if (Subject.Result == Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"The game is still ongoing.\"}");
                        return;
                    }
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
                        await gameRepository.RegisterWhiteRematchOfferAsync(Subject);
                        if (Subject.BlackWantsRematch)
                        {
                            createRematch = true;
                        }
                    }
                    else
                    {
                        await gameRepository.RegisterBlackRematchOfferAsync(Subject);
                        if (Subject.WhiteWantsRematch)
                        {
                            createRematch = true;
                        }
                    }
                    if (createRematch)
                    {
                        int posWhite;
                        int posBlack;
                        if (Subject.RematchLevel % 2 == 0)
                        {
                            posWhite = Subject.PositionWhite;
                            posBlack = Subject.PositionBlack;
                        }
                        else
                        {
                            posWhite = randomProvider.RandomPositiveInt(Subject.ShortVariantName != "RacingKings" ? 960 : 1440);
                            if (Subject.IsSymmetrical)
                            {
                                posBlack = posWhite;
                            }
                            else
                            {
                                posBlack = randomProvider.RandomPositiveInt(Subject.ShortVariantName != "RacingKings" ? 960 : 1440);
                            }
                        }
                        Game newGame = new Game(await gameRepository.GenerateIdAsync(),
                            Subject.Black,
                            Subject.White,
                            Subject.ShortVariantName,
                            Subject.FullVariantName,
                            posWhite,
                            posBlack,
                            Subject.IsSymmetrical,
                            Subject.TimeControl,
                            DateTime.UtcNow,
                            Subject.RematchLevel + 1,
                            gameConstructor);
                        await gameRepository.AddAsync(newGame);
                        await gameRepository.SetRematchIDAsync(Subject, newGame.ID);
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
                    if (Subject.Result == Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"The game is still ongoing.\"}");
                        return;
                    }
                    bool isWhite_ = false;
                    bool isBlack_ = false;
                    if (!(isWhite_ = Subject.White.Equals(client)) && !(isBlack_ = Subject.Black.Equals(client)))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    await gameRepository.ClearRematchOffersAsync(Subject);
                    await handlerRepository.SendAll(gameId, "{\"t\":\"rematch-decline\"}", null, x => x.Equals(isWhite_ ? Subject.Black : Subject.White));
                    break;
                case "resign":
                    if (Subject.Result != Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"The game is not ongoing.\"}");
                        return;
                    }
                    bool whiteResigns;
                    if (!(whiteResigns = Subject.White.Equals(client)) && !Subject.Black.Equals(client))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    if (Subject.Result != Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"Game is not ongoing.\"}");
                        return;
                    }
                    string outcomeAfterResign;
                    if (!whiteResigns)
                    {
                        outcomeAfterResign = "1-0, white wins";
                        await gameRepository.RegisterGameResultAsync(Subject, Game.Results.WHITE_WINS, Game.Terminations.RESIGNATION);
                    }
                    else
                    {
                        outcomeAfterResign = "0-1, black wins";
                        await gameRepository.RegisterGameResultAsync(Subject, Game.Results.BLACK_WINS, Game.Terminations.RESIGNATION);
                    }
                    Dictionary<string, string> outcomeResponseDict = new Dictionary<string, string>()
                    {
                        { "t", "outcome" },
                        { "outcome", outcomeAfterResign },
                        { "termination", Game.Terminations.RESIGNATION }
                    };
                    await handlerRepository.SendAll(gameId, JsonConvert.SerializeObject(outcomeResponseDict), null, x => true);
                    break;
                case "abort":
                    if (Subject.Result != Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"The game is not ongoing.\"}");
                        return;
                    }
                    if (!Subject.White.Equals(client) && !Subject.Black.Equals(client))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    if (Subject.UciMoves.Count > 1)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"It's too late to abort.\"}");
                        return;
                    }
                    await gameRepository.RegisterGameResultAsync(Subject, Game.Results.ABORTED, Game.Terminations.ABORTED);
                    Dictionary<string, string> abortResultDict = new Dictionary<string, string>()
                    {
                        { "t", "outcome" },
                        { "outcome", Game.Results.ABORTED },
                        { "termination", Game.Terminations.ABORTED }
                    };
                    await handlerRepository.SendAll(gameId, JsonConvert.SerializeObject(abortResultDict), null, x => true);
                    break;
                case "draw-offer":
                case "draw-yes":
                    if (Subject.Result != Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"The game is not ongoing.\"}");
                        return;
                    }
                    bool whiteOfferingDraw = false;
                    bool blackOfferingDraw = false;
                    if (!(whiteOfferingDraw = Subject.White.Equals(client)) && !(blackOfferingDraw = Subject.Black.Equals(client)))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    if (whiteOfferingDraw)
                    {
                        await gameRepository.RegisterWhiteDrawOfferAsync(Subject);
                    }
                    else
                    {
                        await gameRepository.RegisterBlackDrawOfferAsync(Subject);
                    }
                    if (Subject.WhiteWantsDraw && Subject.BlackWantsDraw)
                    {
                        await gameRepository.RegisterGameResultAsync(Subject, Game.Results.DRAW, Game.Terminations.NORMAL);
                        Dictionary<string, string> drawResultDict = new Dictionary<string, string>()
                        {
                            { "t", "outcome" },
                            { "outcome", Game.Results.DRAW },
                            { "termination", Game.Terminations.NORMAL }
                        };
                        await handlerRepository.SendAll(SubjectID, JsonConvert.SerializeObject(drawResultDict), null, x => true);
                    }
                    else
                    {
                        string rematchOfferJson = "{\"t\":\"draw-offer\"}";
                        await handlerRepository.SendAll(gameId, rematchOfferJson, null, x => x.Equals(whiteOfferingDraw ? Subject.Black : Subject.White));
                    }
                    break;
                case "draw-no":
                    if (Subject.Result != Game.Results.ONGOING)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"The game is not ongoing.\"}");
                        return;
                    }
                    bool whiteDecliningDraw = false;
                    bool blackDecliningDraw = false;
                    if (!(whiteDecliningDraw = Subject.White.Equals(client)) && !(blackDecliningDraw = Subject.Black.Equals(client)))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    if (whiteDecliningDraw && !Subject.BlackWantsDraw)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"You have no open draw offers.\"}");
                        return;
                    }
                    if (blackDecliningDraw && !Subject.WhiteWantsDraw)
                    {
                        await Send("{\"t\":\"error\",\"d\":\"You have no open draw offers.\"}");
                        return;
                    }
                    await gameRepository.ClearDrawOffersAsync(Subject);
                    await handlerRepository.SendAll(gameId, "{\"t\":\"draw-decline\"}", null, x => x.Equals(whiteDecliningDraw ? Subject.Black : Subject.White));
                    break;
                case "keepAlive":
                    await Send("{\"t\":\"keepAlive\"}");
                    break;
            }
        }

        public async Task<bool> HandlePotentialFlag(string player)
        {
            if (Subject.Result != Game.Results.ONGOING)
            {
                return false;
            }

            double secondsLeft = player == "white" ? Subject.ClockWhite.GetSecondsLeft() : Subject.ClockBlack.GetSecondsLeft();
            if (secondsLeft <= 0)
            {
                (player == "white" ? Subject.ClockWhite : Subject.ClockBlack).AckFlag();
                await gameRepository.RegisterGameResultAsync(Subject, player == "white" ? Game.Results.BLACK_WINS : Game.Results.WHITE_WINS, Game.Terminations.TIME_FORFEIT);
                Dictionary<string, string> flagVerificationResponse = new Dictionary<string, string>()
                        {
                            { "t", "outcome" },
                            { "outcome", player == "white" ? "0-1, black wins" : "1-0, white wins" },
                            { "termination", Game.Terminations.TIME_FORFEIT }
                        };
                await handlerRepository.SendAll(gameId, JsonConvert.SerializeObject(flagVerificationResponse), null, x => true);
                return true;
            }
            return false;
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
