using System.Collections.Generic;

namespace WorkdayLoggerBot.Models
{
    public static class ConversationStorage
    {
        public static List<ConversationDataModel> conversations { get; set; } = new List<ConversationDataModel>();

        public static ConversationDataModel GetConversationByChannelId(string id)
        {
            foreach (ConversationDataModel conversation in conversations)
            {
                if (conversation.ChannelId == id)
                {
                    return conversation;
                }
            }
            return null;
        }
    }
}
