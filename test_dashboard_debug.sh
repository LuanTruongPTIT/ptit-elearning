#!/bin/bash

echo "🔍 Debugging Student Dashboard API"
echo "=================================="

# Wait for API to be ready
echo "⏳ Waiting for API to start..."
sleep 10

# Test with a specific student ID (you may need to change this)
STUDENT_ID="3fa85f64-5717-4562-b3fc-2c963f66afa6"
BASE_URL="http://localhost:5093"

echo ""
echo "📊 Testing Student Dashboard Stats API"
echo "URL: $BASE_URL/program/student/dashboard/stats?studentId=$STUDENT_ID"
echo ""

response=$(curl -s -w "\n%{http_code}" "$BASE_URL/program/student/dashboard/stats?studentId=$STUDENT_ID")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n -1)

echo "HTTP Status: $http_code"
echo "Response:"
echo "$body" | jq '.' 2>/dev/null || echo "$body"

echo ""
echo "📚 Testing Student Courses API"
echo "URL: $BASE_URL/student/courses?student_id=$STUDENT_ID"
echo ""

response2=$(curl -s -w "\n%{http_code}" "$BASE_URL/student/courses?student_id=$STUDENT_ID")
http_code2=$(echo "$response2" | tail -n1)
body2=$(echo "$response2" | head -n -1)

echo "HTTP Status: $http_code2"
echo "Response:"
echo "$body2" | jq '.' 2>/dev/null || echo "$body2"

echo ""
echo "🔄 Testing without student ID (should use current user)"
echo "URL: $BASE_URL/program/student/dashboard/stats"
echo ""

response3=$(curl -s -w "\n%{http_code}" "$BASE_URL/program/student/dashboard/stats")
http_code3=$(echo "$response3" | tail -n1)
body3=$(echo "$response3" | head -n -1)

echo "HTTP Status: $http_code3"
echo "Response:"
echo "$body3" | jq '.' 2>/dev/null || echo "$body3"

echo ""
echo "�� Debug complete!" 
