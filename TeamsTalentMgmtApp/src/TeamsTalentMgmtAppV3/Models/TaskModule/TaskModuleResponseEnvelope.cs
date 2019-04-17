using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.TaskModule
{
    public class TaskModuleResponseEnvelope
    {
        [JsonProperty(PropertyName = "task")]
        public TaskModuleResponseBase Task { get; set; }

    }
}