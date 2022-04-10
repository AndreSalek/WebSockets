using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using WebSocketSharp;

namespace Client
{
    public class Client : WebSocket
    {
        private Timer timer;
        private int reconnectCount = 0;
        public bool kicked = false;
        public bool disconnectedManually;

        public event EventHandler ClientStateChange;
        public Client() : base("ws://localhost:8000/Connect")
        {
            
            OnMessage += Client_OnMessage;
            OnClose += Client_OnClose;
            OnError += Client_OnError;
            OnOpen += Client_OnOpen;
            
        }

        private void Client_OnOpen(object sender, EventArgs e)
        {
            OnClientStateChange(this, EventArgs.Empty);
            reconnectCount = 0;
        }

        private void Client_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("Error");
        }

        protected virtual void OnClientStateChange(object sender, EventArgs e)
        {
            Console.WriteLine($"Client state changed to { GetClientState() }");
            ClientStateChange?.Invoke(this, EventArgs.Empty);
        }
        public string GetClientState()
        {
            var stateString = (IsAlive) ? "online" : "offline";
            return stateString;
        }
        private void Client_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"Server returned: \n {e.Data}");
        }

        private void Client_OnClose(object sender, CloseEventArgs e)
        {
            //determine whether client was kicked, Checking for this e.code because I used this one on server to disconnect from session
            OnClientStateChange(this, EventArgs.Empty);
            kicked = (e.Code == 1008) ? true : false;
            
            if (!kicked)
            {
                //setup and start timer to attempt reconnection
                SetTimer();
                timer.Start();
            }
            else
            {
                //if client was kicked, reconnect will not be started
                Console.WriteLine("You have been kicked by the server.");
            }
        }
        //timer settings
        private void SetTimer()
        {
            timer = new Timer(5000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += Timer_Elapsed;
        }
        //timer elapsed event for reconnecting
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //end timer after 10 tries
            if (disconnectedManually)
            {
                disconnectedManually = false;
                timer.Dispose();
                return;
            }
            else
            {

            }
            if (reconnectCount == 10 && ReadyState == (WebSocketState)3)
            {
                Console.WriteLine("Cannot connect to server");
                timer.Dispose();
                
            }
            //if client is not connected 
            else if (ReadyState != (WebSocketState)1) 
            {
                reconnectCount += 1;
                Console.WriteLine($"Failed to connect to server. Reconnecting... ({ reconnectCount }/10)");
                Connect();
            }
        }
        //file with code is sent to server for execution
        public void ExecuteFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                //reading all the file into bytes for data transfer to server
                byte[] fileData = File.ReadAllBytes(path);
                Send(fileData);
            }
            else Console.WriteLine("Path does not exist");
        }
    }
}
