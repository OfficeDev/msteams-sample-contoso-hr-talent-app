using Microsoft.EntityFrameworkCore;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.Services.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Recruiter> Recruiters { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<SubscribeEvent> SubscribeEvents { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<TeamsChannelData> TeamsChannelData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "TeamsTalentMgmtAppV3Database");
        }
    }
}