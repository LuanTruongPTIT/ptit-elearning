-- Tạo schema content_management nếu chưa có
CREATE SCHEMA IF NOT EXISTS content_management;

-- Tạo bảng sections
CREATE TABLE IF NOT EXISTS content_management.table_sections (
  id UUID PRIMARY KEY,
  teaching_assign_course_id UUID NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  order_index INT NOT NULL DEFAULT 0,
  created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_section_teaching_assign_course
    FOREIGN KEY (teaching_assign_course_id) 
    REFERENCES programs.table_teaching_assign_courses(id)
    ON DELETE CASCADE
);

-- Nếu chưa có bảng lectures, tạo mới
CREATE TABLE IF NOT EXISTS content_management.table_lectures (
  id UUID PRIMARY KEY,
  teaching_assign_course_id UUID NOT NULL,
  section_id UUID,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  duration INT NOT NULL DEFAULT 0, -- Thời lượng tính bằng phút
  type VARCHAR(50) NOT NULL, -- video, document, quiz
  content_url TEXT,
  order_index INT NOT NULL DEFAULT 0,
  created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_lecture_teaching_assign_course
    FOREIGN KEY (teaching_assign_course_id) 
    REFERENCES programs.table_teaching_assign_courses(id)
    ON DELETE CASCADE,
  CONSTRAINT fk_lecture_section
    FOREIGN KEY (section_id) 
    REFERENCES content_management.table_sections(id)
    ON DELETE CASCADE
);

-- Tạo schema enrollment nếu chưa có
CREATE SCHEMA IF NOT EXISTS enrollment;

-- Tạo bảng student_lecture_progresses nếu chưa có
CREATE TABLE IF NOT EXISTS enrollment.table_student_lecture_progresses (
  id UUID PRIMARY KEY,
  student_id UUID NOT NULL,
  lecture_id UUID NOT NULL,
  watch_position INT NOT NULL DEFAULT 0, -- Vị trí đã xem đến (giây)
  progress_percentage INT NOT NULL DEFAULT 0, -- Phần trăm hoàn thành
  is_completed BOOLEAN NOT NULL DEFAULT FALSE,
  last_accessed TIMESTAMP WITHOUT TIME ZONE,
  created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
  CONSTRAINT fk_progress_lecture
    FOREIGN KEY (lecture_id) 
    REFERENCES content_management.table_lectures(id)
    ON DELETE CASCADE
);

-- Thêm các chỉ mục để tối ưu truy vấn
CREATE INDEX IF NOT EXISTS idx_sections_teaching_assign_course_id ON content_management.table_sections(teaching_assign_course_id);
CREATE INDEX IF NOT EXISTS idx_lectures_teaching_assign_course_id ON content_management.table_lectures(teaching_assign_course_id);
CREATE INDEX IF NOT EXISTS idx_lectures_section_id ON content_management.table_lectures(section_id);
CREATE INDEX IF NOT EXISTS idx_student_lecture_progresses_student_id ON enrollment.table_student_lecture_progresses(student_id);
CREATE INDEX IF NOT EXISTS idx_student_lecture_progresses_lecture_id ON enrollment.table_student_lecture_progresses(lecture_id);
CREATE INDEX IF NOT EXISTS idx_student_lecture_progresses_combined ON enrollment.table_student_lecture_progresses(student_id, lecture_id); 