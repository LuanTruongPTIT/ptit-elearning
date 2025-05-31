-- Tạo bảng assignments trong schema programs
CREATE TABLE IF NOT EXISTS programs.table_assignments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    teaching_assign_course_id UUID NOT NULL,
    title VARCHAR(500) NOT NULL,
    description TEXT,
    deadline TIMESTAMP WITH TIME ZONE NOT NULL,
    assignment_type VARCHAR(50) DEFAULT 'upload', -- 'upload', 'quiz', 'both'
    show_answers BOOLEAN DEFAULT FALSE,
    time_limit_minutes INT, -- for quiz type
    attachment_urls JSON, -- array of file URLs
    max_score DECIMAL(5,2) DEFAULT 100.00,
    is_published BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL,
    
    CONSTRAINT fk_assignment_teaching_assign_course
        FOREIGN KEY (teaching_assign_course_id) 
        REFERENCES programs.table_teaching_assign_courses(id)
        ON DELETE CASCADE,
    
    CONSTRAINT fk_assignment_created_by
        FOREIGN KEY (created_by) 
        REFERENCES users.table_users(id)
        ON DELETE CASCADE
);

-- Tạo bảng assignment submissions
CREATE TABLE IF NOT EXISTS programs.table_assignment_submissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    assignment_id UUID NOT NULL,
    student_id UUID NOT NULL,
    submission_type VARCHAR(50) DEFAULT 'file', -- 'file', 'quiz', 'both'
    file_urls JSON, -- array of submitted file URLs
    quiz_attempt_id UUID, -- link to quiz attempt if applicable
    submitted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    grade DECIMAL(5,2), -- điểm số
    feedback TEXT, -- phản hồi từ giảng viên
    status VARCHAR(50) DEFAULT 'submitted', -- 'submitted', 'graded', 'late', 'draft'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_submission_assignment
        FOREIGN KEY (assignment_id) 
        REFERENCES programs.table_assignments(id)
        ON DELETE CASCADE,
    
    CONSTRAINT fk_submission_student
        FOREIGN KEY (student_id) 
        REFERENCES users.table_users(id)
        ON DELETE CASCADE,
        
    CONSTRAINT fk_submission_quiz_attempt
        FOREIGN KEY (quiz_attempt_id) 
        REFERENCES programs.table_quiz_attempts(attempt_id)
        ON DELETE SET NULL,
    
    UNIQUE (assignment_id, student_id)
);

-- Tạo bảng recent activities
CREATE TABLE IF NOT EXISTS programs.table_recent_activities (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    action VARCHAR(100) NOT NULL, -- 'assignment_created', 'assignment_submitted', 'quiz_completed', etc.
    target_type VARCHAR(50) NOT NULL, -- 'assignment', 'quiz', 'lecture', 'course'
    target_id UUID NOT NULL,
    target_title VARCHAR(500),
    course_id UUID, -- for context
    course_name VARCHAR(255),
    metadata JSON, -- additional data (score, deadline, etc.)
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_activity_user
        FOREIGN KEY (user_id) 
        REFERENCES users.table_users(id)
        ON DELETE CASCADE
);

-- Tạo bảng notifications
CREATE TABLE IF NOT EXISTS programs.table_notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    type VARCHAR(50) NOT NULL, -- 'assignment', 'quiz', 'announcement', 'grade'
    target_type VARCHAR(50), -- 'assignment', 'quiz', 'lecture', 'course'
    target_id UUID,
    is_read BOOLEAN DEFAULT FALSE,
    priority VARCHAR(20) DEFAULT 'normal', -- 'low', 'normal', 'high', 'urgent'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    read_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT fk_notification_user
        FOREIGN KEY (user_id) 
        REFERENCES users.table_users(id)
        ON DELETE CASCADE
);

-- Tạo indexes để tối ưu performance
CREATE INDEX IF NOT EXISTS idx_assignments_teaching_assign_course ON programs.table_assignments(teaching_assign_course_id);
CREATE INDEX IF NOT EXISTS idx_assignments_deadline ON programs.table_assignments(deadline);
CREATE INDEX IF NOT EXISTS idx_assignments_created_by ON programs.table_assignments(created_by);

CREATE INDEX IF NOT EXISTS idx_submissions_assignment ON programs.table_assignment_submissions(assignment_id);
CREATE INDEX IF NOT EXISTS idx_submissions_student ON programs.table_assignment_submissions(student_id);
CREATE INDEX IF NOT EXISTS idx_submissions_status ON programs.table_assignment_submissions(status);

CREATE INDEX IF NOT EXISTS idx_activities_user ON programs.table_recent_activities(user_id);
CREATE INDEX IF NOT EXISTS idx_activities_created_at ON programs.table_recent_activities(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_activities_target ON programs.table_recent_activities(target_type, target_id);

CREATE INDEX IF NOT EXISTS idx_notifications_user ON programs.table_notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_unread ON programs.table_notifications(user_id, is_read);
CREATE INDEX IF NOT EXISTS idx_notifications_created_at ON programs.table_notifications(created_at DESC);
