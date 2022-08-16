using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace QZBot
{
    public class Room
    {
        public ulong ChannelId { get; set; }
        public string RoomName { get; set; }
        public ulong MessageId { get; set; }

        public string MinCpu { get; set; }

        public List<Participant> participants = new List<Participant>();
        
        public string date { get; set; }
    }
}
