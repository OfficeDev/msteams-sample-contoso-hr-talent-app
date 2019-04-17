using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public sealed class TeamMessagingSettings
    {
        [JsonProperty("allowUserEditMessages")]
        public bool AllowUserEditMessages { get; set; }

        [JsonProperty("allowUserDeleteMessages")]
        public bool AllowUserDeleteMessages { get; set; }
    }
}