using ARBISTO_POS.Models;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options) { }

        // ----------------- DbSets -----------------
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<FoodCategories> FoodCategories { get; set; }
        public DbSet<Ingredients> Ingredients { get; set; }
        public DbSet<Items> Items { get; set; }
        public DbSet<ItemIngredients> ItemIngredients { get; set; }
        public DbSet<Modifier> Modifiers { get; set; }
        public DbSet<ModifierIngredients> ModifierIngredients { get; set; }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<ServiceTables> ServiceTables { get; set; }
        public DbSet<PaymentMethods> PaymentMethods { get; set; }
        public DbSet<PickPoints> PickPoints { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<SaleOrders> SaleOrders { get; set; }
        public DbSet<SaleOrderItems> SaleOrderItems { get; set; }
        public DbSet<HeldOrders> HeldOrders { get; set; }
        public DbSet<HeldOrdersItem> HeldOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItemIngredients>()
                .HasOne(ii => ii.Item)
                .WithMany(i => i.ItemIngredients)
                .HasForeignKey(ii => ii.ItemId);

            modelBuilder.Entity<ItemIngredients>()
                .HasOne(ii => ii.Ingredient)
                .WithMany()
                .HasForeignKey(ii => ii.IngredientId);
        }

    }
}
