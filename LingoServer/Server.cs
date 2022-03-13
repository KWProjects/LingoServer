using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace LingoServer
{
    class Server
    {
        TCPListener _listener;
        Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        private Object _clientsLock = new Object();
        Lingo LingoGame = new Lingo();
        Thread Worker;
        bool Running = false;

        Random random = new Random();

        public Server()
        {
            //IPHostEntry host = Dns.GetHostEntry("localhost");
            //IPAddress ipAddress = host.AddressList[0];
            //2a02:a210:94c0:3300:95e1:7292:2824:b44f local
            //2a02:a210:94c0:3300:79a5:65b8:11b9:3f84 Public
            IPAddress ipAddress = IPAddress.Parse("192.168.178.101");
            _listener = new TCPListener(ipAddress, 26555, 5);

            _listener.MessageReceived += listener_OnMessageReceived;
            _listener.ClientConnected += listener_OnClientConnected;
            _listener.ClientDisconnected += listener_OnClientDisconnected;

            _listener.Start();

        }

        public void Stop()
        {
            _listener.Stop();
            Console.WriteLine("Total messages received: " + _listener.total);
        }

        public void listener_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            lock (_clientsLock)
            {
                if (WebSocketProtocol.IsGetRequest(Encoding.ASCII.GetString(e.Message, 0, e.Message.Length)))
                {
                    _listener.Send(e.ClientId, WebSocketProtocol.CreateHandshake(Encoding.ASCII.GetString(e.Message, 0, e.Message.Length)));
                    
                }
                else
                {
                    string message = WebSocketProtocol.Decode(e.Message);
                    _listener.Send(e.ClientId, WebSocketProtocol.Encode("Message received!"));
                    if (message.Length > 2)
                    {
                        switch (message.Substring(0, 3))
                        {
                            case "400":
                                OnPlayerRequestJoin(e.ClientId, message.Substring(3));
                                break;
                            case "410":
                                OnRequestPlayerList(e.ClientId);
                                break;

                            case "101":
                                OnPlayerGuessed(e.ClientId, message.Substring(3));
                                break;

                        }
                    }
                    else
                    {
                        _listener.Disconnect(e.ClientId);
                    }

                    //Console.WriteLine("{0}", WebSocketProtocol.Decode(e.Message));
                    //_listener.Send(e.ClientId, WebSocketProtocol.Encode("Message received!"));
                }
            }
        }

        public void listener_OnClientConnected(object sender, ClientConnectionEventArgs e)
        {
            Console.WriteLine("Client" + e.ClientId + " connected!");
            Client client = new Client();
            client.ClientId = e.ClientId;
            lock (_clientsLock)
            {
                Clients.Add(client.ClientId, client);
            }
            Console.WriteLine(e.RemoteEndPoint);
            
        }

        public void listener_OnClientDisconnected(object sender, ClientConnectionEventArgs e)
        {
            Console.WriteLine("Client" + e.ClientId + " disconnected!");
            lock (_clientsLock)
            {
                if (Clients.TryGetValue(e.ClientId, out Client client))
                {
                    Clients.Remove(e.ClientId);
                }


                Player player = LingoGame.FindPlayerById(e.ClientId);
                if (player != null)
                {
                    OnPlayerLeft(player);
                }
            }
            
        }


        void OnPlayerReady(Player player)
        {
            if (!player.Ready)
            {
                player.Ready = true;
                //SendAll("402" + player.Color);
            }
        }
        void OnRequestPlayerList(int clientId)
        {
            lock (_clientsLock)
            {
                for (int i = 0; i < LingoGame.Players.Count(); i++)
                {
                    _listener.Send(clientId, WebSocketProtocol.Encode("401" + JsonSerializer.Serialize(LingoGame.Players[i].GetJSONObject())));
                }
            }
        }

        void OnPlayerRequestJoin(int clientId, string message)
        {
            lock (_clientsLock)
            {
                JoinRequest joinRequest = JsonSerializer.Deserialize<JoinRequest>(message);
                if (this.LingoGame.SeatAvailable(Int32.Parse(joinRequest.seat)) && joinRequest.name != "")
                {
                    Player player = LingoGame.FindPlayerById(clientId);
                    if(player != null)
                    {
                        LingoGame.RemovePlayer(player);
                        OnPlayerLeft(player);
                    }
                    player = new Player(clientId, Int32.Parse(joinRequest.seat), joinRequest.name);
                    LingoGame.AddPlayer(player);
                    NotifyAllPlayerJoined(player);
                }
                if (LingoGame.IsGameFull())
                {
                    StartGame();
                }
            }
        }
        void StartGame()
        {
            LingoGame.NewGame();
            LingoGame.NewRound();

            SendAll("100" + JsonSerializer.Serialize(LingoGame.GetJSONNextTurn()));
        }

        void OnPlayerGuessed(int clientId, string guess)
        {
            Player player = LingoGame.FindPlayerById(clientId);
            if (player != null)
            {
                if (LingoGame.IsPlayerTurn(player))
                {
                    LingoGame.Guess(guess);
                    LingoGame.NextTurn();
                    SendAll("101" + JsonSerializer.Serialize(LingoGame.GetJSONNextTurn()));
                }  
            }
        }

        void OnPlayerLeft(Player player)
        {
            lock (_clientsLock)
            {
                SendAll("404" + JsonSerializer.Serialize(player.GetJSONObject()));
                LingoGame.RemovePlayer(player);
            }
        }

        void SendPlayerList(int ClientId)
        {
            //_listener.Send(ClientId, WebSocketProtocol.Encode("400" + PlayerListToJSON()));
        }

        public void SendAll(string message)
        {
            lock (_clientsLock)
            {
                foreach (KeyValuePair<int, Client> entry in Clients)
                {
                    _listener.Send(entry.Value.ClientId, WebSocketProtocol.Encode(message));
                }
            }
        }

        public void Send(int clientId, string message)
        {
            _listener.Send(clientId, WebSocketProtocol.Encode(message));
        }

        void NotifyAllPlayerJoined(Player player)
        {
            lock (_clientsLock)
            {
                foreach (KeyValuePair<int, Client> entry in Clients)
                {
                    //if (entry.Value.ClientId != player.ClientId)
                    //{
                       _listener.Send(entry.Value.ClientId, WebSocketProtocol.Encode("401" + JsonSerializer.Serialize(player.GetJSONObject())));
                    //}
                }
            }
        }
    }
}
