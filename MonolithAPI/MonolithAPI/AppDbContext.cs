using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonolithAPI.Models;

namespace MonolithAPI;

public class AppDbContext : IdentityDbContext<UserModel, RoleModel, Guid>
{
    public DbSet<ProductModel> Products { get; set; }
    public DbSet<ProfileModel> Profiles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RoleModel>().HasData(
            new RoleModel { Id = Guid.Parse("dfb04ae0-0184-41ce-ac5d-1bee1ade19b3"), Name = "Customer", NormalizedName = "CUSTOMER", Description = "Customer role" },
            new RoleModel { Id = Guid.Parse("d1b172ba-4d15-4505-8de0-b43588da3359"), Name = "Seller", NormalizedName = "SELLER", Description = "Seller role" }
        );

        builder.Entity<ProductModel>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.CreatedBy)
            .IsRequired(false);
    }
}
