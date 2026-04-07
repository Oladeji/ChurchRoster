using ChurchRoster.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<MinistryTask> Tasks { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Invitation> Invitations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.TenantId);
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.SubscriptionEndDate).HasColumnName("subscription_end_date");

            entity.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("idx_tenants_slug");
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255); // Nullable for invitations
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).HasDefaultValue("Member");
            entity.Property(e => e.MonthlyLimit).HasColumnName("monthly_limit");
            entity.Property(e => e.DeviceToken).HasColumnName("device_token");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique().HasDatabaseName("idx_users_tenant_email");
            entity.HasIndex(e => e.Role).HasDatabaseName("idx_users_role");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);

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
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.SkillName).HasColumnName("skill_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.TenantId, e.SkillName }).IsUnique().HasDatabaseName("idx_skills_tenant_name");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Skills)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        });

        // Configure UserSkill entity (junction table)
        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.ToTable("user_skills");
            entity.HasKey(e => new { e.UserId, e.SkillId });
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
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

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        });

        // Configure Task entity
        modelBuilder.Entity<MinistryTask>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.TaskId);
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
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

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Tasks)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        });

        // Configure Assignment entity
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.ToTable("assignments");
            entity.HasKey(e => e.AssignmentId);
            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
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

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Assignments)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        });

        // Configure Invitation entity
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.ToTable("invitations");
            entity.HasKey(e => e.InvitationId);
            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).HasDefaultValue("Member");
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
            entity.Property(e => e.UsedAt).HasColumnName("used_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasIndex(e => e.Token).IsUnique().HasDatabaseName("idx_invitations_token");
            entity.HasIndex(e => new { e.TenantId, e.Email }).HasDatabaseName("idx_invitations_tenant_email");
            entity.HasIndex(e => e.IsUsed).HasDatabaseName("idx_invitations_is_used");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Invitations)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const int defaultTenantId = 1;

        modelBuilder.Entity<Tenant>().HasData(
            new Tenant
            {
                TenantId = defaultTenantId,
                Name = "Default Church",
                Slug = "default-church",
                IsActive = true,
                CreatedAt = seedDate,
                ContactEmail = "admin@church.com",
                SubscriptionEndDate = seedDate.AddYears(10)
            }
        );

        // Seed Skills
        modelBuilder.Entity<Skill>().HasData(
            new Skill { SkillId = 1, TenantId = defaultTenantId, SkillName = "CanLeadBibleStudy", Description = "Can lead Tuesday Bible Study sessions", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 2, TenantId = defaultTenantId, SkillName = "CanLeadPreaching", Description = "Can lead Sunday Preaching and deliver sermons", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 3, TenantId = defaultTenantId, SkillName = "CanLeadPrayer", Description = "Can lead prayer sessions", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 4, TenantId = defaultTenantId, SkillName = "CanMakeAnnouncements", Description = "Can make church announcements", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 5, TenantId = defaultTenantId, SkillName = "CanLeadWorship", Description = "Can lead worship and praise sessions", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 6, TenantId = defaultTenantId, SkillName = "CanOperateSound", Description = "Can operate sound and audio equipment", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Skill { SkillId = 7, TenantId = defaultTenantId, SkillName = "CanManageChildren", Description = "Can manage children's ministry", CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // Seed Tasks
        modelBuilder.Entity<MinistryTask>().HasData(
            new MinistryTask { TaskId = 1, TenantId = defaultTenantId, TaskName = "Lead Bible Study", Frequency = "Weekly", DayRule = "Tuesday", RequiredSkillId = 1, IsRestricted = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 2, TenantId = defaultTenantId, TaskName = "Lead Prayer Meeting", Frequency = "Weekly", DayRule = "Tuesday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 3, TenantId = defaultTenantId, TaskName = "Lead Preaching", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = 2, IsRestricted = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 4, TenantId = defaultTenantId, TaskName = "Lead Opening Prayer", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 5, TenantId = defaultTenantId, TaskName = "Lead Announcements", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 6, TenantId = defaultTenantId, TaskName = "Lead Closing Prayer", Frequency = "Weekly", DayRule = "Sunday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 7, TenantId = defaultTenantId, TaskName = "Lead All-Night Prayer", Frequency = "Monthly", DayRule = "Last Friday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new MinistryTask { TaskId = 8, TenantId = defaultTenantId, TaskName = "Lead Vigil Prayer", Frequency = "Monthly", DayRule = "Last Saturday", RequiredSkillId = null, IsRestricted = false, CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // Seed Admin User
        // Password: Admin@123 (hashed with BCrypt)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                TenantId = defaultTenantId,
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
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added && user.TenantId == default && _tenantContext.TenantId.HasValue)
                    user.TenantId = _tenantContext.TenantId.Value;
                user.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Skill skill)
            {
                if (entry.State == EntityState.Added && skill.TenantId == default && _tenantContext.TenantId.HasValue)
                    skill.TenantId = _tenantContext.TenantId.Value;
                skill.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is MinistryTask task)
            {
                if (entry.State == EntityState.Added && task.TenantId == default && _tenantContext.TenantId.HasValue)
                    task.TenantId = _tenantContext.TenantId.Value;
                task.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Assignment assignment)
            {
                if (entry.State == EntityState.Added && assignment.TenantId == default && _tenantContext.TenantId.HasValue)
                    assignment.TenantId = _tenantContext.TenantId.Value;
                assignment.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Invitation invitation)
            {
                if (entry.State == EntityState.Added && invitation.TenantId == default && _tenantContext.TenantId.HasValue)
                    invitation.TenantId = _tenantContext.TenantId.Value;
            }
            else if (entry.Entity is UserSkill userSkill)
            {
                if (entry.State == EntityState.Added && userSkill.TenantId == default && _tenantContext.TenantId.HasValue)
                    userSkill.TenantId = _tenantContext.TenantId.Value;
            }
        }
    }
}
