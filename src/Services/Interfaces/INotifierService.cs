using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface INotifierService
    {
        Task<bool> SendProactiveNotification(string upnOrOid, string tenantId, IActivity activityToSend, CancellationToken cancellationToken = default);
    }
}
