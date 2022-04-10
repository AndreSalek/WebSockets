using System;
using System.Linq;
using System.Threading.Tasks;

//importing web socket library
using WebSocketSharp.Server;

namespace Server
{
    class Server : WebSocketServer
    {
        private static readonly string port = "8000";
        public int idLength = 4;
        public event EventHandler ServerStateChange;
        
        public Server() : base($"ws://localhost:{port}")
        {
            
        }

        protected virtual void OnServerStateChange(object sender, EventArgs e)
        {
            if (!IsListening)
            {
                Start();
                //Adding websocketservice after every start because server would not have any services after turning off
                AddWebSocketService<ServerConnect>("/Connect", () => new ServerConnect());
            } 
            else Stop();
            Console.WriteLine($"Server state changed to { GetServerState() }");
            ServerStateChange?.Invoke(this, e);
        }

        public void ChangeServerState()
        {
            OnServerStateChange(this, EventArgs.Empty);
        }

        public string GetServerState()
        {
            var stateString = (IsListening) ? "online" : "offline";
            return stateString;
        }
        //Creates new array of short IDs that are needed for console output
        public async Task<string[]> CreateShortIDArrayAsync()
        {
            try
            {
                string[] id = new string[WebSocketServices["/Connect"].Sessions.ActiveIDs.Count()];
                var i = 0;
                foreach (var client in WebSocketServices["/Connect"].Sessions.ActiveIDs)
                {
                    id[i] = client.Remove(idLength);
                    i++;
                }
                return id;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("No clients connected \n " + ex);
            }
            return new string[0];
        }
        //Input string array, returns true if it contains any duplicates
        public async Task<bool> CheckforDuplicatesAsync(string[] array)
        {
            var duplicates = array
             .GroupBy(p => p)
             .Where(g => g.Count() > 1)
             .Select(g => g.Key);
            return (duplicates.Count() > 0);
        }
        //returns true if some client was kicked
        public async Task<bool> KickUser(string shortID)
        {
            foreach (var clientID in WebSocketServices["/Connect"].Sessions.ActiveIDs)
            {
                if (clientID.StartsWith(shortID))
                {
                    WebSocketServices["/Connect"].Sessions.CloseSession(clientID, 1008, "kicked");
                    return true;
                }
            }
            return false;
        }
    }
}
