// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using BasicEchoBot.Dialogs;
using static BasicEchoBot.Dialogs.Class;

namespace BasicEchoBot
{
    public class BasicEchoBotBot : IBot
    {
        private readonly BasicEchoBotAccessors _accessors;
        private readonly ILogger _logger;

        private readonly DialogSet _dialogs;
        private DialogContext dialogContext;
        private const string PersonalDetailsDialogId = "personalDetailsDialog";

        private Flow flow;

        public BasicEchoBotBot(ConversationState conversationState, ILoggerFactory loggerFactory)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            flow = new Flow();

            _accessors = new BasicEchoBotAccessors(conversationState)
            {
                CounterState = conversationState.CreateProperty<CounterState>(BasicEchoBotAccessors.CounterStateName),
                ConversationDialogState = conversationState.CreateProperty<DialogState>(BasicEchoBotAccessors.DialogStateAccessorName),
            };

            _logger = loggerFactory.CreateLogger<BasicEchoBotBot>();
            _logger.LogTrace("Turn start.");

            WaterfallStep[] steps = new WaterfallStep[]
            {
                GetPersonalDetailsStepAsync,
                HandleAgeStepAsync,
            };

            _dialogs = new DialogSet(_accessors.ConversationDialogState);
            _dialogs.Add(new GenericDialog(flow.flowID, flow));
            _dialogs.Add(new WaterfallDialog("GetPersonalDetails", steps));
            
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {

                dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (results.Status is DialogTurnStatus.Empty)
                {
                    if (results.Result == null)
                    {
                        var userInput = turnContext.Activity.Text;

                        if (userInput.ToString().ToLower().Equals("hi") || userInput.ToString().ToLower().Equals("hello"))
                        {
                            await dialogContext.BeginDialogAsync("GetPersonalDetails", cancellationToken);
                        }
                        else
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Text($"You said'{userInput}'."));
                        }
                    }
                }

            }

            // Save the new turn count into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private async Task<DialogTurnResult> GetPersonalDetailsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(flow.flowID, flow);
        }

        private async Task<DialogTurnResult> HandleAgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Personal Details collected"));

            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}
