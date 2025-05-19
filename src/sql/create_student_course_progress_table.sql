-- Create student_course_progress table in the programs schema
CREATE TABLE
IF NOT EXISTS programs.table_student_course_progress
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid
(),
    student_id UUID NOT NULL,
    teaching_assign_course_id UUID NOT NULL,
    total_lectures INT NOT NULL DEFAULT 0,
    completed_lectures INT NOT NULL DEFAULT 0,
    progress_percentage INT NOT NULL DEFAULT 0,
    last_accessed TIMESTAMP
WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR
(20) DEFAULT 'in_progress', -- in_progress, completed, not_started
    created_at TIMESTAMP
WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

    -- Add unique constraint for student_id and teaching_assign_course_id
    CONSTRAINT uk_student_course_progress UNIQUE
(student_id, teaching_assign_course_id),

    CONSTRAINT fk_student_course_progress_student
        FOREIGN KEY
(student_id)
        REFERENCES users.table_users
(id)
        ON
DELETE CASCADE,

    CONSTRAINT fk_student_course_progress_teaching_assign_course
        FOREIGN KEY
(teaching_assign_course_id)
        REFERENCES programs.table_teaching_assign_courses
(id)
        ON
DELETE CASCADE
);

-- Create index for faster queries
CREATE INDEX
IF NOT EXISTS idx_student_course_progress_student_id
    ON programs.table_student_course_progress
(student_id);
CREATE INDEX
IF NOT EXISTS idx_student_course_progress_teaching_assign_course_id
    ON programs.table_student_course_progress
(teaching_assign_course_id);
CREATE INDEX
IF NOT EXISTS idx_student_course_progress_combined
    ON programs.table_student_course_progress
(student_id, teaching_assign_course_id);

-- Create a trigger to update the updated_at timestamp
CREATE OR REPLACE FUNCTION programs.update_student_course_progress_updated_at
()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_student_course_progress_updated_at
BEFORE
UPDATE ON programs.table_student_course_progress
FOR EACH ROW
EXECUTE FUNCTION programs
.update_student_course_progress_updated_at
();
