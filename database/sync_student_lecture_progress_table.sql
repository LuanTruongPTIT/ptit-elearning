-- Script to synchronize student lecture progress table naming
-- This script ensures both naming conventions work

-- Check if enrollment.table_student_lecture_progresses exists
DO $$
BEGIN
    -- If enrollment table exists but programs table doesn't, create a view
    IF EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'enrollment' 
        AND table_name = 'table_student_lecture_progresses'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_student_lecture_progress'
    ) THEN
        -- Create a view in programs schema pointing to enrollment table
        EXECUTE 'CREATE OR REPLACE VIEW programs.table_student_lecture_progress AS 
                 SELECT * FROM enrollment.table_student_lecture_progresses';
        RAISE NOTICE 'Created view programs.table_student_lecture_progress pointing to enrollment.table_student_lecture_progresses';
    END IF;

    -- If programs table exists but enrollment table doesn't, create a view
    IF EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_student_lecture_progress'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'enrollment' 
        AND table_name = 'table_student_lecture_progresses'
    ) THEN
        -- Create a view in enrollment schema pointing to programs table
        EXECUTE 'CREATE OR REPLACE VIEW enrollment.table_student_lecture_progresses AS 
                 SELECT * FROM programs.table_student_lecture_progress';
        RAISE NOTICE 'Created view enrollment.table_student_lecture_progresses pointing to programs.table_student_lecture_progress';
    END IF;

    -- If neither exists, create the table in programs schema (as requested)
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'enrollment' 
        AND table_name = 'table_student_lecture_progresses'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_student_lecture_progress'
    ) THEN
        -- Create the table in programs schema
        EXECUTE 'CREATE TABLE programs.table_student_lecture_progress (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            student_id UUID NOT NULL,
            lecture_id UUID NOT NULL,
            watch_position INT NOT NULL DEFAULT 0,
            progress_percentage INT NOT NULL DEFAULT 0,
            is_completed BOOLEAN NOT NULL DEFAULT FALSE,
            last_accessed TIMESTAMP WITH TIME ZONE,
            created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
            CONSTRAINT uk_student_lecture_progress UNIQUE (student_id, lecture_id)
        )';
        
        -- Create indexes
        EXECUTE 'CREATE INDEX IF NOT EXISTS idx_student_lecture_progress_student_id 
                 ON programs.table_student_lecture_progress(student_id)';
        EXECUTE 'CREATE INDEX IF NOT EXISTS idx_student_lecture_progress_lecture_id 
                 ON programs.table_student_lecture_progress(lecture_id)';
        
        -- Create view in enrollment schema for compatibility
        EXECUTE 'CREATE OR REPLACE VIEW enrollment.table_student_lecture_progresses AS 
                 SELECT * FROM programs.table_student_lecture_progress';
        
        RAISE NOTICE 'Created table programs.table_student_lecture_progress and compatibility view';
    END IF;
END $$; 
