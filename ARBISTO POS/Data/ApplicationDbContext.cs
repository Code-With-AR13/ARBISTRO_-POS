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
        public DbSet<DataBaseBackup> DataBaseBackups { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<Expenses> Expenses { get; set; }        
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserRolePermission> UserRolePermissions { get; set; }
        public DbSet<IngredientTransaction> IngredientTransactions { get; set; }
        public DbSet<AppSettting> AppSetttings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<UserRolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.UserRolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<UserRolePermission>()
                .HasOne(rp => rp.UserPermission)
                .WithMany(p => p.UserRolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            modelBuilder.Entity<ItemIngredients>()
                .HasOne(ii => ii.Item)
                .WithMany(i => i.ItemIngredients)
                .HasForeignKey(ii => ii.ItemId);

            modelBuilder.Entity<ItemIngredients>()
                .HasOne(ii => ii.Ingredient)
                .WithMany()
                .HasForeignKey(ii => ii.IngredientId);
        }



        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<ItemIngredients>()
        //        .HasOne(ii => ii.Item)
        //        .WithMany(i => i.ItemIngredients)
        //        .HasForeignKey(ii => ii.ItemId);

        //    modelBuilder.Entity<ItemIngredients>()
        //        .HasOne(ii => ii.Ingredient)
        //        .WithMany()
        //        .HasForeignKey(ii => ii.IngredientId);
        //}

    }
}
