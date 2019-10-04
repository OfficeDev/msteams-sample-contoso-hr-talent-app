using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using TeamTalentMgmtApp.Shared.Services.Data;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<object>
    {
        private readonly DatabaseContext _databaseContext;

        public HelpDialog(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var candidate = _databaseContext.Candidates.FirstOrDefault();
            var helpMessage = "Here's what I can help you do: \n\n"
                              + $"* Show details about a candidate, for example: candidate details {candidate?.Name} \n"
                              + $"* Show summary about a candidate, for example: summary {candidate?.Name} \n"
                              + $"* Show top recent candidates for a Position ID, for example: top candidates {candidate?.Position.PositionExternalId} \n"
                              + "* Create a new job posting \n"
                              + "* List all your open positions";

            await context.PostAsync(helpMessage);
            context.Done(string.Empty);
        }
    }
}