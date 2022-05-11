// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

using AdaptiveCards;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WorkdayLoggerBot.LUIS;
using WorkdayLoggerBot.Models;

namespace WorkdayLoggerBot.Bots
{
    public class DialogBot<T> : ActivityHandler
            where T : Dialog
    {

        private BotState ConversationState;
        private Dialog Dialog;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        private LuisHelper _luisHelper;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, LuisHelper luisHelper)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
            _luisHelper = luisHelper;
        }


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }


        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {


            if (ConversationStorage.GetConversationByChannelId(turnContext.Activity.Conversation.Id) != null)
            {
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
            else
            {
                var result = await _luisHelper.RecognizeAsync<WorkDayLUIS>(turnContext, cancellationToken);

                ConversationDataModel newConversation = new ConversationDataModel();
                WorkdayModel newModel = new WorkdayModel();

                if (result.Entities.datetime != null)
                {
                    foreach (DateTimeSpec dateTime in result.Entities.datetime)
                    {
                        if (dateTime.Type == "date")
                        {
                            newModel.Day = DateTime.Parse(dateTime.Expressions[0].ToString());
                            newModel.isDaySet = true;
                        }
                    }
                }

                if (result.Entities.projectName != null)
                {
                    newModel.Project = result.Entities.projectName[0].ToString();
                }


                if (result.Entities.taskName != null)
                {
                    newModel.Task = result.Entities.taskName[0].ToString();
                }


                if (result.Entities.hours != null)
                {
                    newModel.Hours = int.Parse(result.Entities.hours[0].ToString());
                }


                if (result.Entities.comment != null)
                {
                    newModel.Comment = result.Entities.comment[0].ToString();
                }

                newConversation.workdayModel = newModel;
                newConversation.ChannelId = turnContext.Activity.Conversation.Id;

                ConversationStorage.conversations.Add(newConversation);

                string msg = "The bot made the following entity:  \n" + newConversation.workdayModel.toString() + "  \nPlease answer the questions.";
                var promptMessage = MessageFactory.Text(msg, msg, InputHints.ExpectingInput);

                await turnContext.SendActivityAsync(promptMessage);

                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Welcome! Please type in a work day log sentence";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }


        private Attachment CreateDatePickerAdaptiveCard()
        {
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(File.ReadAllText("DatePickerCard.json")).Card
            };
        }

    }
}
