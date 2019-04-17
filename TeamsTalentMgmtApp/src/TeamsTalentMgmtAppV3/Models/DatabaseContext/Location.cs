namespace TeamsTalentMgmtAppV3.Models.DatabaseContext
{
    public sealed class Location
    {
        public int LocationId { get; set; }
        
        public string City { get; set; }

        public string State { get; set; }
    }
}