using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.MemoryRepositories.Variant960;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    public class SocketController : CVTController
    {
        ILobbySocketHandlerRepository lobbySocketHandlerRepository;
        ILobbySeekRepository seekRepository;
        IGameRepository gameRepository;
        IRandomProvider randomProvider;
        IGameSocketHandlerRepository gameSocketHandlerRepository;
        IGameRepoForSocketHandlers gameRepoForSocketHandlers;
        IMoveCollectionTransformer moveCollectionTransformer;

        public SocketController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler, ILobbySocketHandlerRepository _lobbySocketHandlerRepository, ILobbySeekRepository _seekRepository, IGameRepository _gameRepository, IRandomProvider _randomProvider, IGameSocketHandlerRepository _gameSocketHandlerRepository, IGameRepoForSocketHandlers _gameRepoForSocketHandlers, IMoveCollectionTransformer _moveCollectionTransformer)
            : base(_userRepository, _loginHandler)
        {
            lobbySocketHandlerRepository = _lobbySocketHandlerRepository;
            seekRepository = _seekRepository;
            gameRepository = _gameRepository;
            randomProvider = _randomProvider;
            gameSocketHandlerRepository = _gameSocketHandlerRepository;
            gameRepoForSocketHandlers = _gameRepoForSocketHandlers;
            moveCollectionTransformer = _moveCollectionTransformer;
        }

        [Route("/Socket/Lobby")]
        public async Task LobbySocket()
        {
            System.Console.WriteLine(HttpContext.WebSockets.IsWebSocketRequest);
            /*if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 418;
                return;
            }*/

            WebSocket ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            GamePlayer client;
            int? userId = loginHandler.LoggedInUserId(HttpContext);
            if (userId.HasValue)
            {
                client = new RegisteredPlayer() { UserId = userId.Value };
            }
            else
            {
                if (HttpContext.Session.GetString("anonymousIdentifier") == null)
                {
                    HttpContext.Response.StatusCode = 400;
                    return;
                }
                client = new AnonymousPlayer() { AnonymousIdentifier = HttpContext.Session.GetString("anonymousIdentifier") };
            }
            LobbySocketHandler handler = new LobbySocketHandler(ws, client, lobbySocketHandlerRepository, seekRepository, gameRepository, randomProvider, userRepository);
            lobbySocketHandlerRepository.Add(handler);
            await handler.LobbyLoop();
        }

        [Route("/Socket/Game/{id}")]
        public async Task GameSocket(string id)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 418;
                return;
            }

            WebSocket ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            GamePlayer client;
            int? userId = loginHandler.LoggedInUserId(HttpContext);
            if (userId.HasValue)
            {
                client = new RegisteredPlayer() { UserId = userId.Value };
            }
            else
            {
                if (HttpContext.Session.GetString("anonymousIdentifier") == null)
                {
                    HttpContext.Response.StatusCode = 400;
                    return;
                }
                client = new AnonymousPlayer() { AnonymousIdentifier = HttpContext.Session.GetString("anonymousIdentifier") };
            }
            GameSocketHandler handler = new GameSocketHandler(ws, client, gameRepoForSocketHandlers, gameSocketHandlerRepository, moveCollectionTransformer, userRepository, id);
            if (!handler.GameExists)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }
            gameSocketHandlerRepository.Add(handler);
            await handler.LobbyLoop();
        }
    }
}
