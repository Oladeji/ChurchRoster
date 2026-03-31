-- =============================================
-- Church Ministry Rostering System
-- Database Clean-Up Script
-- Version: 1.0
-- WARNING: This will delete ALL data!
-- =============================================

-- Drop all tables in reverse order of dependencies
DROP TABLE IF EXISTS audit_logs CASCADE;
DROP TABLE IF EXISTS assignments CASCADE;
DROP TABLE IF EXISTS tasks CASCADE;
DROP TABLE IF EXISTS user_skills CASCADE;
DROP TABLE IF EXISTS skills CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Drop triggers
DROP TRIGGER IF EXISTS update_users_updated_at ON users;
DROP TRIGGER IF EXISTS update_skills_updated_at ON skills;
DROP TRIGGER IF EXISTS update_tasks_updated_at ON tasks;
DROP TRIGGER IF EXISTS update_assignments_updated_at ON assignments;

-- Drop functions
DROP FUNCTION IF EXISTS update_updated_at_column();

-- Verify all tables are dropped
SELECT tablename FROM pg_tables WHERE schemaname = 'public';

-- =============================================
-- Clean-Up Complete
-- =============================================
