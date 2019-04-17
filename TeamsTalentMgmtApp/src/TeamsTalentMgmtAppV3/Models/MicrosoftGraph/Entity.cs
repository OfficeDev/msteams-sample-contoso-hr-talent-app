using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public abstract class Entity
    {
        protected internal Entity()
        {
            // Don't allow initialization of abstract entity types
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("@odata.type")]
        public string ODataType { get; set; }

        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }
    }
}