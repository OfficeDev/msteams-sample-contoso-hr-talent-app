using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public sealed class TeamsTabConfiguration
    {
        [JsonProperty(PropertyName = "entityId")]
        public string EntityId { get; set; }
        
        [JsonProperty(PropertyName = "contentUrl")]
        public string ContentUrl { get; set; }
        
        [JsonProperty(PropertyName = "removeUrl")]
        public string RemoveUrl { get; set; }
        
        [JsonProperty(PropertyName = "websiteUrl")]
        public string WebsiteUrl { get; set; }
    }
}