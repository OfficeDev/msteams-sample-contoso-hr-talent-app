using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtAppV4.Extensions;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Models.TemplateModels;
using TeamsTalentMgmtAppV4.Services.Templates;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Bot.Dialogs
{
    public class CandidateDetailsDialog : Dialog
    {
        private readonly AppSettings _appSettings;
        private readonly ICandidateService _candidateService;
        private readonly IRecruiterService _recruiterService;
        private readonly CandidatesTemplate _candidatesTemplate;

        public CandidateDetailsDialog(
            IOptions<AppSettings> appSettings,
            CandidatesTemplate candidatesTemplate,
            IRecruiterService recruiterService,
            ICandidateService candidateService)
            : base(nameof(CandidateDetailsDialog))
        {
            _candidateService = candidateService;
            _candidatesTemplate = candidatesTemplate;
            _recruiterService = recruiterService;
            _appSettings = appSettings.Value;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var text = dc.Context.Activity.GetTextWithoutCommand(BotCommands.CandidateDetailsDialogCommand);
            var candidates = await _candidateService.Search(text, 15, cancellationToken);
            var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);

            var templateModel = new CandidateTemplateModel
            {
                ListCardTitle = "I found following candidates:",
                BotCommand = string.Empty,
                Items = candidates,
                Interviewers = interviewers,
                AppSettings = _appSettings,
                NoItemsLabel = "You don't have such candidates."
            };

            await _candidatesTemplate.ReplyWith(dc.Context, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
