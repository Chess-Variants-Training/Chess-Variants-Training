using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.MemoryRepositories.Variant960;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    public class SocketController : CVTController
    {
        ILobbySocketHandlerRepository lobbySocketHandlerRepository;
        ILobbySeekRepository seekRepository;

        public SocketController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler, ILobbySocketHandlerRepository _lobbySocketHandlerRepository, ILobbySeekRepository _seekRepository)
            : base(_userRepository, _loginHandler)
        {
            lobbySocketHandlerRepository = _lobbySocketHandlerRepository;
            seekRepository = _seekRepository;
        }

        [Route("/Socket/Lobby")]
        public async Task LobbySocket([FromQuery] string clientId)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 418;
                return;
            }

            if (string.IsNullOrWhiteSpace(clientId) || clientId.Length < 6)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            WebSocket ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            LobbySocketHandler handler = new LobbySocketHandler(ws, loginHandler.LoggedInUserId(HttpContext), clientId, lobbySocketHandlerRepository, seekRepository);
            lobbySocketHandlerRepository.Add(handler);
            await handler.LobbyLoop();
        }
    }
}
