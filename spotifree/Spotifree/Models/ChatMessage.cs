using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.Models
{
    public class ChatMessage
    {
        public string Content { get; set; }
        public bool IsUserMessage { get; set; } // True = User, False = Bot
    }
}
