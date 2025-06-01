-- Script để thêm cột grade và các cột liên quan vào bảng assignment submissions
-- Chạy script này để cập nhật cấu trúc bảng

-- Kiểm tra xem cột grade đã tồn tại chưa
DO $$
BEGIN
    -- Thêm cột grade nếu chưa tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_assignment_submissions' 
        AND column_name = 'grade'
    ) THEN
        ALTER TABLE programs.table_assignment_submissions 
        ADD COLUMN grade DECIMAL(5,2);
        
        RAISE NOTICE 'Đã thêm cột grade vào bảng table_assignment_submissions';
    ELSE
        RAISE NOTICE 'Cột grade đã tồn tại trong bảng table_assignment_submissions';
    END IF;

    -- Thêm cột feedback nếu chưa tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_assignment_submissions' 
        AND column_name = 'feedback'
    ) THEN
        ALTER TABLE programs.table_assignment_submissions 
        ADD COLUMN feedback TEXT;
        
        RAISE NOTICE 'Đã thêm cột feedback vào bảng table_assignment_submissions';
    ELSE
        RAISE NOTICE 'Cột feedback đã tồn tại trong bảng table_assignment_submissions';
    END IF;

    -- Thêm cột status nếu chưa tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_assignment_submissions' 
        AND column_name = 'status'
    ) THEN
        ALTER TABLE programs.table_assignment_submissions 
        ADD COLUMN status VARCHAR(50) DEFAULT 'submitted';
        
        RAISE NOTICE 'Đã thêm cột status vào bảng table_assignment_submissions';
    ELSE
        RAISE NOTICE 'Cột status đã tồn tại trong bảng table_assignment_submissions';
    END IF;

    -- Thêm cột submission_type nếu chưa tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_assignment_submissions' 
        AND column_name = 'submission_type'
    ) THEN
        ALTER TABLE programs.table_assignment_submissions 
        ADD COLUMN submission_type VARCHAR(50) DEFAULT 'file';
        
        RAISE NOTICE 'Đã thêm cột submission_type vào bảng table_assignment_submissions';
    ELSE
        RAISE NOTICE 'Cột submission_type đã tồn tại trong bảng table_assignment_submissions';
    END IF;

    -- Thêm cột file_urls nếu chưa tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_assignment_submissions' 
        AND column_name = 'file_urls'
    ) THEN
        ALTER TABLE programs.table_assignment_submissions 
        ADD COLUMN file_urls JSON;
        
        RAISE NOTICE 'Đã thêm cột file_urls vào bảng table_assignment_submissions';
    ELSE
        RAISE NOTICE 'Cột file_urls đã tồn tại trong bảng table_assignment_submissions';
    END IF;

    -- Thêm cột text_content nếu chưa tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_assignment_submissions' 
        AND column_name = 'text_content'
    ) THEN
        ALTER TABLE programs.table_assignment_submissions 
        ADD COLUMN text_content TEXT;
        
        RAISE NOTICE 'Đã thêm cột text_content vào bảng table_assignment_submissions';
    ELSE
        RAISE NOTICE 'Cột text_content đã tồn tại trong bảng table_assignment_submissions';
    END IF;

    -- Thêm cột quiz_attempt_id nếu chưa tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'programs' 
        AND table_name = 'table_assignment_submissions' 
        AND column_name = 'quiz_attempt_id'
    ) THEN
        ALTER TABLE programs.table_assignment_submissions 
        ADD COLUMN quiz_attempt_id UUID;
        
        RAISE NOTICE 'Đã thêm cột quiz_attempt_id vào bảng table_assignment_submissions';
    ELSE
        RAISE NOTICE 'Cột quiz_attempt_id đã tồn tại trong bảng table_assignment_submissions';
    END IF;

END $$;

-- Cập nhật các record có status NULL thành 'submitted'
UPDATE programs.table_assignment_submissions 
SET status = 'submitted' 
WHERE status IS NULL;

-- Cập nhật các record có submission_type NULL thành 'file'
UPDATE programs.table_assignment_submissions 
SET submission_type = 'file' 
WHERE submission_type IS NULL;

-- Thêm constraint NOT NULL cho status sau khi đã cập nhật dữ liệu
ALTER TABLE programs.table_assignment_submissions 
ALTER COLUMN status SET NOT NULL;

-- Thêm constraint NOT NULL cho submission_type sau khi đã cập nhật dữ liệu
ALTER TABLE programs.table_assignment_submissions 
ALTER COLUMN submission_type SET NOT NULL;

-- Tạo indexes để tối ưu performance nếu chưa tồn tại
CREATE INDEX IF NOT EXISTS idx_submissions_grade 
ON programs.table_assignment_submissions(grade);

CREATE INDEX IF NOT EXISTS idx_submissions_status 
ON programs.table_assignment_submissions(status);

-- Kiểm tra cấu trúc bảng sau khi cập nhật
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_schema = 'programs' 
AND table_name = 'table_assignment_submissions'
ORDER BY ordinal_position;

-- Kiểm tra số lượng record đã được cập nhật
SELECT 
    status,
    COUNT(*) as count
FROM programs.table_assignment_submissions
GROUP BY status;

-- Thông báo hoàn thành
SELECT 'Đã hoàn thành việc cập nhật cấu trúc bảng table_assignment_submissions và dữ liệu' as message; 
