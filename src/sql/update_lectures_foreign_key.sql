-- Xóa ràng buộc khóa ngoại hiện tại
ALTER TABLE programs.table_lectures DROP CONSTRAINT IF EXISTS fk_lecture_course;

-- Thêm ràng buộc khóa ngoại mới tham chiếu đến table_teaching_assign_courses
ALTER TABLE programs.table_lectures 
ADD CONSTRAINT fk_lecture_teaching_assign_course
FOREIGN KEY (course_id) 
REFERENCES programs.table_teaching_assign_courses(id)
ON DELETE CASCADE;
