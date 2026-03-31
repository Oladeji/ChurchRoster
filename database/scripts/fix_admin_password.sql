-- Update admin user password hash
-- Password: Admin@123
-- This hash was generated using BCrypt.Net.BCrypt.HashPassword("Admin@123")

UPDATE users 
SET password_hash = '$2a$11$X6qGvzOj1Xd/sOJs.rc0FOxxoda66NVWgoIHi.VkPTy2XRlzAlOJm',
    updated_at = NOW()
WHERE user_id = 1;

-- Verify the update
SELECT user_id, name, email, role, password_hash 
FROM users 
WHERE user_id = 1;
