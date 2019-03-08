using BasicEchoBot.Validators;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BasicEchoBot.Dialogs
{
    public class PersonalDetailsDialog: ComponentDialog
    {
        private const string InitialId = "personalDetailsDialog";
        private const string NamePrompt = "nameTextPrompt";
        private const string SurnamePrompt = "surnameTextPrompt";
        private const string EmailPrompt = "emailTextPrompt";
        private const string DateOfBirthPrompt = "dateOfBithDateTimePrompt";

        private DateValidator dateValidator = new DateValidator();

        public PersonalDetailsDialog(string id) : base(id)
        {
            InitialDialogId = InitialId;

            AddDialog(new TextPrompt(NamePrompt));
            AddDialog(new TextPrompt(SurnamePrompt));
            AddDialog(new TextPrompt(EmailPrompt));
            AddDialog(new DateTimePrompt(DateOfBirthPrompt, dateValidator.DateValidatorAsync));

            WaterfallStep[] waterfallsteps = new WaterfallStep[]
            {
                NameStepAsync,
                SurnameStepAsync,
                EmailStepAsync,
                DateOfBirthAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(InitialId, waterfallsteps));
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(NamePrompt, new PromptOptions { Prompt = MessageFactory.Text("Please enter your name") });
        }

        private async Task<DialogTurnResult> SurnameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your name as {stepContext.Result}"));
            return await stepContext.PromptAsync(NamePrompt, new PromptOptions { Prompt = MessageFactory.Text("Please enter your surname") });
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your surname as {stepContext.Result}"));
            return await stepContext.PromptAsync(NamePrompt, new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address") });
        }

        private async Task<DialogTurnResult> DateOfBirthAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your email as {stepContext.Result}"));
            return await stepContext.PromptAsync(DateOfBirthPrompt, new PromptOptions { Prompt = MessageFactory.Text("Please enter your date of birth?") });
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var resolution = (stepContext.Result as IList<DateTimeResolution>)?.FirstOrDefault();
            DateTime date = Convert.ToDateTime(resolution.Value ?? resolution.Timex);

            int age = DateTime.Now.Year - date.Year;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your date of birth as as {date.ToString("yyyy/MM/dd")}."), cancellationToken);

            if (age > 18)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You are over 18"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
