using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Solutions.Dialogs;
using Microsoft.Bot.Builder.Solutions.Extensions;
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
    public abstract class InterruptableDialog : ComponentDialog
    {
        public InterruptableDialog(string dialogId, IBotTelemetryClient telemetryClient)
            : base(dialogId)
        {
            PrimaryDialogName = dialogId;
            TelemetryClient = telemetryClient;
        }

        public string PrimaryDialogName { get; set; }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc.Dialogs.Find(PrimaryDialogName) != null)
            {
                // Overrides default behavior which starts the first dialog added to the stack (i.e. Cancel waterfall)
                return await dc.BeginDialogAsync(PrimaryDialogName, options).ConfigureAwait(false);
            }
            else
            {
                // If we don't have a matching dialog, start the initial dialog
                return await dc.BeginDialogAsync(InitialDialogId, options).ConfigureAwait(false);
            }
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var status = await OnInterruptDialogAsync(dc, cancellationToken).ConfigureAwait(false);

            if (status == InterruptionAction.Resume)
            {
                // Resume the waiting dialog after interruption
                await dc.RepromptDialogAsync().ConfigureAwait(false);
                return EndOfTurn;
            }
            else if (status == InterruptionAction.Waiting)
            {
                // Stack is already waiting for a response, shelve inner stack
                return EndOfTurn;
            }

            return await base.OnContinueDialogAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected abstract Task<InterruptionAction> OnInterruptDialogAsync(DialogContext dc, CancellationToken cancellationToken);
    }

    public enum InterruptionAction
    {
        End,
        Resume,
        Waiting,
        NoAction
    }

    public abstract class RouterDialog : InterruptableDialog
    {
        public RouterDialog(string dialogId, IBotTelemetryClient telemetryClient)
            : base(dialogId, telemetryClient)
        {
            TelemetryClient = telemetryClient;
        }

        protected override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnContinueDialogAsync(innerDc, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var status = await OnInterruptDialogAsync(innerDc, cancellationToken).ConfigureAwait(false);

            if (status == InterruptionAction.Resume)
            {
                // Resume the waiting dialog after interruption
                await innerDc.RepromptDialogAsync().ConfigureAwait(false);
                return EndOfTurn;
            }
            else if (status == InterruptionAction.Waiting)
            {
                // Stack is already waiting for a response, shelve inner stack
                return EndOfTurn;
            }
            else
            {
                var activity = innerDc.Context.Activity;

                if (activity.IsStartActivity())
                {
                    await OnStartAsync(innerDc).ConfigureAwait(false);
                }

                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        {
                            // Note: This check is a workaround for adaptive card buttons that should map to an event (i.e. startOnboarding button in intro card)
                            if (activity.Value != null)
                            {
                                await OnEventAsync(innerDc).ConfigureAwait(false);
                            }
                            else
                            {
                                var result = await innerDc.ContinueDialogAsync().ConfigureAwait(false);

                                switch (result.Status)
                                {
                                    case DialogTurnStatus.Empty:
                                        {
                                            await RouteAsync(innerDc).ConfigureAwait(false);
                                            break;
                                        }

                                    case DialogTurnStatus.Complete:
                                        {
                                            // End active dialog
                                            await innerDc.EndDialogAsync().ConfigureAwait(false);
                                            break;
                                        }

                                    default:
                                        {
                                            break;
                                        }
                                }
                            }

                            // If the active dialog was ended on this turn (either on single-turn dialog, or on continueDialogAsync) run CompleteAsync method.
                            if (innerDc.ActiveDialog == null)
                            {
                                await CompleteAsync(innerDc).ConfigureAwait(false);
                            }

                            break;
                        }

                    case ActivityTypes.Event:
                        {
                            await OnEventAsync(innerDc).ConfigureAwait(false);
                            break;
                        }

                    case ActivityTypes.Invoke:
                        {
                            // Used by Teams for Authentication scenarios.
                            await innerDc.ContinueDialogAsync().ConfigureAwait(false);
                            break;
                        }

                    default:
                        {
                            await OnSystemMessageAsync(innerDc).ConfigureAwait(false);
                            break;
                        }
                }

                return EndOfTurn;
            }
        }

        protected override Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.OnEndDialogAsync(context, instance, reason, cancellationToken);
        }

        protected override Task OnRepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.OnRepromptDialogAsync(turnContext, instance, cancellationToken);
        }

        /// <summary>
        /// Called when the inner dialog stack is empty.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task RouteAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called when the inner dialog stack is complete.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="result">The dialog result when inner dialog completed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual async Task CompleteAsync(DialogContext innerDc, DialogTurnResult result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await innerDc.EndDialogAsync(result, cancellationToken);
        }

        /// <summary>
        /// Called when an event activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task OnEventAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when a system activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task OnSystemMessageAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when a conversation update activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task OnStartAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        protected override Task<InterruptionAction> OnInterruptDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(InterruptionAction.NoAction);
        }
    }

    public class MainDialog : RouterDialog
    {
        private readonly IBotService _botService;
        private readonly AppSettings _appSettings;

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
            : base(nameof(MainDialog), botTelemetryClient)
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
            AddDialog(new OAuthPrompt(nameof(OAuthPrompt), new OAuthPromptSettings
            {
                ConnectionName = _appSettings.OAuthConnectionName,
                Text = "Please sign in to proceed.",
                Title = "Sign In",
                Timeout = 9000
            }));
        }

        protected override async Task RouteAsync(
            DialogContext innerDc,
            CancellationToken cancellationToken = default)
        {
            var activityText = innerDc.Context.Activity.GetActivityTextWithoutMentions()?.Trim() ?? string.Empty;

            List<(string CommandName, string DialogName, bool AuthorizationIsNeeded)> commandDialogs = new List<(string, string, bool)>
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

            var sentAnswer = false;
            foreach (var commandDialog in commandDialogs)
            {
                var isFit = Regex.IsMatch(
                    activityText,
                    $@"^(.*){commandDialog.CommandName}(.*)$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (isFit && !sentAnswer)
                {
                    var dialogName = commandDialog.DialogName;
                    if (commandDialog.AuthorizationIsNeeded)
                    {
                        var connectionName = _appSettings.OAuthConnectionName;
                        var token = await ((IUserTokenProvider)innerDc.Context.Adapter)
                            .GetUserTokenAsync(innerDc.Context, connectionName, null, cancellationToken);

                        if (string.IsNullOrEmpty(token?.Token))
                        {
                            dialogName = nameof(OAuthPrompt);
                        }
                    }

                    await innerDc.BeginDialogAsync(dialogName, cancellationToken: cancellationToken);
                    sentAnswer = true;
                }
            }

            if (!sentAnswer)
            {
                var message = $"Sorry, I didn't understand '{activityText}'. Type {BotCommands.HelpDialogCommand} to explore commands.";
                await innerDc.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
            }
        }

        protected override async Task OnEventAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(innerDc.Context.Activity.Name) &&
                   string.IsNullOrEmpty(innerDc.Context.Activity.Text) &&
                   innerDc.Context.Activity.Value != null)
            {
                var command = JsonConvert.DeserializeObject<ActionCommandBase>(innerDc.Context.Activity.Value.ToString());
                if (string.IsNullOrEmpty(command?.CommandId))
                {
                    return;
                }

                IMessageActivity message = null;
                switch (command.CommandId)
                {
                    case AppCommands.OpenNewPosition:
                        message = await _botService.OpenPositionAsync(innerDc.Context, cancellationToken);
                        break;

                    case AppCommands.LeaveInternalComment:
                        message = await _botService.LeaveInternalCommentAsync(innerDc.Context, cancellationToken);
                        break;

                    case AppCommands.ScheduleInterview:
                        message = await _botService.ScheduleInterviewAsync(innerDc.Context, cancellationToken);
                        break;
                }

                if (message != null)
                {
                    await innerDc.Context.TurnState.Get<IConnectorClient>().Conversations.UpdateActivityWithHttpMessagesAsync(
                        innerDc.Context.Activity.Conversation.Id,
                        innerDc.Context.Activity.ReplyToId,
                        (Activity)message,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}
