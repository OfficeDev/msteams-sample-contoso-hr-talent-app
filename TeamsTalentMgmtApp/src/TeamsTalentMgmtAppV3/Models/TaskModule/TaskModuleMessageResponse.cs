using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.TaskModule
{
    public class TaskModuleMessageResponse : TaskModuleResponseBase
    {
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

    }
}