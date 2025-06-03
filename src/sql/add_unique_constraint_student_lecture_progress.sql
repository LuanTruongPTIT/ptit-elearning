-- Add unique constraint to prevent duplicate progress records for same student-lecture combination
ALTER TABLE programs.table_student_lecture_progress
ADD CONSTRAINT uk_student_lecture_progress 
UNIQUE (student_id, lecture_id);
