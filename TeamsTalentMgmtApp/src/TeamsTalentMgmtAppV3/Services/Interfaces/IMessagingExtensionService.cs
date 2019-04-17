using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface IMessagingExtensionService
    {
        Task<HttpResponseMessage> HandleInvokeRequest(HttpRequestMessage request, Activity activity, CancellationToken cancellationToken = default);
	}
}
