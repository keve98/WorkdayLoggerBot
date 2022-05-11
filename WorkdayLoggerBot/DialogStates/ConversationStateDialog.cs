using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WorkdayLoggerBot.AdaptiveCards;
using WorkdayLoggerBot.Models;


namespace WorkdayLoggerBot
{
    public class ConversationStateDialog : ComponentDialog
    {

        static string AdaptivePromptId = "adaptive";

        public ConversationStateDialog(ConversationState conversationState) : base("root")
        {
            var _conversationStateDataAccessor = conversationState.CreateProperty<ConversationDataModel>("ConversationDataModel");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                ProjectNameStepAsync,
                TaskNameStepAsync,
                CommentStepAsync,
                HoursStepAsync,
                DayStepAsync,
                OvertimeStepAsync,
                OnSiteStepAsync,
                EndWaterfall
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


        public string getConversationId(WaterfallStepContext waterfallStepContext)
        {
            return waterfallStepContext.Context.Activity.Conversation.Id;
        }

        public ConversationDataModel GetConversationDataModelByChannelId(WaterfallStepContext waterfallStepContext)
        {
            return ConversationStorage.GetConversationByChannelId(getConversationId(waterfallStepContext)); ;
        }

        public Activity generatePromptMessage(string msg, WaterfallStepContext stepContext)
        {
            var messageText = stepContext.Options?.ToString() ?? msg;
            return MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
        }

        private async Task<DialogTurnResult> ProjectNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);
            if (conversationDataModel == null)
            {
                conversationDataModel = new ConversationDataModel(getConversationId(stepContext));
                ConversationStorage.conversations.Add(conversationDataModel);
            }

            if (conversationDataModel.workdayModel.Project != null && conversationDataModel.workdayModel.Project != "")
            {
                return await stepContext.NextAsync(-1, cancellationToken);
            }
            var promptMessage = generatePromptMessage("Which project did you work on?", stepContext);
            var ret = await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return ret;
        }

        private async Task<DialogTurnResult> TaskNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);
            if (conversationDataModel.workdayModel.Project == null || conversationDataModel.workdayModel.Project == "")
            {
                conversationDataModel.workdayModel.Project = stepContext.Result.ToString();
            }
            if (conversationDataModel.workdayModel.Task != null && conversationDataModel.workdayModel.Task != "")
            {
                return await stepContext.NextAsync(-1, cancellationToken);
            }

            var promptMessage = generatePromptMessage("What is the name of the task?", stepContext);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> CommentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);
            if (conversationDataModel.workdayModel.Task == null || conversationDataModel.workdayModel.Task == "")
            {
                conversationDataModel.workdayModel.Task = stepContext.Result.ToString();
            }
            if (conversationDataModel.workdayModel.Comment != null && conversationDataModel.workdayModel.Comment != "")
            {
                return await stepContext.NextAsync(-1, cancellationToken);
            }

            var promptMessage = generatePromptMessage("What is your comment?", stepContext);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> HoursStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);
            if (conversationDataModel.workdayModel.Comment == null || conversationDataModel.workdayModel.Comment == "")
            {
                conversationDataModel.workdayModel.Comment = stepContext.Result.ToString();
            }
            if (conversationDataModel.workdayModel.Hours != 0)
            {
                return await stepContext.NextAsync(-1, cancellationToken);
            }
            var promptMessage = generatePromptMessage("How many hours did you work on this?", stepContext);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> DayStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);
            if (conversationDataModel.workdayModel.Hours == 0)
            {
                conversationDataModel.workdayModel.Hours = int.Parse(stepContext.Result.ToString());
            }


            if (conversationDataModel.workdayModel.isDaySet)
            {
                return await stepContext.NextAsync(-1, cancellationToken);
            }


            var cardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(AdaptiveCard.FromJson(File.ReadAllText("DatePickerCard.json")).Card))
            };

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() { cardAttachment },
                    Type = ActivityTypes.Message,
                    Text = "Pick a date",
                }
            };

            return await stepContext.PromptAsync(AdaptivePromptId, opts);
        }

        private async Task<DialogTurnResult> OvertimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);
            if (!conversationDataModel.workdayModel.isDaySet)
            {
                JObject json = JObject.Parse(stepContext.Result.ToString());
                conversationDataModel.workdayModel.Day = (System.DateTime)json.GetValue("DateTime");
            }
            var promptMessage = generatePromptMessage("Is it overtime? yes/no", stepContext);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> OnSiteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);

            conversationDataModel.workdayModel.Overtime = parseStringToBool(stepContext.Result.ToString());
            var promptMessage = generatePromptMessage("Is is OnSite? yes/no?", stepContext);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndWaterfall(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ConversationDataModel conversationDataModel = GetConversationDataModelByChannelId(stepContext);
            conversationDataModel.workdayModel.OnSite = parseStringToBool(stepContext.Result.ToString());
            var model = conversationDataModel.workdayModel.toString();
            var promptMessage = generatePromptMessage(model, stepContext);
            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


        public bool parseStringToBool(string b)
        {
            if (b.ToLower().Contains("yes"))
            {
                return true;
            }

            return false;
        }





    }
}
