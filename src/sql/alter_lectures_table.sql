-- Thêm cột teaching_assign_course_id vào bảng table_lectures
ALTER TABLE programs.table_lectures 
ADD COLUMN teaching_assign_course_id UUID;

-- Thêm ràng buộc khóa ngoại cho cột mới
ALTER TABLE programs.table_lectures 
ADD CONSTRAINT fk_lecture_teaching_assign_course
FOREIGN KEY (teaching_assign_course_id) 
REFERENCES programs.table_teaching_assign_courses(id)
ON DELETE CASCADE;
