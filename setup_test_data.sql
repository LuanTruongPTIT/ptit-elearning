-- Setup Test Data for Student Dashboard
-- This script creates test data to verify the dashboard functionality

-- Insert test program if not exists
INSERT INTO programs.table_programs (id, name, description, code, created_at, updated_at)
VALUES (
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Computer Science Program',
    'A comprehensive computer science program',
    'CS2024',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Insert test class if not exists
INSERT INTO programs.classes (id, class_name, program_id, status, created_at, updated_at)
VALUES (
    'b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'CS101 - Introduction to Programming',
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'active',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Insert test student if not exists
INSERT INTO users.table_users (id, email, full_name, role_name, created_at, updated_at)
VALUES (
    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    'student.test@example.com',
    'Test Student',
    'Student',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Insert test teacher if not exists
INSERT INTO users.table_users (id, email, full_name, role_name, created_at, updated_at)
VALUES (
    '4fa85f64-5717-4562-b3fc-2c963f66afa6',
    'teacher.test@example.com',
    'Test Teacher',
    'Teacher',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Insert test course if not exists
INSERT INTO programs.table_courses (id, code, name, description, credit_hours, created_at, updated_at)
VALUES (
    'c0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'PROG101',
    'Introduction to Programming',
    'Learn the basics of programming',
    3,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Insert another test course
INSERT INTO programs.table_courses (id, code, name, description, credit_hours, created_at, updated_at)
VALUES (
    'c1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'WEB101',
    'Web Development Basics',
    'Learn web development fundamentals',
    3,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Enroll student in program
INSERT INTO programs.table_student_programs (student_id, program_id, enrollment_date)
VALUES (
    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    CURRENT_TIMESTAMP
) ON CONFLICT (student_id, program_id) DO NOTHING;

-- Insert teaching assign courses
INSERT INTO programs.table_teaching_assign_courses (
    id, course_id, teacher_id, class_id, course_class_name, description, 
    start_date, end_date, status, created_at, updated_at
)
VALUES (
    'd0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'c0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    '4fa85f64-5717-4562-b3fc-2c963f66afa6',
    'b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Introduction to Programming - Fall 2024',
    'Programming fundamentals course',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP + INTERVAL '3 months',
    'active',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

INSERT INTO programs.table_teaching_assign_courses (
    id, course_id, teacher_id, class_id, course_class_name, description, 
    start_date, end_date, status, created_at, updated_at
)
VALUES (
    'd1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'c1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    '4fa85f64-5717-4562-b3fc-2c963f66afa6',
    'b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Web Development Basics - Fall 2024',
    'Web development fundamentals',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP + INTERVAL '3 months',
    'active',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT (id) DO NOTHING;

-- Insert test lectures
INSERT INTO programs.table_lectures (
    id, title, content, teaching_assign_course_id, 
    order_index, duration_minutes, is_published, created_at, updated_at
)
VALUES 
(
    'e0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Introduction to Programming Concepts',
    'Basic programming concepts and syntax',
    'd0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    1,
    60,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'e1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Variables and Data Types',
    'Understanding variables and data types',
    'd0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    2,
    45,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'e2eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Control Structures',
    'If statements, loops, and control flow',
    'd0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    3,
    50,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'e3eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'HTML Basics',
    'Introduction to HTML',
    'd1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    1,
    40,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'e4eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'CSS Fundamentals',
    'Styling with CSS',
    'd1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    2,
    45,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (id) DO NOTHING;

-- Insert student lecture progress (student completed first lecture, partially completed second)
INSERT INTO programs.table_student_lecture_progress (
    id, student_id, lecture_id, watch_position, progress_percentage, 
    is_completed, last_accessed, created_at, updated_at
)
VALUES 
(
    'f0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    'e0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    3600, -- watched full 60 minutes
    100,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'f1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    'e1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    1350, -- watched 22.5 minutes out of 45
    50,
    false,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'f2eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    'e3eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    2400, -- watched full 40 minutes  
    100,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (student_id, lecture_id) DO NOTHING;

-- The student course progress will be automatically calculated when GetStudentCoursesQuery is called
-- But let's insert initial records to ensure they exist
INSERT INTO programs.table_student_course_progress (
    id, student_id, teaching_assign_course_id, total_lectures, completed_lectures,
    progress_percentage, status, last_accessed, created_at, updated_at
)
VALUES 
(
    'g0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    'd0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    3, -- total lectures in programming course
    1, -- completed lectures
    33, -- 1/3 = 33%
    'in_progress',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'g1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    'd1eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    2, -- total lectures in web dev course
    1, -- completed lectures
    50, -- 1/2 = 50%
    'in_progress',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (student_id, teaching_assign_course_id) DO NOTHING;

-- Verify the data
SELECT 'Test data setup completed successfully!' as message;
SELECT 
    COUNT(*) as student_courses,
    COUNT(CASE WHEN progress_percentage >= 100 THEN 1 END) as completed,
    COUNT(CASE WHEN progress_percentage > 0 AND progress_percentage < 100 THEN 1 END) as in_progress,
    COUNT(CASE WHEN progress_percentage = 0 THEN 1 END) as not_started
FROM programs.table_student_course_progress 
WHERE student_id = '3fa85f64-5717-4562-b3fc-2c963f66afa6'; 
