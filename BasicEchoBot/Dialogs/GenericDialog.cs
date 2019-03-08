using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BasicEchoBot.Dialogs.Class;

namespace BasicEchoBot.Dialogs
{
    public class GenericDialog : ComponentDialog
    {
        private string InitialId;
        private Dictionary<string, string> dict;
        string[] currentVariable;
        string[] currentQuestion;
        string[] currentBranchFlowId;
        string[] currentBranchText;

        Flow _flow;
        WaterfallStep[] waterfallsteps;

        public GenericDialog(string id, Flow flow) : base(flow.flowID)
        {
            InitialDialogId = InitialId = flow.flowID;
            _flow = flow;

            //These WILL be removed #HACK
            currentVariable = new string[flow.questions.Count];
            currentQuestion = new string[flow.questions.Count];
            currentBranchFlowId = new string[flow.questions.Count];
            currentBranchText = new string[flow.questions.Count];

            dict = new Dictionary<string, string>();

            waterfallsteps = new WaterfallStep[flow.questions.Count + 2];
            
            AddDialog(new TextPrompt("TextDialogPromptThing"));
            AddDialog(new DateTimePrompt("DateDialogPromt"));

            foreach (var o in flow.questions.OfType<Question>().Select((question, index) => new { question, index }))
            {
                switch(o.question.Type)
                {
                    case "Text":
                        currentVariable[o.index] = o.question.Value;
                        currentQuestion[o.index] = o.question.Text;
                        //currentBranchFlowId[o.index] = o.question.Branch.FlowId;
                        //currentBranchText[o.index] = o.question.Branch.Text;
                        waterfallsteps[o.index] = TextPromptAsync;
                    break;

                    case "Date":
                        currentVariable[o.index] = o.question.Value;
                        currentQuestion[o.index] = o.question.Text;
                        waterfallsteps[o.index] = DatePromptAsync;
                    break;
                }
            }

            waterfallsteps[flow.questions.Count] = FinalPromptAsync;

            AddDialog(new WaterfallDialog(InitialId, waterfallsteps));
        }

        private async Task<DialogTurnResult> TextPromptAsync(WaterfallStepContext stepContext , CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                //if the result from the previous prompt is equal to the text required to start new branch then start the new dialog
                //if (((string)stepContext.Result).Equals(currentBranchText[stepContext.Index]))
                //{
                //    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You entered {stepContext.Result}"), cancellationToken);
                //    await stepContext.BeginDialogAsync(currentBranchFlowId[stepContext.Index]);
                //}
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You entered {stepContext.Result}"), cancellationToken);
                dict.Add(currentVariable[stepContext.Index-1], stepContext.Result.ToString());
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Dict Variable Name:{currentVariable[stepContext.Index - 1]} value: {dict[currentVariable[stepContext.Index - 1]]}"), cancellationToken);
            }
            
            var prompt = new PromptOptions { Prompt = MessageFactory.Text(currentQuestion[stepContext.Index]) };
            
            return await stepContext.PromptAsync("TextDialogPromptThing", prompt);
        }

        private async Task<DialogTurnResult> DatePromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompt = new PromptOptions { Prompt = MessageFactory.Text(currentQuestion[stepContext.Index]) };
            return await stepContext.PromptAsync("DateDialogPromt", prompt);
        }

        private async Task<DialogTurnResult> FinalPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Final step {dict["fname"]} {dict["lname"]}"), cancellationToken);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Dict Variable Name:{dict["fname"]}"), cancellationToken);

            var resolution = (stepContext.Result as IList<DateTimeResolution>)?.FirstOrDefault();
            DateTime date = Convert.ToDateTime(resolution.Value ?? resolution.Timex);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You entered {date.ToString("yyyy/MM/dd")}"), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
