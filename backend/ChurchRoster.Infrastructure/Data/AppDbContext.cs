using ChurchRoster.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<MinistryTask> Tasks { get; set; }
    public DbSet<Assignment> Assignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).HasDefaultValue("Member");
            entity.Property(e => e.MonthlyLimit).HasColumnName("monthly_limit");
            entity.Property(e => e.DeviceToken).HasColumnName("device_token");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("idx_users_email");
            entity.HasIndex(e => e.Role).HasDatabaseName("idx_users_role");

            // Configure relationships
            entity.HasMany(e => e.Assignments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.AssignmentsCreated)
                .WithOne(a => a.AssignedByUser)
                .HasForeignKey(a => a.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Skill entity
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("skills");
            entity.HasKey(e => e.SkillId);
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.SkillName).HasColumnName("skill_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.SkillName).IsUnique().HasDatabaseName("idx_skills_name");
        });

        // Configure UserSkill entity (junction table)
        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.ToTable("user_skills");
            entity.HasKey(e => new { e.UserId, e.SkillId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.AssignedDate).HasColumnName("assigned_date").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_user_skills_user");
            entity.HasIndex(e => e.SkillId).HasDatabaseName("idx_user_skills_skill");
        });

        // Configure Task entity
        modelBuilder.Entity<MinistryTask>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.TaskId);
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.TaskName).HasColumnName("task_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasMaxLength(50);
            entity.Property(e => e.DayRule).HasColumnName("day_rule").HasMaxLength(50).IsRequired();
            entity.Property(e => e.RequiredSkillId).HasColumnName("required_skill_id");
            entity.Property(e => e.IsRestricted).HasColumnName("is_restricted").HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.RequiredSkill)
                .WithMany(s => s.Tasks)
                .HasForeignKey(e => e.RequiredSkillId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Frequency).HasDatabaseName("idx_tasks_frequency");
            entity.HasIndex(e => e.RequiredSkillId).HasDatabaseName("idx_tasks_skill");
        });

        // Configure Assignment entity
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.ToTable("assignments");
            entity.HasKey(e => e.AssignmentId);
            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EventDate).HasColumnName("event_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.IsOverride).HasColumnName("is_override").HasDefaultValue(false);
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Task)
                .WithMany(t => t.Assignments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.EventDate }).HasDatabaseName("idx_assignments_user_date");
            entity.HasIndex(e => e.TaskId).HasDatabaseName("idx_assignments_task");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_assignments_status");
            entity.HasIndex(e => e.EventDate).HasDatabaseName("idx_assignments_event_date");
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed Skills
        modelBuilder.Entity<Skill>().HasData(
            new Skill { SkillId = 1, SkillName = "CanLeadBibleStudy", Description = "Can lead Tuesday Bible Study sessions", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 2, SkillName = "CanLeadPreaching", Description = "Can lead Sunday Preaching and deliver sermons", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 3, SkillName = "CanLeadPrayer", Description = "Can lead prayer sessions", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 4, SkillName = "CanMakeAnnouncements", Description = "Can make church announcements", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 5, SkillName = "CanLeadWorship", Description = "Can lead worship and praise sessions", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 6, SkillName = "CanOperateSound", Description = "Can operate sound and audio equipment", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 7, SkillName = "CanManageChildren", Description = "Can manage children's ministry", CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // Seed Tasks
        modelBuilder.Entity<MinistryTask>().HasData(
            new MinistryTask { TaskId = 1, TaskName = "Lead Bible Study", Frequency = "Weekly", DayRule = "Tuesday", RequiredSkillId = 1, IsRestricted = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 2, TaskName = "Lead Prayer Meeting", Frequency = "Weekly", DayRule = "Tuesday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 3, TaskName = "Lead Preaching", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = 2, IsRestricted = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 4, TaskName = "Lead Opening Prayer", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 5, TaskName = "Lead Announcements", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 6, TaskName = "Lead Closing Prayer", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 7, TaskName = "Lead All-Night Prayer", Frequency = "Monthly", DayRule = "Last Friday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 8, TaskName = "Lead Vigil Prayer", Frequency = "Monthly", DayRule = "Last Saturday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // Seed Admin User
        // Password: Admin@123 (hashed with BCrypt)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                Name = "System Administrator",
                Email = "admin@church.com",
                Phone = "+1234567890",
                PasswordHash = "$2a$11$X6qGvzOj1Xd/sOJs.rc0FOxxoda66NVWgoIHi.VkPTy2XRlzAlOJm",
                Role = "Admin",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
                user.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Skill skill)
                skill.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is MinistryTask task)
                task.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Assignment assignment)
                assignment.UpdatedAt = DateTime.UtcNow;
        }
    }
}
