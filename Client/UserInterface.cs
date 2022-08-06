using System;

namespace Client
{
    class UserInterface
    {
        Client client;
        public bool menuExited = false;


        public UserInterface(Client client)
        {
            this.client = client;
        }

        public void Run()
        {
            GetMenuOptions();
            menuLoop();
        }       
        //main loop
        public void menuLoop()
        {
            while (!menuExited || !client.disconnectedManually)
            {
                if (client.disconnectedManually) menuExited = true;
                string command = Console.ReadLine().ToString().ToLower().Trim();
                MenuSwitch(command);
            }
        }
        
        //determines whether client on/off state will be changed
        public void SetNewState(string newState)
        {
            bool newStateBool = (newState == "connect") ? true : false;
            if (client.IsAlive == newStateBool) Console.WriteLine($"Client is already { client.GetClientState() }");
            else if (client.IsAlive)
            {
                client.Close();
            }
            else client.Connect();
        }

        private void GetMenuOptions()
        {
            Console.WriteLine("Choose by numbers or commands in brackets");
            Console.WriteLine("1.Exit application                | [exit]");
            Console.WriteLine("2.Execute file on server          | [execute path]");
            Console.WriteLine("3.Change client connection status | [client connect/disconnect/status]");
            Console.WriteLine("4.Get menu options                | [menu]");
        }
        public void MenuSwitch(string command)
        {
            var args = command.Split(" ");
            switch (args[0])
            {
                case "exit":
                case "1":
                    if (client.IsAlive) client.Close();
                    menuExited = true;
                    break;
                case "execute":
                case "2":
                    if (args.Length.Equals(1))
                    {
                        Console.WriteLine("Write absolute path of the file you want to execute");
                        string path = Console.ReadLine().ToString().ToLower().Trim();
                        client.ExecuteFile(path);
                    }
                    else client.ExecuteFile(args[1]);
                    break;
                case "3":
                    if (!args.Length.Equals(1)) goto default;
                    else if (client.IsAlive)
                    {
                        SetNewState("close");
                    }
                    else SetNewState("connect");
                    break;
                case "client":
                    if (args.Length.Equals(2)) goto default;
                    else if (!args[1].Equals("status") || !args[1].Equals("connect") || !args[1].Equals("close")) goto default;
                    else if (args[1].Equals("status")) Console.WriteLine($"Client is { client.GetClientState() }");
                    else SetNewState(args[1]);
                    break;
                case "4":
                    GetMenuOptions();
                    break;
                default:
                    Console.WriteLine("Command does not exist.");
                    break;
            }
        }
    }
}                                                                                                                                           
