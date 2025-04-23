-- Bảng lưu trữ bài giảng của khóa học
CREATE TABLE IF NOT EXISTS programs.table_lectures (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    course_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    content_type VARCHAR(50) NOT NULL, -- 'VIDEO_UPLOAD' or 'YOUTUBE_LINK'
    content_url TEXT NOT NULL, -- URL đến file video hoặc YouTube link
    youtube_video_id VARCHAR(50), -- ID của video YouTube (nếu là YouTube link)
    duration INTEGER, -- Thời lượng video (tính bằng giây)
    is_published BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL,
    
    CONSTRAINT fk_lecture_course
        FOREIGN KEY (course_id) 
        REFERENCES programs.table_courses(id)
        ON DELETE CASCADE,
    
    CONSTRAINT fk_lecture_created_by
        FOREIGN KEY (created_by)
        REFERENCES users.table_users(id)
);

-- Index
CREATE INDEX IF NOT EXISTS idx_table_lectures_course_id ON programs.table_lectures(course_id);
CREATE INDEX IF NOT EXISTS idx_table_lectures_created_at ON programs.table_lectures(created_at DESC);
