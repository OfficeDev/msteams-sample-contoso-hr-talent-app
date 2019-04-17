namespace TeamsTalentMgmtAppV3.Models.DatabaseContext
{
    public class TeamsChannelData
    {
        public int TeamsChannelDataId { get; set; }

        public int RecruiterId { get; set; }

        public string AccountId { get; set; }
        
        public string ServiceUrl { get; set; }

        public string TenantId { get; set; }
    }
}