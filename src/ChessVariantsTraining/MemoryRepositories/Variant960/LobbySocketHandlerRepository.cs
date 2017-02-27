using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models.Variant960;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public class LobbySocketHandlerRepository : ILobbySocketHandlerRepository
    {
        List<LobbySocketHandler> handlers = new List<LobbySocketHandler>();
        bool shouldCleanup = true;
        IUserRepository userRepository;

        public LobbySocketHandlerRepository(IUserRepository _userRepository)
        {
            Thread cleanupThread = new Thread(ScheduledCleanup);
            cleanupThread.Start();
            userRepository = _userRepository;
        }

        public void Add(LobbySocketHandler handler)
        {
            handlers.Add(handler);
        }

        public async Task SendAll(string text)
        {
            List<LobbySocketHandler> open = handlers.Where(x => x.Open).ToList();
            foreach (LobbySocketHandler handler in open)
            {
                await handler.Send(text);
            }
        }

        public async Task SendSeekAddition(LobbySeek seek)
        {
            Dictionary<string, object> msg = new Dictionary<string, object>();
            msg.Add("t", "add");
            msg.Add("d", seek.SeekJson(userRepository));
            await SendAll(JsonConvert.SerializeObject(msg));
        }

        public async Task SendSeekRemoval(string id)
        {
            await SendAll("{\"t\":\"remove\",\"d\":\"" + id + "\"}");
        }

        public async Task SendTo(GamePlayer client, string text)
        {
            IEnumerable<LobbySocketHandler> receiverHandlers = handlers.Where(x => x.Client.Equals(client));
            foreach (LobbySocketHandler handler in receiverHandlers)
            {
                if (handler != null)
                {
                    await handler.Send(text);
                }
            }
        }

        void ScheduledCleanup()
        {
            while (shouldCleanup)
            {
                CleanupClosed();
                Thread.Sleep(60 * 1000);
            }
        }

        void CleanupClosed()
        {
            List<LobbySocketHandler> closed = handlers.Where(x => x.Closed).ToList();
            foreach (LobbySocketHandler handler in closed)
            {
                handler.Dispose();
            }
            handlers.RemoveAll(x => x.Closed);
        }
    }
}
