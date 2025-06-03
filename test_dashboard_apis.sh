#!/bin/bash

# Test script for Student Dashboard APIs
echo "🧪 Testing Student Dashboard APIs..."
echo "=================================="

# API base URL
BASE_URL="http://localhost:5093"

# Test student ID (you can change this)
STUDENT_ID="123e4567-e89b-12d3-a456-426614174000"

# Function to test API endpoint
test_api() {
    local endpoint=$1
    local description=$2
    
    echo ""
    echo "📡 Testing: $description"
    echo "Endpoint: $endpoint"
    echo "---"
    
    response=$(curl -s -w "\n%{http_code}" "$endpoint")
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | head -n -1)
    
    if [ "$http_code" = "200" ]; then
        echo "✅ SUCCESS (HTTP $http_code)"
        echo "Response preview:"
        echo "$body" | jq '.' 2>/dev/null || echo "$body" | head -c 200
        echo "..."
    else
        echo "❌ FAILED (HTTP $http_code)"
        echo "Error response:"
        echo "$body"
    fi
    echo ""
}

# Test all dashboard APIs
echo "Testing with Student ID: $STUDENT_ID"
echo ""

# 1. Main Dashboard Data
test_api "$BASE_URL/program/student/dashboard?studentId=$STUDENT_ID" "Main Dashboard Data"

# 2. Dashboard Stats
test_api "$BASE_URL/program/student/dashboard/stats?studentId=$STUDENT_ID" "Dashboard Statistics"

# 3. Recent Courses
test_api "$BASE_URL/program/student/dashboard/recent-courses?studentId=$STUDENT_ID" "Recent Courses"

# 4. Upcoming Deadlines
test_api "$BASE_URL/program/student/dashboard/deadlines?studentId=$STUDENT_ID" "Upcoming Deadlines"

# 5. Recent Activities
test_api "$BASE_URL/program/student/dashboard/activities?studentId=$STUDENT_ID&limit=10&offset=0" "Recent Activities"

# 6. Progress Data
test_api "$BASE_URL/program/student/dashboard/progress?studentId=$STUDENT_ID" "Progress Over Time"

# 7. Weekly Study Data
test_api "$BASE_URL/program/student/dashboard/weekly-study?studentId=$STUDENT_ID" "Weekly Study Hours"

# 8. Subject Distribution
test_api "$BASE_URL/program/student/dashboard/subjects?studentId=$STUDENT_ID" "Subject Distribution"

echo ""
echo "🏁 Testing completed!"
echo ""
echo "📝 Notes:"
echo "- Make sure the backend is running on port 5093"
echo "- Update STUDENT_ID variable with a valid student ID from your database"
echo "- APIs should return either real data or fallback mock data"
echo "- All APIs should return HTTP 200 even if no real data exists" 
