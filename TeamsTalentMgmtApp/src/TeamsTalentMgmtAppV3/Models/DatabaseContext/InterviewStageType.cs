using System.Runtime.Serialization;

namespace TeamsTalentMgmtAppV3.Models.DatabaseContext
{
    public enum InterviewStageType
    {
        [EnumMember(Value = "Applied")] Applied,

        [EnumMember(Value = "Screening")] Screening,

        [EnumMember(Value = "Interviewing")] Interviewing,
        
        [EnumMember(Value = "Offered")] Offered
    }
}