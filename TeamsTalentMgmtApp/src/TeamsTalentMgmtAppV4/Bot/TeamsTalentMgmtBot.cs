using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using TeamsTalentMgmtAppV4.Bot.Dialogs;
using TeamsTalentMgmtAppV4.Extensions;
using TeamsTalentMgmtAppV4.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Bot
{
    public class TeamsTalentMgmtBot : TeamsActivityHandler
    {
        private readonly MainDialog _mainDialog;
        private readonly IInvokeActivityHandler _invokeActivityHandler;
        private readonly IBotService _botService;
        private readonly ConversationState _conversationState;

        public TeamsTalentMgmtBot(
            MainDialog mainDialog,
            IInvokeActivityHandler invokeActivityHandler,
            IBotService botService,
            ConversationState conversationState)
        {
            _mainDialog = mainDialog;
            _conversationState = conversationState;
            _botService = botService;
            _invokeActivityHandler = invokeActivityHandler;
        }

        protected override Task<InvokeResponse> OnSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, SigninStateVerificationQuery query, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleSigninVerifyStateAsync(turnContext, query, cancellationToken);

        protected override Task<InvokeResponse> OnMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionQueryAsync(turnContext, query, cancellationToken);

        protected override Task<InvokeResponse> OnMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionFetchTaskAsync(turnContext, cancellationToken);

        protected override Task<InvokeResponse> OnMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionSubmitActionAsync(turnContext, cancellationToken);

        protected override Task<InvokeResponse> OnMessagingExtensionOnCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionOnCardButtonClickedAsync(turnContext, cancellationToken);

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            => _botService.HandleMembersAddedAsync(turnContext, cancellationToken);

        protected override Task<InvokeResponse> OnAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleAppBasedLinkQueryAsync(turnContext, query, cancellationToken);

        protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.HasFileAttachments())
            {
                return _botService.HandleFileAttachments(turnContext, cancellationToken);
            }

            // continue process for text messages
            return _mainDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(TeamsTalentMgmtBot)), cancellationToken);
        }

        protected override Task<InvokeResponse> OnFileConsentDeclineAsync(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleFileConsentDeclineResponse(turnContext, fileConsentCardResponse, cancellationToken);

        protected override Task<InvokeResponse> OnFileConsentAcceptAsync(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleFileConsentAcceptResponse(turnContext, fileConsentCardResponse, cancellationToken);
    }
}
