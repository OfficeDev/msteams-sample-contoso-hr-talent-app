using Newtonsoft.Json;

namespace TeamTalentMgmtApp.Shared.Models.Commands
{
    public class ActionCommandBase
    {
        [JsonProperty("commandId")]
        public string CommandId { get; set; }
    }
}