#!/bin/bash

# Database setup script for Student Dashboard
echo "Setting up database for Student Dashboard..."

# Database connection details
DB_HOST="localhost"
DB_PORT="5432"
DB_NAME="elearning_db"
DB_USER="postgres"

# Function to run SQL file
run_sql_file() {
    local file=$1
    echo "Running $file..."
    if [ -f "$file" ]; then
        psql -h $DB_HOST -p $DB_PORT -d $DB_NAME -U $DB_USER -f "$file"
        if [ $? -eq 0 ]; then
            echo "✅ $file executed successfully"
        else
            echo "❌ Error executing $file"
        fi
    else
        echo "⚠️  File $file not found"
    fi
    echo ""
}

# Run database setup files in order
echo "1. Setting up basic tables..."
run_sql_file "src/sql/database.sql"

echo "2. Setting up student tables..."
run_sql_file "src/sql/create_student_tables.sql"

echo "3. Setting up course progress tracking..."
run_sql_file "src/sql/create_student_course_progress_table.sql"

echo "4. Setting up assignments and activities..."
run_sql_file "src/sql/create_assignments_and_activities_tables.sql"

echo "5. Setting up lectures..."
run_sql_file "src/sql/create_lectures_table.sql"
run_sql_file "src/sql/update_lectures_table_structure.sql"

echo "6. Synchronizing student lecture progress table..."
run_sql_file "database/sync_student_lecture_progress_table.sql"

echo "7. Setting up notifications..."
run_sql_file "database-setup-notifications.sql"

echo ""
echo "Database setup completed!"
echo ""
echo "Next steps:"
echo "1. Start the backend: dotnet run --project src/Elearning.Api/Elearning.Api.csproj"
echo "2. Test APIs: ./test_dashboard_apis.sh"
echo "3. Start frontend: cd ../e-learning-ui && npm run dev" 
