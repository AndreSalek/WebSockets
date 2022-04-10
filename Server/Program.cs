using System;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.ChangeServerState();
            UserInterface ui = new UserInterface(server);
            ui.Run();
        }
    }
}
