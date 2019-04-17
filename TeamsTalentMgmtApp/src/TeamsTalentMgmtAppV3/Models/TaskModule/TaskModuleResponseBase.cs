using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.TaskModule
{
    public class TaskModuleResponseBase
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

    }
}