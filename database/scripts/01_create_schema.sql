-- =============================================
-- Church Ministry Rostering System
-- Database Schema Creation Script
-- Version: 1.0
-- Database: PostgreSQL
-- =============================================

-- =============================================
-- Users Table
-- =============================================
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(50),
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'Member' CHECK (role IN ('Admin', 'Member')),
    monthly_limit INTEGER,
    device_token TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Create index on email for faster lookups
CREATE INDEX idx_users_email ON users(email);

-- Create index on role for filtering
CREATE INDEX idx_users_role ON users(role);

-- =============================================
-- Skills Table
-- =============================================
CREATE TABLE skills (
    skill_id SERIAL PRIMARY KEY,
    skill_name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Create index on skill_name for faster lookups
CREATE INDEX idx_skills_name ON skills(skill_name);

-- =============================================
-- User Skills (Many-to-Many Junction Table)
-- =============================================
CREATE TABLE user_skills (
    user_id INTEGER NOT NULL,
    skill_id INTEGER NOT NULL,
    assigned_date TIMESTAMP DEFAULT NOW(),
    PRIMARY KEY (user_id, skill_id),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (skill_id) REFERENCES skills(skill_id) ON DELETE CASCADE
);

-- Create indexes for faster joins
CREATE INDEX idx_user_skills_user ON user_skills(user_id);
CREATE INDEX idx_user_skills_skill ON user_skills(skill_id);

-- =============================================
-- Tasks Table
-- =============================================
CREATE TABLE tasks (
    task_id SERIAL PRIMARY KEY,
    task_name VARCHAR(255) NOT NULL,
    frequency VARCHAR(50) CHECK (frequency IN ('Weekly', 'Monthly')),
    day_rule VARCHAR(50) NOT NULL,
    required_skill_id INTEGER,
    is_restricted BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (required_skill_id) REFERENCES skills(skill_id) ON DELETE SET NULL
);

-- Create index on frequency for filtering
CREATE INDEX idx_tasks_frequency ON tasks(frequency);

-- Create index on required_skill_id for joins
CREATE INDEX idx_tasks_skill ON tasks(required_skill_id);

-- =============================================
-- Assignments Table
-- =============================================
CREATE TABLE assignments (
    assignment_id SERIAL PRIMARY KEY,
    task_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    event_date DATE NOT NULL,
    status VARCHAR(50) DEFAULT 'Pending' CHECK (status IN ('Pending', 'Accepted', 'Rejected', 'Confirmed', 'Completed', 'Expired')),
    rejection_reason TEXT,
    is_override BOOLEAN DEFAULT FALSE,
    assigned_by INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (task_id) REFERENCES tasks(task_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (assigned_by) REFERENCES users(user_id) ON DELETE RESTRICT
);

-- Create composite index for user and event date (for conflict detection)
CREATE INDEX idx_assignments_user_date ON assignments(user_id, event_date);

-- Create index on task_id for filtering
CREATE INDEX idx_assignments_task ON assignments(task_id);

-- Create index on status for filtering
CREATE INDEX idx_assignments_status ON assignments(status);

-- Create index on event_date for date range queries
CREATE INDEX idx_assignments_event_date ON assignments(event_date);

-- =============================================
-- Audit Log Table (Optional - for tracking changes)
-- =============================================
CREATE TABLE audit_logs (
    log_id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    record_id INTEGER NOT NULL,
    action VARCHAR(50) NOT NULL CHECK (action IN ('INSERT', 'UPDATE', 'DELETE')),
    user_id INTEGER,
    old_values JSONB,
    new_values JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE SET NULL
);

-- Create index on table_name and record_id for faster lookups
CREATE INDEX idx_audit_table_record ON audit_logs(table_name, record_id);

-- Create index on created_at for time-based queries
CREATE INDEX idx_audit_created ON audit_logs(created_at);

-- =============================================
-- Triggers for Updated_At Timestamps
-- =============================================

-- Function to update updated_at column
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply trigger to users table
CREATE TRIGGER update_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Apply trigger to skills table
CREATE TRIGGER update_skills_updated_at
BEFORE UPDATE ON skills
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Apply trigger to tasks table
CREATE TRIGGER update_tasks_updated_at
BEFORE UPDATE ON tasks
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Apply trigger to assignments table
CREATE TRIGGER update_assignments_updated_at
BEFORE UPDATE ON assignments
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- Comments for Documentation
-- =============================================

COMMENT ON TABLE users IS 'Stores all church members and administrators';
COMMENT ON TABLE skills IS 'Stores ministry skills that members can possess';
COMMENT ON TABLE user_skills IS 'Junction table linking users to their skills';
COMMENT ON TABLE tasks IS 'Stores ministry tasks that need to be assigned';
COMMENT ON TABLE assignments IS 'Stores task assignments to users for specific dates';
COMMENT ON TABLE audit_logs IS 'Tracks all changes made to critical tables';

COMMENT ON COLUMN users.monthly_limit IS 'Maximum number of tasks a member can be assigned per month';
COMMENT ON COLUMN users.device_token IS 'Firebase Cloud Messaging token for push notifications';
COMMENT ON COLUMN tasks.day_rule IS 'Day specification (e.g., Tuesday, Sunday, Last Friday)';
COMMENT ON COLUMN tasks.is_restricted IS 'Whether task requires specific skill qualification';
COMMENT ON COLUMN assignments.is_override IS 'Whether admin overrode qualification requirements';

-- =============================================
-- Database Setup Complete
-- =============================================
