﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace TeamsTalentMgmtAppV4.Services.Interfaces
{
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken);
        Task SetTokenAsync(string token, ITurnContext turnContext, CancellationToken cancellationToken);
    }
}
