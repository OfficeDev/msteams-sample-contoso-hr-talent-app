using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using TeamsTalentMgmtAppV3.Constants;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class MainDialog : DispatchDialog
    {
        private readonly IDialogFactory _dialogFactory;

        public MainDialog(IDialogFactory dialogFactory)
        {
            _dialogFactory = dialogFactory;
        }

        [MethodBind]
        [ScorableGroup(4)]
        public async Task Default(IDialogContext context, IActivity activity)
        {
            // The Azure Bot Service sends a six digit magic code and ask the user to type this into the chat window
            // so we need to send this code to the Azure Bot Service
            if (activity is IInvokeActivity invokeActivity)
            {
                if (invokeActivity.IsSigninStateVerificationQuery())
                {
                    var data = invokeActivity.GetSigninStateVerificationQueryData();
                    if (string.IsNullOrEmpty(data?.State))
                    {
                        return;
                    }

                    var connectionName = ConfigurationManager.AppSettings["BotOAuthConnectionName"];
                    var tokenResponse = await context.GetUserTokenAsync(connectionName, data.State);
                    if (tokenResponse != null)
                    {
                        await context.PostAsync("You have signed in successfully. Please type command one more time.");
                    }
                    return;
                }
            }

            var query = string.Empty;
            if (activity is Activity act && act.Text.HasValue())
            {
                query = $" '{act.Text.Trim()}'";
            }

            var message = $"Sorry, I didn't understand{query}. Type {BotCommands.HelpDialogCommand} to explore commands.";
            await context.PostAsync(message);
        }

        [RegexPattern(BotCommands.HelpDialogCommand)]
        [ScorableGroup(2)]
        public void HelpDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<HelpDialog>(), EndDialog);
        }
        
        [RegexPattern(BotCommands.SignOutDialogCommand)]
        [ScorableGroup(2)]
        public void SignOutDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<SignOutDialog>(), EndDialog);
        }

        [RegexPattern(BotCommands.CandidateDetailsDialogCommand)]
        [ScorableGroup(0)]
        public void CandidateDetailsDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<CandidateDetailsDialog>(), EndDialog);
        }

        [RegexPattern(BotCommands.TopCandidatesDialogCommand)]
        [ScorableGroup(0)]
        public void TopCandidatesDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<TopCandidatesDialog>(), EndDialog);
        }

        [RegexPattern(BotCommands.OpenPositionsDialogCommand)]
        [ScorableGroup(0)]
        public void OpenPositionsDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<OpenPositionsDialog>(), EndDialog);
        }
        
        [RegexPattern(BotCommands.PositionsDetailsDialogCommand)]
        [ScorableGroup(1)]
        public void PositionsDetailsDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<PositionsDetailsDialog>(), EndDialog);
        }

        [RegexPattern(BotCommands.NewJobPostingDialog)]
        [ScorableGroup(0)]
        public void NewJobPostingDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<NewJobPostingDialog>(), EndDialog);
        }

        [RegexPattern(BotCommands.CandidateSummaryDialog)]
        [ScorableGroup(0)]
        public void CandidateSummaryDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<CandidateSummaryDialog>(), EndDialog);
        }
        
        [RegexPattern(BotCommands.NewTeamDialog)]
        [ScorableGroup(3)]
        public void NewTeamDialog(IDialogContext context)
        {
            context.Call(_dialogFactory.Create<NewTeamDialog>(), EndDialog);
        }

        private static async Task EndDialog(IDialogContext context, IAwaitable<object> result)
        {
            await result;
            context.Done(string.Empty);
        }
    }
}