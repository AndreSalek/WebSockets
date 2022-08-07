using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//importing web socket library
using WebSocketSharp.Server;

namespace Server
{
    class Server : WebSocketServer
    {
        private static readonly string port = "8000";
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
        //Returns collection of short IDs that are needed for console output
        public IEnumerable<string> CreateShortIDArray(int idLength)
        {
            foreach (var client in WebSocketServices["/Connect"].Sessions.ActiveIDs)
            {
                yield return client.Remove(idLength);
            }
        }
        //Input collection, returns true of it has duplicates 
        public bool CheckforDuplicates(IEnumerable<T> array)
        {
            var duplicates = array
             .GroupBy(p => p)
             .Where(g => g.Count() > 1)
             .Select(g => g.Key);
            return (duplicates.Count() > 0);
        }
        //returns true if some client was kicked
        public bool KickUser(string shortID)
        {
            foreach (var clientID in WebSocketServices["/Connect"].Sessions.ActiveIDs)
            {
                if (clientID.StartsWith(shortID))
                {
                    WebSocketServices["/Connect"].Sessions.CloseSession(clientID, 1008, "Disconnected by server.");
                    return true;
                }
            }
            return false;
        }
    }
}
