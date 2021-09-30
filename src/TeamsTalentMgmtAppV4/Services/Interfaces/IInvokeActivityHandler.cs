using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace TeamsTalentMgmtAppV4.Services.Interfaces
{
    public interface IInvokeActivityHandler
    {
        Task<InvokeResponse> HandleMessagingExtensionQueryAsync(
            ITurnContext turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleMessagingExtensionOnCardButtonClickedAsync(
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleSigninVerifyStateAsync(
            ITurnContext<IInvokeActivity> turnContext,
            SigninStateVerificationQuery query,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleAppBasedLinkQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            AppBasedLinkQuery query,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleFileConsentDeclineResponse(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken);

        Task<InvokeResponse> HandleFileConsentAcceptResponse(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken);
    }
}
