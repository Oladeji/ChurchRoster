-- Create invitations table migration
-- Run this in your Supabase SQL Editor or pgAdmin

-- Step 1: Make password_hash nullable (for invitation workflow)
ALTER TABLE users ALTER COLUMN password_hash DROP NOT NULL;

-- Step 2: Create invitations table
CREATE TABLE IF NOT EXISTS invitations (
    invitation_id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    phone VARCHAR(50) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Member',
    token VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT FALSE,
    used_at TIMESTAMP WITH TIME ZONE,
    user_id INTEGER,
    created_by INTEGER NOT NULL,
    CONSTRAINT FK_invitations_users_created_by FOREIGN KEY (created_by) REFERENCES users (user_id) ON DELETE RESTRICT,
    CONSTRAINT FK_invitations_users_user_id FOREIGN KEY (user_id) REFERENCES users (user_id) ON DELETE RESTRICT
);

-- Step 3: Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_invitations_email ON invitations (email);
CREATE INDEX IF NOT EXISTS idx_invitations_is_used ON invitations (is_used);
CREATE UNIQUE INDEX IF NOT EXISTS idx_invitations_token ON invitations (token);
CREATE INDEX IF NOT EXISTS IX_invitations_created_by ON invitations (created_by);
CREATE INDEX IF NOT EXISTS IX_invitations_user_id ON invitations (user_id);

-- Step 4: Record migration in history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260401200850_AddInvitationSystem', '10.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Verification queries (optional - run these to check):
-- SELECT * FROM invitations LIMIT 10;
-- SELECT column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = 'invitations';
