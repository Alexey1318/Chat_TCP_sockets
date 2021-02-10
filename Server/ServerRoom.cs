using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServerRoom
    {
        public string RoomName { get; private set; }
        public List<ClientThread> Participants { get; private set; }
        public List<string> RoomMessageHistroy { get; private set; }
        public ServerRoom(string name)
        {
            RoomName = name;
            Participants = new List<ClientThread>();
            RoomMessageHistroy = new List<string>();
        }
    }
}
