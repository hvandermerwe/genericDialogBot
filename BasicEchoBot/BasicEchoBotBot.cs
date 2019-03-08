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
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class BasicEchoBotBot : IBot
    {
        private readonly BasicEchoBotAccessors _accessors;
        private readonly ILogger _logger;

        private readonly DialogSet _dialogs;

        private DialogContext dialogContext;

        private const string PersonalDetailsDialogId = "personalDetailsDialog";

        private Flow flow;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
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

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
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
