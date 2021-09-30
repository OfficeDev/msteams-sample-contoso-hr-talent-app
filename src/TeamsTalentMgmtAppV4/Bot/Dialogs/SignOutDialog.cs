using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtAppV4.Models;

namespace TeamsTalentMgmtAppV4.Bot.Dialogs
{
    public class SignOutDialog : Dialog
    {
        private readonly AppSettings _appSettings;

        public SignOutDialog(IOptions<AppSettings> appSettings)
            : base(nameof(SignOutDialog))
        {
            _appSettings = appSettings.Value;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var notificationMessage = "You are not logged in yet.";
            var connectionName = _appSettings.OAuthConnectionName;
            var adapter = (IUserTokenProvider)dc.Context.Adapter;
            var token = await adapter.GetUserTokenAsync(dc.Context, connectionName, null, cancellationToken);
            if (token?.Token != null)
            {
                await ((IUserTokenProvider)dc.Context.Adapter).SignOutUserAsync(dc.Context, connectionName, cancellationToken: cancellationToken);
                notificationMessage = "You've been logged out.";
            }

            await dc.Context.SendActivityAsync(notificationMessage, cancellationToken: cancellationToken);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
