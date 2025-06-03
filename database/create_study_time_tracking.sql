-- Create table to track student study time
CREATE TABLE IF NOT EXISTS programs.table_study_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL,
    course_id UUID,
    lecture_id UUID,
    session_start TIMESTAMP WITH TIME ZONE NOT NULL,
    session_end TIMESTAMP WITH TIME ZONE,
    duration_minutes INTEGER,
    activity_type VARCHAR(50) NOT NULL, -- 'lecture', 'assignment', 'quiz', 'reading'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_study_sessions_student_id ON programs.table_study_sessions(student_id);
CREATE INDEX IF NOT EXISTS idx_study_sessions_date ON programs.table_study_sessions(session_start);
CREATE INDEX IF NOT EXISTS idx_study_sessions_course ON programs.table_study_sessions(course_id);

-- Insert some sample data for testing
INSERT INTO programs.table_study_sessions (student_id, course_id, session_start, session_end, duration_minutes, activity_type)
VALUES 
    -- Sample data for last week
    ('550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440001', 
     CURRENT_DATE - INTERVAL '6 days' + TIME '09:00:00', 
     CURRENT_DATE - INTERVAL '6 days' + TIME '10:30:00', 90, 'lecture'),
    
    ('550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440001', 
     CURRENT_DATE - INTERVAL '5 days' + TIME '14:00:00', 
     CURRENT_DATE - INTERVAL '5 days' + TIME '15:00:00', 60, 'assignment'),
     
    ('550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440002', 
     CURRENT_DATE - INTERVAL '4 days' + TIME '10:00:00', 
     CURRENT_DATE - INTERVAL '4 days' + TIME '12:00:00', 120, 'lecture'),
     
    ('550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440001', 
     CURRENT_DATE - INTERVAL '3 days' + TIME '16:00:00', 
     CURRENT_DATE - INTERVAL '3 days' + TIME '17:30:00', 90, 'quiz'),
     
    ('550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440002', 
     CURRENT_DATE - INTERVAL '2 days' + TIME '11:00:00', 
     CURRENT_DATE - INTERVAL '2 days' + TIME '11:45:00', 45, 'reading'),
     
    ('550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440001', 
     CURRENT_DATE - INTERVAL '1 days' + TIME '15:00:00', 
     CURRENT_DATE - INTERVAL '1 days' + TIME '18:00:00', 180, 'assignment'),
     
    ('550e8400-e29b-41d4-a716-446655440000', '550e8400-e29b-41d4-a716-446655440002', 
     CURRENT_DATE + TIME '09:00:00', 
     CURRENT_DATE + TIME '11:30:00', 150, 'lecture');

-- Create trigger to automatically update updated_at
CREATE OR REPLACE FUNCTION update_study_sessions_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_study_sessions_updated_at
    BEFORE UPDATE ON programs.table_study_sessions
    FOR EACH ROW
    EXECUTE FUNCTION update_study_sessions_updated_at(); 
