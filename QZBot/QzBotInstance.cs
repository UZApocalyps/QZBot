using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QZBot
{
    public class QzBotInstance
    {
        string token = File.ReadAllText("./token.txt");
        DiscordSocketClient _client;
        List<Room> rooms = new List<Room>();
        public QzBotInstance()
        {
            _client = new DiscordSocketClient();
            if (File.Exists("save.json"))
            {
                string content = File.ReadAllText("save.json");
                if (content != "")
                {
                    rooms = JsonConvert.DeserializeObject<List<Room>>(content);
                }
            }

        }
        public async void start()
        {
            try
            {
                _client.Log += _client_Log; ;
                _client.LoginAsync(TokenType.Bot, token, false);
                _client.StartAsync();
                _client.Ready += _client_Ready;

            }
            catch (Exception)
            {

            }

        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine("Message : " + arg.Message);
            if (arg.Exception != null)
            {
                Console.WriteLine("Error : " + arg.Exception);
            }
            return Task.CompletedTask;
        }

        private Task _client_Ready()
        {
            _client.MessageReceived += _client_MessageReceived;
            return Task.CompletedTask;
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            string message = arg.Content;
            string helpMessage = "**==== QZ BOT ====** \n" + "Pour rejoindre une QZ, fait la commande *`:join <nom_de_l'event> <nom_du_compte>-<vaisseau> <nom_du_compte_2>-<vaisseau_2> ...`* \n" +
                "Pour créer un évenement fait la commande *`:create <date> <nom_de_levent> <nombre_de_cpu_min>`*\n" +
                "Afficher un évenement : *`:show <nom_de_levent>`*\n" +
                "Terminer un évenement : *`:finish <nom_de_levent>`*\n" +
                "Lister tous les évenement actifs : *`:list`*";
            if (arg.Author.Id != 1008996021793210368)
            {
                arg.Channel.DeleteMessageAsync(arg.Id);
            }
            if (message.StartsWith(":"))
            {
                message = message.Substring(1);
                string[] arguments = message.Split(' ');
                string command = arguments[0];
                switch (message)
                {
                    case "help":
                        arg.Channel.SendMessageAsync(helpMessage);
                        break;
                    case null:
                        break;
                }
                if (command == "join")
                {
                    if (rooms.Count() <= 0)
                    {
                        arg.Channel.SendMessageAsync("Aucun évenement prévu ! Commencez par en créer un.");
                    }
                    else
                    {
                        string roomName = arguments[1];
                        Room r = rooms.Find(x => x.RoomName == roomName);
                        if (r != null)
                        {
                            for (int i = 2; i < arguments.Length; i++)
                            {
                                string[] datas = arguments[i].Split('-');
                                string username = datas[0];
                                string ship = datas[1];
                                Participant participant = new Participant();
                                participant.Name = username;
                                participant.AuthorId = arg.Author.Id;
                                participant.Ship = ship;
                                Participant p = r.participants.Find(x => x.Name == username);
                                ulong last = r.MessageId;
                                if (r.participants.Count + 1 < 9 && p == null)
                                {
                                    r.participants.Add(participant);
                                    arg.Channel.DeleteMessageAsync(last);
                                    last = await sendParticipants(r);
                                    Task.Delay(200);
                                }
                                else
                                {
                                    arg.Channel.SendMessageAsync("Il n'y a plus de place pour cet évenement ou alors vous êtes déjà inscrit");
                                }
                            }
                        }
                        else
                        {
                            arg.Channel.SendMessageAsync("Aucun évenement avec ce nom.");
                        }
                        
                    }
                }
                else if (command == "create")
                {
                    if (arguments.Length >= 4)
                    {
                        string date = arguments[1];
                        string roomName = arguments[2];
                        string cpu = arguments[3];
                        if (rooms.Find(x => x.RoomName == roomName) != null)
                        {
                            arg.Channel.SendFileAsync("Il y a déjà un évenement avec ce nom pour cette date");
                        }
                        else
                        {
                            Room r = new Room();
                            r.date = date;
                            r.ChannelId = arg.Channel.Id;
                            r.RoomName = roomName;
                            r.MinCpu = cpu;
                            rooms.Add(r);
                            sendParticipants(r);
                        }
                    }
                    else
                    {
                        arg.Channel.SendMessageAsync(arg.Author.Mention +" Vous n'avez pas fournis assez d'informations");
                    }
                    

                }
                else if (command =="finish")
                {
                    string roomName = arguments[1];
                    Room r = rooms.Find(r => r.RoomName == roomName);
                    if (r != null)
                    {
                        rooms.Remove(r);
                    }
                }
                else if (command == "show")
                {
                    string roomName = arguments[1];
                    Room r = rooms.Find(r => r.RoomName == roomName);
                    if (r != null)
                    {
                        sendParticipants(r);
                    }
                }
                else if (command == "list")
                {
                    string availableRoom = "**Evenement actif : **\n";
                    foreach (var item in rooms)
                    {
                        availableRoom += " **=>** *" +  item.RoomName + " - " + item.date + " - " + item.participants.Count + "/8 *\n";
                    }
                    arg.Channel.SendMessageAsync(availableRoom);

                }
                else if (true)
                {

                }
            }
        }

        private async Task<ulong> sendParticipants(Room r)
        {
            string message = "**__=== QZ "+r.RoomName+" ===__**\n" +
                "**__Date :__ " + r.date + "**\n" +
                "**__Cpu minimum :__ " +r.MinCpu + "**\n\n" +
                "**__Participants__**\n" ;
            foreach (var item in r.participants)
            {
                message += "[" + item.Name + "|" + item.Ship + "]\n";
            }
            for (int i = 0; i < 8-r.participants.Count; i++)
            {
                message += "[OPEN]\n";
            }
            ISocketMessageChannel channel = await _client.GetChannelAsync(r.ChannelId) as ISocketMessageChannel;
            RestUserMessage msg = await channel.SendMessageAsync(message);
            r.MessageId = msg.Id;
            return msg.Id;
        }


        public void SaveRooms()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string json = JsonConvert.SerializeObject(rooms,settings);
            File.WriteAllText("./save.json", json);
        }
    }
}
