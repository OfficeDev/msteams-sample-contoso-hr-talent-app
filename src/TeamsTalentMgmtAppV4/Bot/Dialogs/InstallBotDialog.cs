using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Services.Interfaces;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Bot.Dialogs
{
    public class InstallBotDialog : Dialog
    {
        private readonly IGraphApiService _graphApiService;
        private readonly IRecruiterService _recruiterService;
        private readonly AppSettings _appSettings;

        public InstallBotDialog(
            IGraphApiService graphApiService,
            IOptions<AppSettings> appSettings,
            IRecruiterService recruiterService)
            : base(nameof(InstallBotDialog))
        {
            _appSettings = appSettings.Value;
            _graphApiService = graphApiService;
            _recruiterService = recruiterService;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var hiringManagers = await _recruiterService.GetAllHiringManagers(cancellationToken);
            var token = await ((IUserTokenProvider)dc.Context.Adapter)
                            .GetUserTokenAsync(dc.Context, _appSettings.OAuthConnectionName, null, cancellationToken);
            var successfullyInstalled = new List<string>();
            foreach (var manager in hiringManagers)
            {
                if (await _graphApiService.InstallBotForUser(token.Token, manager.Alias, cancellationToken))
                {
                    successfullyInstalled.Add(manager.Name);
                }
            }

            var message = successfullyInstalled.Count == 0
                ? "Bot wasn't installed to any of hiring manager."
                : $"Bot was successfully installed for: {string.Join(", ", successfullyInstalled)}";

            await dc.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
