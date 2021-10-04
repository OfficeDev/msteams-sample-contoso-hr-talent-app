using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TeamsTalentMgmtAppV4.Extensions;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Services.Interfaces;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.Commands;

namespace TeamsTalentMgmtAppV4.Bot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly IBotService _botService;
        private readonly AppSettings _appSettings;

        private readonly List<(string CommandName, string DialogName, bool AuthorizationIsNeeded)> _commandDialogs;

        public MainDialog(
            CandidateDetailsDialog candidateDetailsDialog,
            CandidateSummaryDialog candidateSummaryDialog,
            HelpDialog helpDialog,
            NewJobPostingDialog newJobPostingDialog,
            OpenPositionsDialog openPositionsDialog,
            PositionsDetailsDialog positionsDetailsDialog,
            NewTeamDialog newTeamDialog,
            SignOutDialog signOutDialog,
            InstallBotDialog installBotDialog,
            TopCandidatesDialog topCandidatesDialog,
            IBotService botService,
            IOptions<AppSettings> appSettings,
            IBotTelemetryClient botTelemetryClient)
            : base(nameof(MainDialog))
        {
            _botService = botService;
            _appSettings = appSettings.Value;

            AddDialog(candidateDetailsDialog);
            AddDialog(candidateSummaryDialog);
            AddDialog(helpDialog);
            AddDialog(newJobPostingDialog);
            AddDialog(openPositionsDialog);
            AddDialog(positionsDetailsDialog);
            AddDialog(signOutDialog);
            AddDialog(newTeamDialog);
            AddDialog(topCandidatesDialog);
            AddDialog(installBotDialog);

            _commandDialogs = new List<(string, string, bool)>
            {
                (BotCommands.HelpDialogCommand, nameof(HelpDialog), false),
                (BotCommands.SignOutDialogCommand, nameof(SignOutDialog), false),
                (BotCommands.CandidateDetailsDialogCommand, nameof(CandidateDetailsDialog), false),
                (BotCommands.TopCandidatesDialogCommand, nameof(TopCandidatesDialog), false),
                (BotCommands.OpenPositionsDialogCommand, nameof(OpenPositionsDialog), false),
                (BotCommands.PositionsDetailsDialogCommand, nameof(PositionsDetailsDialog), false),
                (BotCommands.NewJobPostingDialog, nameof(NewJobPostingDialog), false),
                (BotCommands.CandidateSummaryDialog, nameof(CandidateSummaryDialog), false),
                (BotCommands.NewTeamDialog, nameof(NewTeamDialog), true),
                (BotCommands.InstallBotDialogCommand, nameof(InstallBotDialog), true)
            };
        }

        public override async Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(dc.Context.Activity.Name) &&
                   string.IsNullOrEmpty(dc.Context.Activity.Text) &&
                   dc.Context.Activity.Value != null)
            {
                var command = JsonConvert.DeserializeObject<ActionCommandBase>(dc.Context.Activity.Value.ToString());
                if (string.IsNullOrEmpty(command?.CommandId))
                {
                    return false;
                }

                IMessageActivity message = null;
                switch (command.CommandId)
                {
                    case AppCommands.OpenNewPosition:
                        message = await _botService.OpenPositionAsync(dc.Context, cancellationToken);
                        break;

                    case AppCommands.LeaveInternalComment:
                        message = await _botService.LeaveInternalCommentAsync(dc.Context, cancellationToken);
                        break;

                    case AppCommands.ScheduleInterview:
                        message = await _botService.ScheduleInterviewAsync(dc.Context, cancellationToken);
                        break;
                }

                if (message != null)
                {
                    await dc.Context.TurnState.Get<IConnectorClient>().Conversations.UpdateActivityWithHttpMessagesAsync(
                        dc.Context.Activity.Conversation.Id,
                        dc.Context.Activity.ReplyToId,
                        (Activity)message,
                        cancellationToken: cancellationToken);
                }
            }

            return true;
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            var activity = innerDc.Context.Activity;

            if (activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(activity.Text))
            {
                var didRoute = await RouteAsync(innerDc, cancellationToken);
                if (!didRoute)
                {
                    var message = $"Sorry, I didn't understand '{activity.Text}'. Type {BotCommands.HelpDialogCommand} to explore commands.";
                    await innerDc.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
                }
            }

            return EndOfTurn;
        }

        protected async Task<bool> RouteAsync(
            DialogContext innerDc,
            CancellationToken cancellationToken = default)
        {
            var activityText = innerDc.Context.Activity.GetActivityTextWithoutMentions()?.Trim() ?? string.Empty;

            var foundDialog = false;
            foreach (var commandDialog in _commandDialogs)
            {
                var isFit = Regex.IsMatch(
                    activityText,
                    $@"^(.*){commandDialog.CommandName}(.*)$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (isFit && !foundDialog)
                {
                    var dialogName = commandDialog.DialogName;

                    await innerDc.BeginDialogAsync(dialogName, cancellationToken: cancellationToken);
                    foundDialog = true;
                }
            }

            return foundDialog;
        }
    }
}
