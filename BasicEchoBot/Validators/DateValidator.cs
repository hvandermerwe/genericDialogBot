using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BasicEchoBot.Validators
{
    public class DateValidator
    {
        public async Task<bool> DateValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (promptContext.Recognized.Succeeded)
            {
                var value = promptContext.Recognized.Value.FirstOrDefault();
                var date = DateTime.Parse(value.Value ?? value.Timex);
                int age = DateTime.Now.Year - date.Year;

                if (age > 18)
                {
                    return true;
                }
                else
                {
                    await promptContext.Context.SendActivityAsync("You are under 18", cancellationToken: cancellationToken);
                    await promptContext.Context.SendActivityAsync("Please enter your date of birth.", cancellationToken: cancellationToken);

                    return false;
                }

            }

            await promptContext.Context.SendActivityAsync("This aint no date.", cancellationToken: cancellationToken);
            await promptContext.Context.SendActivityAsync("Please enter your date of birth.", cancellationToken: cancellationToken);

            return false;
        }
    }
}
