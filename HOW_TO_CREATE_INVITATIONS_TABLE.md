# 🔧 Fix: Create Invitations Table

## ❌ The Problem

```
42P01: relation "invitations" does not exist
```

The `invitations` table was never created in your database because the migration was never applied.

## ✅ Solution: Run the SQL Script Manually

Since the `dotnet ef database update` command is having issues with the Supabase connection, we'll run the SQL directly in Supabase.

### Option A: Use Supabase SQL Editor (Easiest)

1. **Open Supabase Dashboard**
   - Go to: https://supabase.com/dashboard
   - Select your project

2. **Open SQL Editor**
   - Click **SQL Editor** in the left sidebar
   - Click **+ New query**

3. **Copy and Paste the SQL**
   - Open the file: `backend/ChurchRoster.Api/create_invitations_table.sql`
   - Copy ALL the SQL
   - Paste it into the Supabase SQL Editor

4. **Run the SQL**
   - Click **Run** (or press Ctrl+Enter)
   - You should see: "Success. No rows returned"

5. **Verify**
   - Run this query to check:
   ```sql
   SELECT * FROM invitations LIMIT 1;
   ```
   - Should return empty result (no error)

### Option B: Use pgAdmin (If you have it installed)

1. Connect to your Supabase database:
   - Host: `aws-1-ca-central-1.pooler.supabase.com`
   - Port: `5432`
   - Database: `postgres`
   - Username: `postgres.edxjeuoutitcdfuqzyxp`
   - Password: `Deji1@Akoms!`
   - SSL Mode: Require

2. Open Query Tool

3. Copy and paste the SQL from `create_invitations_table.sql`

4. Execute (F5)

### Option C: Use psql Command Line

```bash
psql "postgresql://postgres.edxjeuoutitcdfuqzyxp:Deji1@Akoms!@aws-1-ca-central-1.pooler.supabase.com:5432/postgres?sslmode=require" -f backend/ChurchRoster.Api/create_invitations_table.sql
```

## 🧪 After Running the SQL

### Step 1: Restart Your Application
- Stop (Shift+F5)
- Start (F5)

### Step 2: Try Adding a Member with Invitation Email
1. Go to Members page
2. Click "+ Add Member"
3. Fill in details
4. **Check** "Send invitation email"
5. Click "Send Invitation"

### Expected Result:
✅ "Invitation email sent successfully!"
✅ Check your logs - should see:
```
📧 API ENDPOINT: POST /api/invitations/send
=== SendInvitationAsync started ===
✅ No existing user or pending invitation found
✅ Invitation saved to database with ID: 1
📧 Attempting to send invitation email
✅✅✅ EMAIL SENT SUCCESSFULLY ✅✅✅
```

### Step 3: Check the Email
- Check the recipient's inbox
- Check spam/junk folder
- Email should have:
  - Subject: "You're Invited to Join Church Ministry Roster"
  - Professional HTML design
  - "Accept Invitation" button
  - Link to: `http://localhost:3000/accept-invitation?token=...`

## 🎯 What the SQL Does

1. **Makes password nullable** - Allows users created via invitation to not have password initially
2. **Creates invitations table** - Stores all invitation data
3. **Creates indexes** - Optimizes query performance
4. **Records migration** - Tells EF Core the migration was applied

## 📊 Table Structure

```sql
invitations
├── invitation_id (PK, Auto-increment)
├── email (Unique per invitation)
├── name
├── phone
├── role (Admin/Member)
├── token (Unique, 32-byte secure random)
├── created_at (Timestamp with timezone)
├── expires_at (7 days from creation)
├── is_used (Boolean, default false)
├── used_at (Timestamp when accepted)
├── user_id (FK to users, set when accepted)
└── created_by (FK to users, who sent invitation)
```

## 🔍 Verify the Table Exists

Run this in Supabase SQL Editor:

```sql
-- Check table exists
SELECT tablename FROM pg_tables WHERE tablename = 'invitations';

-- Check columns
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'invitations'
ORDER BY ordinal_position;

-- Check indexes
SELECT indexname, indexdef 
FROM pg_indexes 
WHERE tablename = 'invitations';

-- Check migration history
SELECT * FROM "__EFMigrationsHistory" 
WHERE "MigrationId" = '20260401200850_AddInvitationSystem';
```

## ⚠️ If You Get Errors

### Error: "relation already exists"
```
Table or index "invitations" already exists
```
✅ **Good!** The table already exists. Just run the last INSERT statement to record the migration:
```sql
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260401200850_AddInvitationSystem', '10.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
```

### Error: "permission denied"
```
permission denied for table invitations
```
❌ You need admin access. Contact your Supabase project admin.

### Error: "database connection failed"
```
could not connect to server
```
❌ Check your internet connection and Supabase status.

## 🚀 Quick Summary

1. **Open Supabase SQL Editor**: https://supabase.com/dashboard
2. **Run**: `backend/ChurchRoster.Api/create_invitations_table.sql`
3. **Verify**: `SELECT * FROM invitations;` (should work, no error)
4. **Restart your app**
5. **Test**: Add member → Check "Send invitation email"
6. **Success**: Email sent, check inbox!

---

**After running the SQL, the invitation system will work perfectly!** 🎉
