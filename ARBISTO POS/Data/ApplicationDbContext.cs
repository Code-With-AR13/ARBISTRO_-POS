using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options) { }

        // ----------------- DbSets -----------------
    }
}
