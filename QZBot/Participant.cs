using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace QZBot
{
    public class Participant
    {
        public ulong AuthorId { get; set; }
        public string Name { get; set; }
        public string Ship { get; set; }
    }
}
