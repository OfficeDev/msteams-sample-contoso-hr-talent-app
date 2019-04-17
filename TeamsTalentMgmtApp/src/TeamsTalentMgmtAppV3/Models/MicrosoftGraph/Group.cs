using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public sealed class Group : Entity
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        
        [JsonProperty("groupTypes")]
        public string[] GroupTypes { get; set; }
        
        [JsonProperty("mailEnabled")]
        public bool MailEnabled { get; set; }
        
        [JsonProperty("mailNickname")]
        public string MailNickname { get; set; }
        
        [JsonProperty("securityEnabled")]
        public bool SecurityEnabled { get; set; }
        
        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty(PropertyName = "members@odata.bind", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Members { get; set; }

        [JsonProperty(PropertyName = "owners@odata.bind", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Owners { get; set; }
    }
}