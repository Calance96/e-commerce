using ECommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; } 

        public DbSet<Product> Products { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<EntityActionType> EntityActionTypes { get; set; }

        public DbSet<OrderAction> OrderActions { get; set; }

        public DbSet<CategoryAuditTrail> CategoryAuditTrails { get; set; }

        public DbSet<ProductAuditTrail> ProductAuditTrails { get; set; }

        public DbSet<OrderAuditTrail> OrderAuditTrails { get; set; }
        
        public DbSet<ProductCategory> ProductCategories { get; set; }
    }
}
