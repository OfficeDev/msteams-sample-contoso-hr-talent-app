using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public sealed class User : Entity
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }
    }
}