-- Debug Student Progress Data
-- This script helps investigate why inProgress courses is showing 0

-- 1. Check student enrollment
SELECT 
    sp.student_id,
    sp.program_id,
    u.full_name as student_name,
    u.email
FROM programs.table_student_programs sp
JOIN users.table_users u ON sp.student_id = u.id
LIMIT 5;

-- 2. Check student courses
SELECT 
    sc.student_id,
    tac.id as teaching_assign_course_id,
    tac.course_class_name,
    tac.status,
    c.id as class_id,
    c.class_name
FROM programs.table_student_programs sp
JOIN programs.classes c ON sp.program_id = c.program_id
JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
WHERE tac.status = 'active'
LIMIT 10;

-- 3. Check lectures for courses
SELECT 
    l.id as lecture_id,
    l.title,
    l.teaching_assign_course_id,
    l.is_published,
    tac.course_class_name
FROM programs.table_lectures l
JOIN programs.table_teaching_assign_courses tac ON l.teaching_assign_course_id = tac.id
WHERE l.is_published = true
LIMIT 10;

-- 4. Check student lecture progress
SELECT 
    slp.student_id,
    slp.lecture_id,
    slp.is_completed,
    slp.progress_percentage,
    l.title as lecture_title,
    tac.course_class_name
FROM programs.table_student_lecture_progress slp
JOIN programs.table_lectures l ON slp.lecture_id = l.id
JOIN programs.table_teaching_assign_courses tac ON l.teaching_assign_course_id = tac.id
LIMIT 10;

-- 5. Check student course progress
SELECT 
    scp.student_id,
    scp.teaching_assign_course_id,
    scp.total_lectures,
    scp.completed_lectures,
    scp.progress_percentage,
    scp.status,
    tac.course_class_name
FROM programs.table_student_course_progress scp
JOIN programs.table_teaching_assign_courses tac ON scp.teaching_assign_course_id = tac.id
LIMIT 10;

-- 6. Check for specific student progress
-- Replace '550e8400-e29b-41d4-a716-446655440000' with actual student ID
WITH student_data AS (
    SELECT student_id FROM programs.table_student_programs LIMIT 1
)
SELECT 
    scp.student_id,
    scp.teaching_assign_course_id,
    scp.total_lectures,
    scp.completed_lectures,
    scp.progress_percentage,
    scp.status,
    tac.course_class_name,
    CASE 
        WHEN scp.progress_percentage >= 100 THEN 'completed'
        WHEN scp.progress_percentage > 0 AND scp.progress_percentage < 100 THEN 'in_progress'
        ELSE 'not_started'
    END as calculated_status
FROM programs.table_student_course_progress scp
JOIN programs.table_teaching_assign_courses tac ON scp.teaching_assign_course_id = tac.id
CROSS JOIN student_data sd
WHERE scp.student_id = sd.student_id;

-- 7. Count courses by status for all students
SELECT 
    scp.student_id,
    COUNT(CASE WHEN scp.progress_percentage >= 100 THEN 1 END) as completed_courses,
    COUNT(CASE WHEN scp.progress_percentage > 0 AND scp.progress_percentage < 100 THEN 1 END) as in_progress_courses,
    COUNT(CASE WHEN scp.progress_percentage = 0 THEN 1 END) as not_started_courses,
    COUNT(*) as total_courses
FROM programs.table_student_course_progress scp
GROUP BY scp.student_id;

-- 8. Check if there are any lecture progresses but missing course progress
SELECT DISTINCT
    slp.student_id,
    l.teaching_assign_course_id,
    COUNT(l.id) as total_lectures,
    COUNT(CASE WHEN slp.is_completed = true THEN 1 END) as completed_lectures,
    scp.id as course_progress_exists
FROM programs.table_student_lecture_progress slp
JOIN programs.table_lectures l ON slp.lecture_id = l.id
LEFT JOIN programs.table_student_course_progress scp 
    ON slp.student_id = scp.student_id 
    AND l.teaching_assign_course_id = scp.teaching_assign_course_id
WHERE l.is_published = true
GROUP BY slp.student_id, l.teaching_assign_course_id, scp.id
HAVING COUNT(CASE WHEN slp.is_completed = true THEN 1 END) > 0
ORDER BY slp.student_id, l.teaching_assign_course_id; 
