using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.TaskModule
{
    public class TaskModuleActionData<T>
    {
        [JsonProperty("data")] 
        public T Data { get; set; }
    }
}