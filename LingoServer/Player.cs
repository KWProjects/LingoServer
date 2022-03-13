using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingoServer
{
    class Player
    {
        public int ClientId;
        public int Seat;
        public string Name;
        public bool Ready = false;

        public Player(int clientId, int seat, string name)
        {
            this.ClientId = clientId;
            this.Seat = seat;
            this.Name = name;
        }

        public JSONPlayer GetJSONObject()
        {
            JSONPlayer JSONObject = new JSONPlayer
            {
                clientid = ClientId.ToString(),
                name = Name,
                seat = Seat.ToString()
            };
            return JSONObject;
        }

    }
}
