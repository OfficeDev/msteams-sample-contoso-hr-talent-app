using System;
using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.Commands
{
    public sealed class ScheduleInterviewCommand : ActionCommandBase
    {
        [JsonProperty("candidateId")] 
        public int CandidateId { get; set; }

        [JsonProperty("interviewDate")] 
        public DateTime InterviewDate { get; set; }
        
        [JsonProperty("interviewType")] 
        public string InterviewType { get; set; }
    
        [JsonProperty("isRemote")] 
        public bool IsRemote { get; set; }

        [JsonProperty("interviewerId")] 
        public int InterviewerId { get; set; }
    }
}