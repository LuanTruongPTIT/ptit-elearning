-- Tạo schema enrollment nếu chưa có
CREATE SCHEMA
IF NOT EXISTS enrollment;

-- Tạo bảng student_lecture_progresses để lưu tiến độ học tập của sinh viên theo từng bài giảng
CREATE TABLE programs.table_student_course_progress
(
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  student_id UUID NOT NULL,
  teaching_assign_course_id UUID NOT NULL,
  total_lectures INT NOT NULL,
  completed_lectures INT NOT NULL,
  progress_percentage INT NOT NULL,
  last_accessed TIMESTAMP,
  status VARCHAR(20) NOT NULL DEFAULT 'in_progress',
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE (student_id, teaching_assign_course_id)
);

-- Tạo các chỉ mục để tối ưu truy vấn
CREATE INDEX
IF NOT EXISTS idx_student_lecture_progresses_student_id 
    ON programs.table_student_lecture_progress
(student_id);
CREATE INDEX
IF NOT EXISTS idx_student_lecture_progresses_lecture_id 
    ON programs.table_student_lecture_progress
(lecture_id);

-- Tạo trigger để tự động cập nhật trường updated_at
CREATE OR REPLACE FUNCTION enrollment.update_student_lecture_progress_updated_at
()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_student_lecture_progress_updated_at
BEFORE
UPDATE ON programs.table_student_lecture_progress
FOR EACH ROW
EXECUTE FUNCTION enrollment
.update_student_lecture_progress_updated_at
();
