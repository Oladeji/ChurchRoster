# Church Roster System - Database

This folder contains all database-related files for the Church Ministry Rostering System using PostgreSQL.

## 📁 Folder Structure

```
database/
├── scripts/                       # SQL scripts for schema, views, and functions
│   ├── 01_create_schema.sql      # Main database schema
│   ├── 02_create_views.sql       # Database views
│   ├── 03_create_functions.sql   # Stored procedures and functions
│   └── 99_cleanup.sql            # Clean-up script (WARNING: Deletes all data)
│
├── seed-data/                     # Initial data population scripts
│   └── 01_initial_seed.sql       # Default skills, tasks, and admin user
│
├── migrations/                    # Database migration scripts (for updates)
│   └── (Empty - migrations will be added as needed)
│
└── README.md                      # This file
```

## 🚀 Quick Start

### Option 1: Using Supabase (Recommended for Free Hosting)

1. **Create Supabase Account**
   - Go to [supabase.com](https://supabase.com)
   - Sign up for a free account
   - Create a new project

2. **Run Scripts in Order**
   - Open Supabase Dashboard → SQL Editor
   - Copy and paste scripts in this order:
     1. `scripts/01_create_schema.sql`
     2. `scripts/02_create_views.sql`
     3. `scripts/03_create_functions.sql`
     4. `seed-data/01_initial_seed.sql`

3. **Get Connection String**
   - Go to Settings → Database
   - Copy the connection string
   - Format: `postgresql://[user]:[password]@[host]:[port]/[database]`

### Option 2: Using Local PostgreSQL

1. **Install PostgreSQL**
   ```bash
   # Windows: Download from postgresql.org
   # macOS: brew install postgresql
   # Linux: sudo apt-get install postgresql
   ```

2. **Create Database**
   ```bash
   psql -U postgres
   CREATE DATABASE church_roster;
   \c church_roster
   ```

3. **Run Scripts**
   ```bash
   psql -U postgres -d church_roster -f scripts/01_create_schema.sql
   psql -U postgres -d church_roster -f scripts/02_create_views.sql
   psql -U postgres -d church_roster -f scripts/03_create_functions.sql
   psql -U postgres -d church_roster -f seed-data/01_initial_seed.sql
   ```

## 📊 Database Schema

### Tables

| Table | Description | Key Columns |
|-------|-------------|-------------|
| **users** | Church members and admins | user_id, name, email, role, monthly_limit |
| **skills** | Ministry skills | skill_id, skill_name, description |
| **user_skills** | User-skill assignments | user_id, skill_id |
| **tasks** | Ministry tasks | task_id, task_name, frequency, required_skill_id |
| **assignments** | Task assignments to users | assignment_id, task_id, user_id, event_date, status |
| **audit_logs** | Change tracking | log_id, table_name, record_id, action |

### Views

| View | Purpose |
|------|---------|
| **vw_member_skills** | Members with their skills |
| **vw_task_requirements** | Tasks with skill requirements |
| **vw_assignment_details** | Full assignment information |
| **vw_upcoming_assignments** | Future assignments |
| **vw_monthly_assignment_count** | Member assignment counts |
| **vw_qualified_members** | Qualified members per task |
| **vw_assignment_statistics** | Assignment summary statistics |

### Functions

| Function | Purpose |
|----------|---------|
| **is_member_qualified** | Check if member can do a task |
| **has_scheduling_conflict** | Detect double-booking |
| **get_monthly_assignment_count** | Count assignments in a month |
| **has_reached_monthly_limit** | Check if member at limit |
| **get_available_members** | Find available members for a task |
| **validate_assignment** | Validate before creating assignment |

### Procedures

| Procedure | Purpose |
|-----------|---------|
| **expire_past_assignments** | Auto-expire old pending assignments |
| **complete_past_assignments** | Auto-complete past assignments |

## 🔑 Default Data

After running the seed script, you'll have:

### Skills
- CanLeadBibleStudy
- CanLeadPreaching
- CanLeadPrayer
- CanMakeAnnouncements
- CanLeadWorship
- CanOperateSound
- CanManageChildren

### Tasks

**Weekly - Tuesday:**
- Lead Bible Study (Restricted - requires CanLeadBibleStudy)
- Lead Prayer Meeting

**Weekly - Sunday:**
- Lead Preaching (Restricted - requires CanLeadPreaching)
- Lead Opening Prayer
- Lead Announcements
- Lead Closing Prayer

**Monthly:**
- Lead All-Night Prayer (Last Friday)
- Lead Vigil Prayer (Last Saturday)

### Admin User
- **Email:** admin@church.com
- **Password:** Admin@123 (Change immediately!)
- **Role:** Admin

## 🔧 Useful Queries

### Get all qualified members for a task
```sql
SELECT * FROM get_available_members(1, '2026-04-15');
```

### Check if member is qualified
```sql
SELECT is_member_qualified(user_id, task_id) FROM users;
```

### View upcoming assignments
```sql
SELECT * FROM vw_upcoming_assignments;
```

### Check monthly assignment counts
```sql
SELECT * FROM vw_monthly_assignment_count;
```

### Expire old assignments
```sql
CALL expire_past_assignments();
```

### Complete past assignments
```sql
CALL complete_past_assignments();
```

## 🗄️ Backup and Restore

### Backup Database
```bash
pg_dump -U postgres church_roster > backup.sql
```

### Restore Database
```bash
psql -U postgres church_roster < backup.sql
```

### Backup to Supabase
Supabase automatically backs up your database daily. To manually export:
- Dashboard → Settings → Database → Download Database Backup

## 🔄 Migrations

When making schema changes:

1. Create a new migration file in `migrations/` folder
2. Name it with timestamp: `YYYYMMDD_description.sql`
3. Include both UP and DOWN scripts
4. Test locally before applying to production

Example:
```sql
-- migrations/20260331_add_user_preferences.sql
-- UP
ALTER TABLE users ADD COLUMN notification_enabled BOOLEAN DEFAULT TRUE;

-- DOWN (for rollback)
ALTER TABLE users DROP COLUMN notification_enabled;
```

## 📈 Monitoring

### Check database size
```sql
SELECT pg_size_pretty(pg_database_size('church_roster'));
```

### Check table sizes
```sql
SELECT 
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

### Check active connections
```sql
SELECT count(*) FROM pg_stat_activity WHERE datname = 'church_roster';
```

## 🔐 Security Best Practices

1. **Change default admin password** immediately after setup
2. **Use environment variables** for connection strings (never commit to Git)
3. **Enable SSL/TLS** for database connections
4. **Limit database access** to backend API only
5. **Regular backups** - Set up automated daily backups
6. **Audit logs** - Review audit_logs table regularly

## 🧪 Testing Queries

### Insert test assignment
```sql
-- First, get IDs
SELECT user_id FROM users WHERE role = 'Member' LIMIT 1;
SELECT task_id FROM tasks WHERE task_name = 'Lead Prayer Meeting';

-- Validate assignment
SELECT validate_assignment(user_id, task_id, '2026-04-15', FALSE);

-- Create assignment if valid
INSERT INTO assignments (task_id, user_id, event_date, assigned_by, status)
VALUES (task_id, user_id, '2026-04-15', 
        (SELECT user_id FROM users WHERE role = 'Admin' LIMIT 1),
        'Pending');
```

## 📞 Support

For database issues:
- Check PostgreSQL logs
- Review Supabase logs (Dashboard → Logs)
- Verify connection string
- Check firewall settings

## 🚨 Clean-Up (WARNING)

To completely reset the database:
```bash
psql -U postgres -d church_roster -f scripts/99_cleanup.sql
```

**⚠️ This will DELETE ALL DATA! Only use in development.**

## 📄 License

This database schema is for church ministry use.

---

*Last Updated: March 31, 2026*
