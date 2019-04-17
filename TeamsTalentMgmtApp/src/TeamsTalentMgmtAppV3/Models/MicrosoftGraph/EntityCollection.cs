using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public class EntityCollection<T>
    {
        [JsonProperty("value")]
        public List<T> Items { get; set; }
    }
}