using Microsoft.EntityFrameworkCore;

namespace TheCrushinator.FitBit.Web.Models
{
    public class FitbitContext : DbContext
    {
        public FitbitContext(DbContextOptions<FitbitContext> options) : base(options)
        {
        }

        public DbSet<ScaleEntry> BeurerWeightEntries { get; set; }
    }
}
