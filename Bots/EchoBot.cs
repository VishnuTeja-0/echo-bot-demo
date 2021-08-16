// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using EchoBotDemo.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text.Sequence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBotDemo.Bots
{
    public class EchoBot<T> : ActivityHandler where T : CarPropertiesDialog
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        private readonly CarPropertiesDialog _dialog;

        //private IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        //private DialogSet _dialogs;

        public EchoBot(UserState userState, ConversationState conversationState, T dialog)
        {
            _userState = userState;
            _conversationState = conversationState;
            _dialog = dialog;
            //_dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            //_dialogs = new DialogSet(_dialogStateAccessor);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            if (!conversationData.DidWelcomeUser)
            {
                return;
            }

            if (string.IsNullOrEmpty(userProfile.Name) || string.IsNullOrEmpty(userProfile.Age.ToString()) || string.IsNullOrEmpty(userProfile.Email))
            {
                await FillOutUserProfileAsync(conversationData, userProfile, turnContext, cancellationToken);
                if (conversationData.DidFillUserProfile)
                {
                    var reply = MessageFactory.Text("What would you like me to do");
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Tell you about stringy thingies",
                            Type = ActionTypes.ImBack,
                            Value = "stringy"
                        },
                        new CardAction()
                        {
                            Title = "Search for a car",
                            Type = ActionTypes.ImBack,
                            Value = "search car"
                        }
                    }
                    };
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }               
            }
            else
            {
                var userInput = turnContext.Activity.Text.ToLower();
                switch (userInput)
                {
                    case "wait":
                        await turnContext.SendActivitiesAsync(
                        new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 3000 },
                MessageFactory.Text("Finished typing", "Finished typing"),
                        },
                        cancellationToken);
                        break;
                    case "stringy":
                        var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resource", "Banjo.jpg");
                        var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

                        var attachment1 = new Attachment
                        {
                            Name = "Banjo",
                            ContentType = "image/jpg",
                            ContentUrl = $"data:image/jpg;base64,{imageData}"
                        };

                        var card = new HeroCard
                        {
                            Text = "Guitars",
                            Images = new List<CardImage>()
                        {
                            new CardImage("https://upload.wikimedia.org/wikipedia/commons/4/45/GuitareClassique5.png"),
                            new CardImage("https://cdn.shopify.com/s/files/1/0657/6821/products/VAU-RG1RW-TBK.jpg?v=1624944674")
                        },
                            Buttons = new List<CardAction>
                        {
                            new CardAction(ActionTypes.OpenUrl, title: "Acoustic", value: "https://en.wikipedia.org/wiki/Acoustic_guitar"),
                            new CardAction(ActionTypes.OpenUrl, title: "Electric", value: "https://en.wikipedia.org/wiki/Electric_guitar"),
                            new CardAction(ActionTypes.ImBack, title: "Say Yea", value: "Yeaaaaaa")
                        }
                        };

                        var attachment3 = new Attachment
                        {
                            Name = "Violin",
                            ContentType = "video/mp4",
                            ContentUrl = "https://youtu.be/UxfRhs_98mQ"
                        };

                        var filePath = Path.Combine(Environment.CurrentDirectory, @"Resource", "CodingStandards.doc");
                        var fileData = Convert.ToBase64String(File.ReadAllBytes(filePath));

                        var attachment4 = new Attachment
                        {
                            Name = "Noodle",
                            ContentType = "file/docx",
                            ContentUrl = $"data:file/docx;base64,{fileData}"
                        };

                        var reply = MessageFactory.Text("Here are som Stringy Thingies");
                        reply.Attachments = new List<Attachment>() { attachment1, card.ToAttachment(), attachment3, attachment4 };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                        await turnContext.SendActivityAsync(reply, cancellationToken);
                        break;
                    case "search car":
                        await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
 
                        break;
                    default:
                        await turnContext.SendActivityAsync(MessageFactory.Text("Sorry I do not know about that."));
                        break;
                }
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and Welcome!"), cancellationToken);
                    conversationData.DidWelcomeUser = true;
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            //var dc = await Dialogs.CreateContextAsync(turnContext);
            //if(turnContext.Activity.Type == ActivityTypes.Message && dc.ActiveDialog != null)
            //{
            //    await dc.ContinueDialogAsync(cancellationToken);
            //}
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        public static async Task FillOutUserProfileAsync(ConversationData conversationData, UserProfile userProfile, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userInput = turnContext.Activity.Text.Trim();

            switch (conversationData.PreviousQuestion)
            {
                case ConversationData.Question.None:
                    await turnContext.SendActivityAsync(MessageFactory.Text("You look like yer new around town pardner. What's your name?"), cancellationToken);
                    conversationData.PreviousQuestion = ConversationData.Question.Name;
                    break;
                case ConversationData.Question.Name:
                    if (IsInputValid(userInput, out _, ConversationData.Question.Name))
                    {
                        userProfile.Name = userInput;
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Howdy {userProfile.Name}! What is your age?"), cancellationToken);
                        conversationData.PreviousQuestion = ConversationData.Question.Age;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("That don't sound like a name to me. Come again?"), cancellationToken);
                        break;
                    }
                case ConversationData.Question.Age:
                    if (IsInputValid(userInput, out var age, ConversationData.Question.Age))
                    {
                        userProfile.Age = Convert.ToInt32(age);
                        await turnContext.SendActivityAsync(MessageFactory.Text($"{userProfile.Age} sounds legal enough to me. Now gimme your email address."), cancellationToken);
                        conversationData.PreviousQuestion = ConversationData.Question.Email;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("That them don't sound like numbers. Try again?"), cancellationToken);
                        break;
                    }
                case ConversationData.Question.Email:
                    if(IsInputValid(userInput, out _, ConversationData.Question.Email))
                    {
                        userProfile.Email = userInput;
                        await turnContext.SendActivityAsync(MessageFactory.Text($"That's all I need. Thanks {userProfile.Name}!"), cancellationToken);
                        conversationData.DidFillUserProfile = true;
                        conversationData.PreviousQuestion = ConversationData.Question.None;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Man you are really struggling with this electronic age aren't you? Enter email in the format abc@example.com"), cancellationToken);
                        break;
                    }
            }
        }

        public static bool IsInputValid(string input, out string formattedInput, ConversationData.Question type)
        {
            formattedInput = null;
            switch (type)
            {
                case ConversationData.Question.Name:
                    return (!String.IsNullOrEmpty(input));
                case ConversationData.Question.Age:
                    try
                    {
                        var numberResults = NumberRecognizer.RecognizeNumber(input, Culture.English);
                        foreach(var result in numberResults)
                        {
                            if(result.Resolution.TryGetValue("value", out var value))
                            {
                                formattedInput = value.ToString();
                                return true;
                            }
                        }
                    }
                    catch{
                        return false;
                    }
                    return false;
                case ConversationData.Question.Email:
                    try
                    {
                        var emailResults = SequenceRecognizer.RecognizeEmail(input, Culture.English);
                        foreach (var result in emailResults)
                        {
                            if (result.Resolution.TryGetValue("value", out var value))
                            {
                                formattedInput = value.ToString();
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    return false;
                default:
                    return false;
            }
        } 
    }
}
