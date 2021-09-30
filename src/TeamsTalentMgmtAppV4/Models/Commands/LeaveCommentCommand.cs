using Newtonsoft.Json;

namespace TeamTalentMgmtApp.Shared.Models.Commands
{
    public sealed class LeaveCommentCommand : ActionCommandBase
    {
        [JsonProperty("candidateId")]
        public int CandidateId { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}