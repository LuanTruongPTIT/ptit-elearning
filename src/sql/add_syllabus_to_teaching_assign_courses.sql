-- Thêm cột syllabus vào bảng table_teaching_assign_courses nếu chưa có
ALTER TABLE programs.table_teaching_assign_courses
ADD COLUMN IF NOT EXISTS syllabus TEXT; 