using LearningAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningAPI
{
    public class MyDbContext(IConfiguration configuration) : DbContext
    {
        private readonly IConfiguration _configuration = configuration;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string? connectionString = _configuration.GetConnectionString("MyConnection");
            if (connectionString != null)
            {
                optionsBuilder.UseMySQL(connectionString);
            }
        }


		public required DbSet<Product> Products { get; set; }

		public required DbSet<Order> Orders { get; set; }

		public required DbSet<Category> Categories { get; set; }

		public required DbSet<Cart> Carts { get; set; }

        public required DbSet<Reward> Rewards { get; set; }

        public required DbSet<User> Users { get; set; }
    

		public required DbSet<Inventory> Inventories { get; set; }
		public required DbSet<Promotion> Promotions { get; set; }
	}
}