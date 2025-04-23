-- Bảng lưu trữ tài liệu của khóa học
CREATE TABLE IF NOT EXISTS programs.table_course_materials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    course_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    file_url TEXT NOT NULL,
    file_type VARCHAR(50) NOT NULL,
    file_size BIGINT NOT NULL, -- Kích thước file tính bằng bytes
    is_published BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL,
    youtube_video_id VARCHAR(50),
    content_type VARCHAR(50) NOT NULL, -- 'VIDEO_UPLOAD' or 'YOUTUBE_LINK'
    
    CONSTRAINT fk_material_course
        FOREIGN KEY (course_id) 
        REFERENCES programs.table_courses(id)
        ON DELETE CASCADE,
    
    CONSTRAINT fk_material_created_by
        FOREIGN KEY (created_by)
        REFERENCES users.table_users(id)
);

-- Index
CREATE INDEX IF NOT EXISTS idx_table_course_materials_course_id ON programs.table_course_materials(course_id);
CREATE INDEX IF NOT EXISTS idx_table_course_materials_created_at ON programs.table_course_materials(created_at DESC);
