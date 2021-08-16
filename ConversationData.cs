using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBotDemo
{
    public class ConversationData
    {
        public string Timestamp { get; set; }
        public string ChannelId { get; set; }
        public enum Question
        {
            None,
            Name,
            Age,
            Email
        }
        public Question PreviousQuestion { get; set; } = ConversationData.Question.None;
        public bool DidFillUserProfile { get; set; } = false;
        public bool DidWelcomeUser { get; set; }

    }
}
