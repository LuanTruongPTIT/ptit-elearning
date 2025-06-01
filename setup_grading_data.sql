-- Setup sample data for grading feature

-- Create sample assignment submissions
INSERT INTO programs.table_assignment_submissions (
    id, assignment_id, student_id, submission_type, file_urls, text_content, 
    submitted_at, grade, feedback, status, created_at, updated_at
) VALUES 
-- Sample submission 1 - Already graded
(
    gen_random_uuid(),
    '11111111-1111-1111-1111-111111111111', -- Assignment ID
    'cb273c51-364d-434c-b54f-f0b1032170ec', -- Student ID (Nguyen Cuong)
    'file',
    '["assignment1.java", "readme.txt"]'::json,
    'Giải thích thuật toán: Sử dụng design pattern Strategy để implement các loại sắp xếp khác nhau.',
    '2024-01-14T10:30:00Z',
    8.5,
    'Bài làm tốt, logic rõ ràng. Cần cải thiện comment code.',
    'graded',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
-- Sample submission 2 - Pending grading
(
    gen_random_uuid(),
    '11111111-1111-1111-1111-111111111111', -- Assignment ID
    '68c0b432-744f-4f2a-a5ce-627930766a3c', -- Student ID (luantruong1)
    'file',
    '["project.zip"]'::json,
    'Mô tả: Đã implement đầy đủ các yêu cầu của bài tập. Code được tổ chức theo mô hình MVC.',
    '2024-01-15T20:45:00Z',
    NULL,
    NULL,
    'submitted',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
-- Sample submission 3 - Late submission
(
    gen_random_uuid(),
    '11111111-1111-1111-1111-111111111111', -- Assignment ID
    '6ce1ac6e-2dd3-4905-8e36-0b371cfbd8a1', -- Student ID (user)
    'file',
    '["late_submission.java", "documentation.pdf"]'::json,
    NULL,
    '2024-01-16T08:15:00Z', -- After deadline
    NULL,
    NULL,
    'submitted',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
);

-- Update assignment to have proper max_score
UPDATE programs.table_assignments 
SET max_score = 10.0
WHERE id = '11111111-1111-1111-1111-111111111111';

-- Create some notifications for graded assignments
INSERT INTO programs.table_notifications (
    id, student_id, assignment_id, type, title, message, created_at
) VALUES 
(
    gen_random_uuid(),
    'cb273c51-364d-434c-b54f-f0b1032170ec',
    '11111111-1111-1111-1111-111111111111',
    'assignment_graded',
    'Bài tập đã được chấm điểm',
    'Bài tập "Sample Assignment" đã được chấm điểm: 8.5/10',
    CURRENT_TIMESTAMP
);

-- Verify data
SELECT 
    'Assignment Submissions' as table_name,
    COUNT(*) as count
FROM programs.table_assignment_submissions
WHERE assignment_id = '11111111-1111-1111-1111-111111111111'

UNION ALL

SELECT 
    'Assignment Info' as table_name,
    COUNT(*) as count
FROM programs.table_assignments
WHERE id = '11111111-1111-1111-1111-111111111111'; 
