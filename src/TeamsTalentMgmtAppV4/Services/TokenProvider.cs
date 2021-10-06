using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using TeamsTalentMgmtAppV4.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Services
{
    public class TokenProvider : ITokenProvider
    {
        private IStatePropertyAccessor<string> _tokenAccessor;

        public TokenProvider(UserState userState)
        {
            _tokenAccessor = userState.CreateProperty<string>("userToken");
        }

        public Task<string> GetTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            => _tokenAccessor.GetAsync(turnContext, cancellationToken: cancellationToken);

        public Task SetTokenAsync(string token, ITurnContext turnContext, CancellationToken cancellationToken)
            => _tokenAccessor.SetAsync(turnContext, token, cancellationToken);
    }
}
