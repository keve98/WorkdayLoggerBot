using WorkdayLoggerBot.Services;

namespace WorkdayLoggerBot.Models
{
    public class ConversationDataModel
    {
        public string ChannelId { get; set; }
        public WorkdayModel workdayModel { get; set; } = new WorkdayModel();

        public ConversationDataModel(string Id)
        {
            ChannelId = Id;
        }
        public ConversationDataModel() { }
    }
}
