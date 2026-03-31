-- =============================================
-- Useful Database Views
-- Church Ministry Rostering System
-- =============================================

-- =============================================
-- View: Member Skills Summary
-- Shows each member with their assigned skills
-- =============================================
CREATE OR REPLACE VIEW vw_member_skills AS
SELECT 
    u.user_id,
    u.name,
    u.email,
    u.role,
    u.monthly_limit,
    u.is_active,
    ARRAY_AGG(s.skill_name ORDER BY s.skill_name) as skills
FROM users u
LEFT JOIN user_skills us ON u.user_id = us.user_id
LEFT JOIN skills s ON us.skill_id = s.skill_id
GROUP BY u.user_id, u.name, u.email, u.role, u.monthly_limit, u.is_active;

-- =============================================
-- View: Task Requirements
-- Shows tasks with their skill requirements
-- =============================================
CREATE OR REPLACE VIEW vw_task_requirements AS
SELECT 
    t.task_id,
    t.task_name,
    t.frequency,
    t.day_rule,
    t.is_restricted,
    s.skill_name as required_skill,
    s.description as skill_description
FROM tasks t
LEFT JOIN skills s ON t.required_skill_id = s.skill_id
WHERE t.is_active = TRUE;

-- =============================================
-- View: Assignment Details
-- Shows full assignment information with user and task details
-- =============================================
CREATE OR REPLACE VIEW vw_assignment_details AS
SELECT 
    a.assignment_id,
    a.event_date,
    a.status,
    a.is_override,
    a.created_at,
    t.task_name,
    t.frequency,
    t.day_rule,
    u.name as assigned_to,
    u.email as assigned_to_email,
    u.phone as assigned_to_phone,
    admin.name as assigned_by_name,
    s.skill_name as required_skill
FROM assignments a
JOIN tasks t ON a.task_id = t.task_id
JOIN users u ON a.user_id = u.user_id
JOIN users admin ON a.assigned_by = admin.user_id
LEFT JOIN skills s ON t.required_skill_id = s.skill_id;

-- =============================================
-- View: Upcoming Assignments
-- Shows assignments for future dates
-- =============================================
CREATE OR REPLACE VIEW vw_upcoming_assignments AS
SELECT *
FROM vw_assignment_details
WHERE event_date >= CURRENT_DATE
ORDER BY event_date, task_name;

-- =============================================
-- View: Member Assignment Count (Current Month)
-- Shows how many assignments each member has this month
-- =============================================
CREATE OR REPLACE VIEW vw_monthly_assignment_count AS
SELECT 
    u.user_id,
    u.name,
    u.email,
    u.monthly_limit,
    COUNT(a.assignment_id) as assignments_this_month,
    CASE 
        WHEN u.monthly_limit IS NULL THEN 'No Limit'
        WHEN COUNT(a.assignment_id) >= u.monthly_limit THEN 'At Limit'
        WHEN COUNT(a.assignment_id) >= (u.monthly_limit * 0.8) THEN 'Near Limit'
        ELSE 'Available'
    END as status
FROM users u
LEFT JOIN assignments a ON u.user_id = a.user_id 
    AND a.event_date >= DATE_TRUNC('month', CURRENT_DATE)
    AND a.event_date < DATE_TRUNC('month', CURRENT_DATE) + INTERVAL '1 month'
    AND a.status NOT IN ('Rejected', 'Expired')
WHERE u.role = 'Member' AND u.is_active = TRUE
GROUP BY u.user_id, u.name, u.email, u.monthly_limit;

-- =============================================
-- View: Qualified Members Per Task
-- Shows which members are qualified for each task
-- =============================================
CREATE OR REPLACE VIEW vw_qualified_members AS
SELECT 
    t.task_id,
    t.task_name,
    t.is_restricted,
    s.skill_name as required_skill,
    u.user_id,
    u.name as member_name,
    u.email as member_email,
    u.is_active
FROM tasks t
LEFT JOIN skills s ON t.required_skill_id = s.skill_id
LEFT JOIN user_skills us ON s.skill_id = us.skill_id
LEFT JOIN users u ON us.user_id = u.user_id
WHERE t.is_active = TRUE
    AND (t.is_restricted = FALSE OR u.user_id IS NOT NULL)
    AND (u.is_active = TRUE OR u.user_id IS NULL);

-- =============================================
-- View: Assignment Statistics
-- Provides summary statistics for assignments
-- =============================================
CREATE OR REPLACE VIEW vw_assignment_statistics AS
SELECT 
    COUNT(*) as total_assignments,
    COUNT(CASE WHEN status = 'Pending' THEN 1 END) as pending,
    COUNT(CASE WHEN status = 'Accepted' THEN 1 END) as accepted,
    COUNT(CASE WHEN status = 'Rejected' THEN 1 END) as rejected,
    COUNT(CASE WHEN status = 'Confirmed' THEN 1 END) as confirmed,
    COUNT(CASE WHEN status = 'Completed' THEN 1 END) as completed,
    COUNT(CASE WHEN status = 'Expired' THEN 1 END) as expired,
    COUNT(CASE WHEN event_date >= CURRENT_DATE THEN 1 END) as upcoming,
    COUNT(CASE WHEN event_date < CURRENT_DATE THEN 1 END) as past
FROM assignments;

-- =============================================
-- Views Created Successfully
-- =============================================
