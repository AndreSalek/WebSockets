using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Connect();
            UserInterface ui = new UserInterface(client);
            ui.Run();
        }
    }
}
