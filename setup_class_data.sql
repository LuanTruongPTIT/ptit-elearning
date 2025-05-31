-- Setup sample data for class and assignments

-- Create sample program
INSERT INTO programs.table_programs (id, name, description, duration_months, created_at, updated_at)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    'Computer Science Program',
    'A comprehensive computer science program',
    36,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Create sample class
INSERT INTO programs.classes (id, program_id, name, description, start_date, end_date, created_at, updated_at)
VALUES (
    '33333333-3333-3333-3333-333333333333',
    '22222222-2222-2222-2222-222222222222',
    'CS101 - Introduction to Programming',
    'Basic programming concepts and algorithms',
    '2024-01-15',
    '2024-05-15',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Assign student to program
INSERT INTO programs.table_student_programs (student_id, program_id, enrollment_date, status, created_at, updated_at)
VALUES (
    'cb273c51-364d-434c-b54f-f0b1032170ec',
    '22222222-2222-2222-2222-222222222222',
    CURRENT_TIMESTAMP,
    'active',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (student_id, program_id) DO NOTHING;

-- Update teaching assignment course to use the class
UPDATE programs.table_teaching_assign_courses 
SET class_id = '33333333-3333-3333-3333-333333333333'
WHERE id = '44444444-4444-4444-4444-444444444444';

-- Update assignment with sample attachment URLs
UPDATE programs.table_assignments 
SET attachment_urls = ARRAY[
    'https://example.com/files/assignment-instructions.pdf',
    'https://example.com/files/sample-code.zip',
    'https://example.com/files/reference-material.docx'
]
WHERE teaching_assign_course_id = '44444444-4444-4444-4444-444444444444';

-- Verify the data
SELECT 
    a.id,
    a.title,
    a.attachment_urls,
    tac.course_class_name,
    c.name as class_name,
    p.name as program_name
FROM programs.table_assignments a
INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
INNER JOIN programs.classes c ON c.id = tac.class_id
INNER JOIN programs.table_programs p ON p.id = c.program_id
WHERE a.teaching_assign_course_id = '44444444-4444-4444-4444-444444444444'; 
