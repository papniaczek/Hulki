using Hulki.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Istniejące DbSety (bez zmian) ──────────────────────────────────────
    public DbSet<TherapyType> TherapyTypes { get; set; }
    public DbSet<ReportStatus> ReportStatuses { get; set; }
    public DbSet<ItemRarity> ItemRarities { get; set; }
    public DbSet<GameType> GameTypes { get; set; }
    public DbSet<ForumCategory> ForumCategories { get; set; }
    public DbSet<FileType> FileTypes { get; set; }
    public DbSet<MoodType> MoodTypes { get; set; }
    public DbSet<ConsultationStatus> ConsultationStatuses { get; set; }
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
    public DbSet<GroupMessage> GroupMessages { get; set; }
    public DbSet<GroupQuest> GroupQuests { get; set; }
    public DbSet<QuestSubmission> QuestSubmissions { get; set; }
    public DbSet<GroupResource> GroupResources { get; set; }
    public DbSet<Consultation> Consultations { get; set; }
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<SurveyAnswer> SurveyAnswers { get; set; }
    public DbSet<SurveySubmission> SurveySubmissions { get; set; }
    public DbSet<MoodLog> MoodLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AchievementBadge> AchievementBadges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<GoalMilestone> GoalMilestones { get; set; }
    public DbSet<TherapyGoal> TherapyGoals { get; set; }
    public DbSet<TherapyGroupDeletionLog> TherapyGroupDeletionLogs { get; set; }

    // ── NOWE: własna tabela użytkowników z ręcznym hashowaniem ─────────────
    public DbSet<CustomUser> CustomUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PatientGroup>().HasKey(pg => new { pg.AppUserId, pg.TherapyGroupId });
        builder.Entity<PatientInventory>().HasKey(pi => new { pi.AppUserId, pi.RewardItemId });
        builder
            .Entity<AppUser>()
            .HasOne(u => u.Wallet)
            .WithOne(w => w.AppUser)
            .HasForeignKey<Wallet>(w => w.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .Entity<DailyReport>()
            .HasOne(d => d.AppUser)
            .WithMany(u => u.DailyReports)
            .HasForeignKey(d => d.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Entity<ForumPost>()
            .HasOne(fp => fp.AppUser)
            .WithMany()
            .HasForeignKey(fp => fp.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Entity<Consultation>()
            .HasOne(c => c.Patient)
            .WithMany()
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Entity<Consultation>()
            .HasOne(c => c.Therapist)
            .WithMany()
            .HasForeignKey(c => c.TherapistId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Entity<SurveyAnswer>()
            .HasOne(a => a.Submission)
            .WithMany(s => s.Answers)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .Entity<SurveyAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Entity<Survey>()
            .HasMany(s => s.Submissions)
            .WithOne(su => su.Survey)
            .HasForeignKey(su => su.SurveyId)
            .OnDelete(DeleteBehavior.Restrict);

        // CustomUser – unikalny email
        builder.Entity<CustomUser>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}