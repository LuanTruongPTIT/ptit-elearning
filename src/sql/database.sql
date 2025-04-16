-- Tạo schema nếu chưa có
CREATE SCHEMA
IF NOT EXISTS programs;
CREATE SCHEMA
IF NOT EXISTS users;

CREATE TABLE users.table_users
(
  id UUID PRIMARY KEY,
  username VARCHAR(100) NOT NULL UNIQUE,
  email VARCHAR(255) NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  full_name VARCHAR(255),
  phone_number VARCHAR(20),
  address TEXT,
  avatar_url TEXT,
  date_of_birth DATE,
  gender INT,
  account_status INT,
  created_at TIMESTAMP
  WITHOUT TIME ZONE NOT NULL DEFAULT NOW
  (),
  updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW
  ()
);
  CREATE TABLE users.table_roles
  (
    name VARCHAR(50) PRIMARY KEY
  );

  CREATE TABLE users.table_user_roles
  (
    user_id UUID NOT NULL,
    role_name VARCHAR(50) NOT NULL,
    PRIMARY KEY (user_id, role_name),
    CONSTRAINT table_user_roles_user_id_fkey 
        FOREIGN KEY (user_id) REFERENCES users.table_users(id) ON DELETE CASCADE,
    CONSTRAINT table_user_roles_role_name_fkey 
        FOREIGN KEY (role_name) REFERENCES users.table_roles(name) ON DELETE CASCADE
  );


  INSERT INTO users.table_roles
    (name)
  VALUES
    ('Administrator'),
    ('Teacher'),
    ('Student');

  -- Bảng Departments (Khoa)
  CREATE TABLE programs.table_departments
  (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    code VARCHAR(100) NOT NULL,
    created_at TIMESTAMP
    WITHOUT TIME ZONE NOT NULL,
  updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

    -- Bảng Program Units (Ngành học)
    CREATE TABLE programs.table_programs
    (
      id UUID PRIMARY KEY,
      department_id UUID NOT NULL,
      name VARCHAR(255) NOT NULL,
      code VARCHAR(100) NOT NULL,
      degree_type VARCHAR(100) NOT NULL,
      created_at TIMESTAMP
      WITHOUT TIME ZONE NOT NULL,
  updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,

  CONSTRAINT fk_program_unit_department 
    FOREIGN KEY
      (department_id) REFERENCES program.table_departments
      (id)
);

      -- Bảng Courses (Môn học)
      CREATE TABLE programs.table_courses
      (
        id UUID PRIMARY KEY,
        name VARCHAR(255) NOT NULL,
        code VARCHAR(100) NOT NULL,
        created_at TIMESTAMP
        WITHOUT TIME ZONE NOT NULL,
  updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

        -- Bảng trung gian ProgramUnit - Courses (many-to-many)
        CREATE TABLE programs.table_program_courses
        (
          course_id UUID NOT NULL,
          program_id UUID NOT NULL,

          PRIMARY KEY (course_id, program_id),

          CONSTRAINT fk_course
    FOREIGN KEY (course_id) REFERENCES programs.table_courses(id)
    ON DELETE CASCADE,

          CONSTRAINT fk_program_unit
    FOREIGN KEY (program_id) REFERENCES programs.table_programs(id)
    ON DELETE CASCADE
        );


        CREATE TABLE programs.table_teaching_assignments
        (
          id UUID PRIMARY KEY,
          subjects UUID
          [] NOT NULL,
    employed_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW
          (),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW
          ()
);


          CREATE TABLE programs.teaching_assignment
          (
            id UUID PRIMARY KEY,
            teacher_id UUID NOT NULL,
            department_id UUID NOT NULL,
            subjects UUID
            [] NOT NULL,
    employed_date TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
