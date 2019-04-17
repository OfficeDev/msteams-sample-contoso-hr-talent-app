using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.Commands
{
    public class ActionCommandBase
    {
        [JsonProperty("commandId")] 
        public string CommandId { get; set; }
    }
}