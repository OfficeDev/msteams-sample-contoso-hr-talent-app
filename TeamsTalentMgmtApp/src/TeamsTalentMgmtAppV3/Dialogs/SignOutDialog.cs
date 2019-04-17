using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class SignOutDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.SignOutUserAsync("AzureADv2");
            context.Done(string.Empty);
        }
    }
}