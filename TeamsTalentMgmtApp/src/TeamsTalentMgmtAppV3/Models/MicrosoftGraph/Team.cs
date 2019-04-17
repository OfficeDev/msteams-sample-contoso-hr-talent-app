using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public sealed class Team : Entity
    {
        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("guestSettings")]
        public TeamPerRoleSettings GuestSettings { get; set; }
        
        [JsonProperty("memberSettings")]
        public TeamPerRoleSettings MemberSettings { get; set; }
        
        [JsonProperty("messagingSettings")]
        public TeamMessagingSettings MessagingSettings { get; set; }
        
        [JsonProperty("funSettings")]
        public TeamFunSettings FunSettings { get; set; }
    }
}