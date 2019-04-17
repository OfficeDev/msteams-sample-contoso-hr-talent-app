using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public sealed class TeamsTab : Entity
    {
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "teamsApp@odata.bind")]
        public string TeamsAppId { get; set; }

        [JsonProperty(PropertyName = "webUrl")]
        public string WebUrl { get; set; }
       
        [JsonProperty(PropertyName = "configuration")]
        public TeamsTabConfiguration Configuration { get; set; }
    }
}