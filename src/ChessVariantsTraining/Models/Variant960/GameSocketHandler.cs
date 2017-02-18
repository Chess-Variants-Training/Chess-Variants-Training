using ChessDotNet;
using ChessVariantsTraining.MemoryRepositories.Variant960;
using ChessVariantsTraining.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        CancellationToken ct = new CancellationToken(false);
        GamePlayer client;
        IGameRepoForSocketHandlers gameRepository;
        IGameSocketHandlerRepository handlerRepository;
        IMoveCollectionTransformer moveCollectionTransformer;
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

        public GameSocketHandler(WebSocket socket, GamePlayer _client, IGameRepoForSocketHandlers _gameRepository, IGameSocketHandlerRepository _handlerRepository, IMoveCollectionTransformer _moveCollectionTransformer, string gameId)
        {
            ws = socket;
            client = _client;
            gameRepository = _gameRepository;
            handlerRepository = _handlerRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
            subject = gameRepository.Get(gameId);
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
                await Send("{\"t\":\"error\",\"d\":\"invalid message\"}");
                return;
            }
            switch (message.Type)
            {
                case "move":
                    if ((subject.ChessGame.WhoseTurn == Player.White && !subject.White.Equals(client)) ||
                        (subject.ChessGame.WhoseTurn == Player.Black && !subject.Black.Equals(client)))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    if (!Regex.IsMatch(message.Data, "[a-h][1-8]-[a-h][1-8](-[qrnbk])?"))
                    {
                        await Send("{\"t\":\"error\",\"d\":\"no permission\"}");
                        return;
                    }
                    string[] moveParts = message.Data.Split('-');
                    Move move;
                    if (moveParts.Length == 2)
                    {
                        move = new Move(moveParts[0], moveParts[1], subject.ChessGame.WhoseTurn);
                    }
                    else
                    {
                        move = new Move(moveParts[0], moveParts[1], subject.ChessGame.WhoseTurn, moveParts[2][0]);
                    }
                    gameRepository.RegisterMove(subject, move);

                    Dictionary<string, object> messageForPlayerWhoseTurnItIs = new Dictionary<string, object>();
                    Dictionary<string, object> messageForOthers = new Dictionary<string, object>();
                    messageForPlayerWhoseTurnItIs["t"] = messageForOthers["t"] = "moved";
                    messageForPlayerWhoseTurnItIs["fen"] = messageForOthers["fen"] = subject.ChessGame.GetFen();
                    messageForPlayerWhoseTurnItIs["dests"] = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(subject.ChessGame.GetValidMoves(subject.ChessGame.WhoseTurn));
                    messageForPlayerWhoseTurnItIs["lastMove"] = messageForOthers["lastMove"] = new string[] { moveParts[0], moveParts[1] };
                    messageForPlayerWhoseTurnItIs["turnColor"] = messageForOthers["turnColor"] = subject.ChessGame.WhoseTurn.ToString().ToLowerInvariant();
                    messageForOthers["dests"] = new Dictionary<object, object>();
                    string jsonPlayers = JsonConvert.SerializeObject(messageForPlayerWhoseTurnItIs);
                    string jsonSpectators = JsonConvert.SerializeObject(messageForOthers);
                    await handlerRepository.SendAll(jsonPlayers, jsonSpectators, p => (subject.White.Equals(p) && subject.ChessGame.WhoseTurn == Player.White) || (subject.Black.Equals(p) && subject.ChessGame.WhoseTurn == Player.Black));

                    break;
            }
        }

        public async Task Send(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
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
