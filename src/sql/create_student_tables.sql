-- Criar tabela para associar estudantes a programas
CREATE TABLE IF NOT EXISTS programs.table_student_programs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL,
    program_id UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_student_programs_student
        FOREIGN KEY (student_id) 
        REFERENCES users.table_users(id)
        ON DELETE CASCADE,
    
    CONSTRAINT fk_student_programs_program
        FOREIGN KEY (program_id)
        REFERENCES programs.table_programs(id)
        ON DELETE CASCADE
);

-- Criar índices para melhorar performance
CREATE INDEX IF NOT EXISTS idx_table_student_programs_student_id ON programs.table_student_programs(student_id);
CREATE INDEX IF NOT EXISTS idx_table_student_programs_program_id ON programs.table_student_programs(program_id);

-- Criar tabela para matrículas de estudantes em cursos
CREATE TABLE IF NOT EXISTS programs.table_student_enrollments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL,
    teaching_assign_course_id UUID NOT NULL,
    enrollment_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) DEFAULT 'active',
    
    CONSTRAINT fk_student_enrollments_student
        FOREIGN KEY (student_id) 
        REFERENCES users.table_users(id)
        ON DELETE CASCADE,
    
    CONSTRAINT fk_student_enrollments_teaching_assign_course
        FOREIGN KEY (teaching_assign_course_id)
        REFERENCES programs.table_teaching_assign_courses(id)
        ON DELETE CASCADE
);

-- Criar índices para melhorar performance
CREATE INDEX IF NOT EXISTS idx_table_student_enrollments_student_id ON programs.table_student_enrollments(student_id);
CREATE INDEX IF NOT EXISTS idx_table_student_enrollments_teaching_assign_course_id ON programs.table_student_enrollments(teaching_assign_course_id);
