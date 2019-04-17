using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.TaskModule
{
    public class TaskModuleContinueResponse : TaskModuleResponseBase
    {
        [JsonProperty(PropertyName = "value")]
        public TaskModuleTaskInfo Value { get; set; }
    }
}