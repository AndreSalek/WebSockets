using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserInterface
    {
        Server server;
        public bool menuExited = false;
        public int idLength = 4;

        public UserInterface(Server server)
        {
            this.server = server;
        }

        public void Run()
        {
            GetMenuOptions();
            MenuLoop();
        }

        //changing client on/off state
        public void ChangeState(string newState)
        {
            bool newStateBool = (newState == "start") ? true : false;
            if (server.IsListening == newStateBool) Console.WriteLine($"Server is already { server.GetServerState() }");
            else server.ChangeServerState();
        }
        //main command switch that has all the menu scenarious
        public void MenuSwitch(string command)
        {
            var args = command.Split(" ");
            switch (args[0])
            {
                case "1":
                    if (server.IsListening) server.ChangeServerState();
                    menuExited = true;
                    break;
                case "2":
                    if (args.Length.Equals(1)) server.ChangeServerState();
                    break;
                case "server":
                    if (!args.Length.Equals(2)) goto default;
                    if (!(args[1].Equals("start") || !(args[1].Equals("stop")) || !(args[1].Equals("status")))) goto default;
                    if (args[1].Equals("status")) Console.WriteLine($"Server is { server.GetServerState() }");
                    else ChangeState(args[1]);
                    break;
                case "3":
                    if (args.Length.Equals(1))
                    {
                        Console.WriteLine("What's the client's ID?");
                        string id = Console.ReadLine().ToString().ToLower().Trim();
                        if (!(id.Length == idLength)) goto default;
                        bool kicked = server.KickUser(id);
                        if (kicked)
                        {
                            Console.WriteLine("Client kicked");
                        }
                        else Console.WriteLine("Client not found");
                    }
                    else goto default;
                    break;
                case "kick":
                    if (args.Length.Equals(1)) goto default;
                    if (args.Length >= 3) goto default;
                    if (server.GetServerState() == "offline")
                    {
                        Console.WriteLine("Cannot use this function while offline");
                        break;
                    }
                    if (!(args[1].Length == idLength)) goto default;
                    bool kicked1 = server.KickUser(args[1]);
                    if (kicked1)
                    {
                        Console.WriteLine("Client kicked");
                    }
                    else Console.WriteLine("Client not found");
                    break;
                case "4":
                case "list":
                    try
                    {
                        IEnumerable<string> shortIDs = server.CreateShortIDArray(idLength);
                        bool hasDuplicates = server.CheckforDuplicates(shortIDs);
                        if (server.GetServerState() == "offline")
                        {
                            Console.WriteLine("Cannot use this function while offline");
                            break;
                        }
                        if (args.Length >= 2) goto default;
                        if (hasDuplicates)
                        {
                            idLength += 1;
                            goto case "list";
                        }
                        Console.WriteLine($"Number of active clients: {shortIDs.Count()}");
                        foreach (var shortID in shortIDs)
                        {
                            Console.WriteLine(shortID);
                        }
                        idLength = 4;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot list connected clients \n" +ex);
                    }
                    
                    break;
                case "5":
                case "menu":
                    GetMenuOptions();
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    break;
            }
        }

        //menu options
        public static void GetMenuOptions()
        {
            Console.WriteLine("Choose by numbers or commands in brackets");
            Console.WriteLine("1.Exit application | [exit]");
            Console.WriteLine("2.Change websocket server status | [server start/stop/status]");
            Console.WriteLine("3.Disconnect client from server | [kick clientID]");
            Console.WriteLine("4.List all connected clients by ID | [list]");
            Console.WriteLine("5.Get menu options | [menu]");
        }
        //main loop 
        public void MenuLoop()
        {
            while (!menuExited)
            {
                var command = Console.ReadLine().ToString().ToLower().Trim();
                MenuSwitch(command);
            }
        }
    }
}
