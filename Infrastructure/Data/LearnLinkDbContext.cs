using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
	public class LearnLinkDbContext : DbContext
	{
		public LearnLinkDbContext(DbContextOptions<LearnLinkDbContext> options) : base(options) { }

		public DbSet<Role> Roles { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<ParentChild> ParentChildren { get; set; }
		public DbSet<Mission> Missions { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<Point> Points { get; set; }
		public DbSet<Transaction> Transactions { get; set; }
		public DbSet<Reward> Rewards { get; set; }
		public DbSet<Shop> Shops { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Redemption> Redemptions { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<Payment> Payments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Role seed
			modelBuilder.Entity<Role>().HasData(
				new Role { RoleId = (int)RoleEnum.Parent, Name = "Parent" },
				new Role { RoleId = (int)RoleEnum.Child, Name = "Child" },
				new Role { RoleId = (int)RoleEnum.Admin, Name = "Admin" }
			);

			modelBuilder.Entity<ParentChild>()
				.HasOne(pc => pc.Parent)
				.WithMany(u => u.ParentRelations)
				.HasForeignKey(pc => pc.ParentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ParentChild>()
				.HasOne(pc => pc.Child)
				.WithMany(u => u.ChildRelations)
				.HasForeignKey(pc => pc.ChildId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Mission>()
				.HasOne(t => t.Parent)
				.WithMany(u => u.MissionAsParent)
				.HasForeignKey(t => t.ParentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Mission>()
				.HasOne(t => t.Child)
				.WithMany(u => u.MissionAsChild)
				.HasForeignKey(t => t.ChildId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Redemption>()
				.HasOne(r => r.Reward)
				.WithMany(rw => rw.Redemptions)
				.HasForeignKey(r => r.RewardId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Redemption>()
				.HasOne(r => r.Child)
				.WithMany(u => u.Redemptions)
				.HasForeignKey(r => r.ChildId)
				.OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Redemption>()
				.HasOne(r => r.Product)
				.WithMany(p => p.Redemptions)
				.HasForeignKey(r => r.ProductId)
				.OnDelete(DeleteBehavior.Restrict);


            // Enum mapping => lưu string thay vì int
            modelBuilder.Entity<Mission>()
				.Property(t => t.Status)
				.HasConversion<string>()
				.HasMaxLength(20);

			modelBuilder.Entity<Submission>()
				.Property(s => s.Status)
				.HasConversion<string>()
				.HasMaxLength(20);

			modelBuilder.Entity<Transaction>()
				.Property(tr => tr.Type)
				.HasConversion<string>()
				.HasMaxLength(20);

			modelBuilder.Entity<Redemption>()
				.Property(r => r.Status)
				.HasConversion<string>()
				.HasMaxLength(20);

			modelBuilder.Entity<Notification>()
				.Property(n => n.Type)
				.HasConversion<string>()
				.HasMaxLength(20);

			modelBuilder.Entity<Payment>()
				.Property(p => p.Status)
				.HasConversion<string>()
				.HasMaxLength(20);

			base.OnModelCreating(modelBuilder);
		}
	}
}
