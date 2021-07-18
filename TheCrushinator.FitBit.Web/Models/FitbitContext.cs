using Microsoft.EntityFrameworkCore;
using TheCrushinator.FitBit.Web.Extensions;

namespace TheCrushinator.FitBit.Web.Models
{
    public class FitbitContext : DbContext
    {
        public FitbitContext(DbContextOptions<FitbitContext> options) : base(options)
        {
        }

        public DbSet<ScaleEntry> BeurerWeightEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetDateKindToUtc();
        }
    }
}
