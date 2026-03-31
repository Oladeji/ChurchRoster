-- =============================================
-- Stored Procedures and Functions
-- Church Ministry Rostering System
-- =============================================

-- =============================================
-- Function: Check if member is qualified for a task
-- =============================================
CREATE OR REPLACE FUNCTION is_member_qualified(
    p_user_id INTEGER,
    p_task_id INTEGER
) RETURNS BOOLEAN AS $$
DECLARE
    v_required_skill_id INTEGER;
    v_is_restricted BOOLEAN;
    v_has_skill BOOLEAN;
BEGIN
    -- Get task requirements
    SELECT required_skill_id, is_restricted
    INTO v_required_skill_id, v_is_restricted
    FROM tasks
    WHERE task_id = p_task_id AND is_active = TRUE;

    -- If task doesn't exist, return false
    IF NOT FOUND THEN
        RETURN FALSE;
    END IF;

    -- If task is not restricted, anyone can do it
    IF NOT v_is_restricted THEN
        RETURN TRUE;
    END IF;

    -- Check if user has the required skill
    SELECT EXISTS(
        SELECT 1 
        FROM user_skills 
        WHERE user_id = p_user_id 
        AND skill_id = v_required_skill_id
    ) INTO v_has_skill;

    RETURN v_has_skill;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Check for scheduling conflicts
-- Returns TRUE if there's a conflict
-- =============================================
CREATE OR REPLACE FUNCTION has_scheduling_conflict(
    p_user_id INTEGER,
    p_event_date DATE,
    p_exclude_assignment_id INTEGER DEFAULT NULL
) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS(
        SELECT 1
        FROM assignments
        WHERE user_id = p_user_id
        AND event_date = p_event_date
        AND status NOT IN ('Rejected', 'Expired')
        AND (p_exclude_assignment_id IS NULL OR assignment_id != p_exclude_assignment_id)
    );
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Get member's assignment count for a month
-- =============================================
CREATE OR REPLACE FUNCTION get_monthly_assignment_count(
    p_user_id INTEGER,
    p_year INTEGER,
    p_month INTEGER
) RETURNS INTEGER AS $$
BEGIN
    RETURN (
        SELECT COUNT(*)
        FROM assignments
        WHERE user_id = p_user_id
        AND EXTRACT(YEAR FROM event_date) = p_year
        AND EXTRACT(MONTH FROM event_date) = p_month
        AND status NOT IN ('Rejected', 'Expired')
    );
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Check if member has reached monthly limit
-- =============================================
CREATE OR REPLACE FUNCTION has_reached_monthly_limit(
    p_user_id INTEGER,
    p_event_date DATE
) RETURNS BOOLEAN AS $$
DECLARE
    v_monthly_limit INTEGER;
    v_assignment_count INTEGER;
    v_year INTEGER;
    v_month INTEGER;
BEGIN
    -- Get user's monthly limit
    SELECT monthly_limit INTO v_monthly_limit
    FROM users
    WHERE user_id = p_user_id;

    -- If no limit set, return false
    IF v_monthly_limit IS NULL THEN
        RETURN FALSE;
    END IF;

    -- Extract year and month from event date
    v_year := EXTRACT(YEAR FROM p_event_date);
    v_month := EXTRACT(MONTH FROM p_event_date);

    -- Get current assignment count
    v_assignment_count := get_monthly_assignment_count(p_user_id, v_year, v_month);

    -- Check if at or over limit
    RETURN v_assignment_count >= v_monthly_limit;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Get available members for a task on a specific date
-- =============================================
CREATE OR REPLACE FUNCTION get_available_members(
    p_task_id INTEGER,
    p_event_date DATE
) RETURNS TABLE (
    user_id INTEGER,
    name VARCHAR,
    email VARCHAR,
    phone VARCHAR,
    assignment_count INTEGER,
    monthly_limit INTEGER,
    is_qualified BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.user_id,
        u.name,
        u.email,
        u.phone,
        get_monthly_assignment_count(
            u.user_id, 
            EXTRACT(YEAR FROM p_event_date)::INTEGER, 
            EXTRACT(MONTH FROM p_event_date)::INTEGER
        ) as assignment_count,
        u.monthly_limit,
        is_member_qualified(u.user_id, p_task_id) as is_qualified
    FROM users u
    WHERE u.role = 'Member'
    AND u.is_active = TRUE
    AND NOT has_scheduling_conflict(u.user_id, p_event_date)
    AND is_member_qualified(u.user_id, p_task_id)
    ORDER BY assignment_count ASC, u.name ASC;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Auto-expire past pending assignments
-- =============================================
CREATE OR REPLACE PROCEDURE expire_past_assignments()
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE assignments
    SET status = 'Expired',
        updated_at = NOW()
    WHERE event_date < CURRENT_DATE
    AND status = 'Pending';
END;
$$;

-- =============================================
-- Procedure: Auto-complete past accepted assignments
-- =============================================
CREATE OR REPLACE PROCEDURE complete_past_assignments()
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE assignments
    SET status = 'Completed',
        updated_at = NOW()
    WHERE event_date < CURRENT_DATE
    AND status IN ('Accepted', 'Confirmed');
END;
$$;

-- =============================================
-- Function: Validate assignment before creation
-- Returns error message if invalid, NULL if valid
-- =============================================
CREATE OR REPLACE FUNCTION validate_assignment(
    p_user_id INTEGER,
    p_task_id INTEGER,
    p_event_date DATE,
    p_is_override BOOLEAN DEFAULT FALSE
) RETURNS TEXT AS $$
DECLARE
    v_user_active BOOLEAN;
    v_task_active BOOLEAN;
BEGIN
    -- Check if user is active
    SELECT is_active INTO v_user_active
    FROM users WHERE user_id = p_user_id;

    IF NOT v_user_active THEN
        RETURN 'User is not active';
    END IF;

    -- Check if task is active
    SELECT is_active INTO v_task_active
    FROM tasks WHERE task_id = p_task_id;

    IF NOT v_task_active THEN
        RETURN 'Task is not active';
    END IF;

    -- Check for scheduling conflict
    IF has_scheduling_conflict(p_user_id, p_event_date) THEN
        RETURN 'Member already has an assignment on this date';
    END IF;

    -- Check qualification (unless override)
    IF NOT p_is_override AND NOT is_member_qualified(p_user_id, p_task_id) THEN
        RETURN 'Member is not qualified for this task';
    END IF;

    -- Check monthly limit (warning, not blocking)
    IF has_reached_monthly_limit(p_user_id, p_event_date) THEN
        RETURN 'WARNING: Member has reached monthly assignment limit';
    END IF;

    -- All validations passed
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Functions and Procedures Created Successfully
-- =============================================
