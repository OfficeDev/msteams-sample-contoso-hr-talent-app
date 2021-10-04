using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using TeamsTalentMgmtAppV4.Extensions;
using TeamsTalentMgmtAppV4.Models.TemplateModels;
using TeamsTalentMgmtAppV4.Services.Templates;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Bot.Dialogs
{
    public class CandidateSummaryDialog : Dialog
    {
        private readonly ICandidateService _candidateService;
        private readonly CandidatesTemplate _candidatesTemplate;

        public CandidateSummaryDialog(
            ICandidateService candidateService,
            CandidatesTemplate candidatesTemplate)
            : base(nameof(CandidateSummaryDialog))
        {
            _candidateService = candidateService;
            _candidatesTemplate = candidatesTemplate;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = default,
            CancellationToken cancellationToken = default)
        {
            var text = dc.Context.Activity.GetTextWithoutCommand(BotCommands.CandidateSummaryDialog);
            var candidates = await _candidateService.Search(text, 15, cancellationToken);

            var templateModel = new CandidateTemplateModel
            {
                BotCommand = BotCommands.CandidateSummaryDialog,
                ListCardTitle = "Please select candidate:",
                Items = candidates,
                NoItemsLabel = "You don't have such candidates."
            };

            await _candidatesTemplate.ReplyWith(dc.Context, TemplateConstants.CandidateAsFileConsentCardWithMultipleItems, templateModel);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
