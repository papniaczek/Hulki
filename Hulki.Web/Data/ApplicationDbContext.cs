using Hulki.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ENCJE SŁOWNIKOWE
        public DbSet<TherapyType> TherapyTypes { get; set; }
        public DbSet<ReportStatus> ReportStatuses { get; set; }
        public DbSet<ItemRarity> ItemRarities { get; set; }
        public DbSet<GameType> GameTypes { get; set; }
        public DbSet<ForumCategory> ForumCategories { get; set; }
        public DbSet<FileType> FileTypes { get; set; }

        // ENCJE NIESŁOWNIKOWE
        public DbSet<TherapyGroup> TherapyGroups { get; set; }
        public DbSet<PatientGroup> PatientGroups { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<ReportAttachment> ReportAttachments { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<RewardItem> RewardItems { get; set; }
        public DbSet<PatientInventory> PatientInventories { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<ForumTopic> ForumTopics { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }

        // KONFIGURACJA ZAAWANSOWANYCH RELACJI
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Pacjent <-> Grupa Wsparcia
            builder.Entity<PatientGroup>()
                .HasKey(pg => new { pg.AppUserId, pg.TherapyGroupId });

            // Pacjent <-> Ekwipunek (Nagrody)
            builder.Entity<PatientInventory>()
                .HasKey(pi => new { pi.AppUserId, pi.RewardItemId });

            // Użytkownik <-> Portfel
            builder.Entity<AppUser>()
                .HasOne(u => u.Wallet)
                .WithOne(w => w.AppUser)
                .HasForeignKey<Wallet>(w => w.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Użytkownik <-> Dzienniczek
            builder.Entity<DailyReport>()
                .HasOne(d => d.AppUser)
                .WithMany(u => u.DailyReports)
                .HasForeignKey(d => d.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<ForumPost>()
                .HasOne(fp => fp.AppUser)
                .WithMany()
                .HasForeignKey(fp => fp.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
}