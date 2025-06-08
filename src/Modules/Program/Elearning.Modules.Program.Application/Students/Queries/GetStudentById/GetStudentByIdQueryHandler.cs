using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Students.Queries.GetStudentById;

internal sealed class GetStudentByIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetStudentByIdQuery, GetStudentByIdResponse>
{
  public async Task<Result<GetStudentByIdResponse>> Handle(
      GetStudentByIdQuery request,
      CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

    try
    {
      var studentId = Guid.Parse(request.StudentId);

      // Main student info query
      var studentSql = """
                SELECT 
                    u.id,
                    u.full_name as name,
                    u.email,
                    COALESCE(u.phone_number, '') as phone_number,
                    COALESCE(u.date_of_birth, NOW()) as date_of_birth,
                    u.created_at as enrollment_date,
                    COALESCE(u.account_status, 1) as status,
                    COALESCE(u.avatar_url, '') as avatar,
                    COALESCE(u.address, '') as address,
                    COALESCE(d.name, 'Chưa xác định') as department,
                    COALESCE(p.name, 'Chưa xác định') as program
                FROM users.table_users u
                LEFT JOIN users.table_user_roles ur ON u.id = ur.user_id
                LEFT JOIN programs.table_student_programs sp ON u.id = sp.student_id
                LEFT JOIN programs.table_programs p ON sp.program_id = p.id
                LEFT JOIN programs.table_departments d ON p.department_id = d.id
                WHERE u.id = @studentId AND (ur.role_name = 'Student' OR ur.role_name IS NULL)
                LIMIT 1
            """;

      var student = await connection.QueryFirstOrDefaultAsync(studentSql, new { studentId });
      if (student == null)
      {
        return Result.Failure<GetStudentByIdResponse>(
            Error.NotFound("Student.NotFound", "Student not found"));
      }

      // Course statistics
      var courseStatsSql = """
                SELECT 
                    COUNT(DISTINCT tac.id) as total_courses,
                    COUNT(DISTINCT CASE WHEN scp.progress_percentage >= 100 THEN tac.id END) as completed_courses,
                    COUNT(DISTINCT CASE WHEN scp.progress_percentage > 0 AND scp.progress_percentage < 100 THEN tac.id END) as in_progress_courses,
                    COALESCE(AVG(CASE WHEN scp.progress_percentage >= 100 THEN scp.progress_percentage END), 0) as avg_completion_rate
                FROM programs.table_student_programs sp
                LEFT JOIN programs.classes c ON sp.program_id = c.program_id
                LEFT JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                    AND tac.id = scp.teaching_assign_course_id
                WHERE sp.student_id = @studentId
            """;

      var courseStats = await connection.QueryFirstOrDefaultAsync(courseStatsSql, new { studentId });

      // Real assignment statistics from table_assignment_submissions
      var assignmentStatsSql = """
                SELECT 
                    COUNT(DISTINCT a.id) as total_assignments,
                    COUNT(DISTINCT CASE WHEN asub.status IN ('submitted', 'graded') THEN a.id END) as completed_assignments,
                    COUNT(DISTINCT CASE WHEN asub.grade IS NOT NULL THEN asub.id END) as graded_assignments,
                    COALESCE(AVG(asub.grade), 0) as average_score,
                    COALESCE(MIN(asub.grade), 0) as min_score,
                    COALESCE(MAX(asub.grade), 0) as max_score,
                    COUNT(DISTINCT CASE WHEN asub.submitted_at > a.deadline THEN asub.id END) as late_submissions,
                    SUM(CASE WHEN a.created_at >= NOW() - INTERVAL '30 days' THEN 1 ELSE 0 END) as assignments_this_month
                FROM programs.table_student_programs sp
                LEFT JOIN programs.classes c ON sp.program_id = c.program_id
                LEFT JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                LEFT JOIN programs.table_assignments a ON tac.id = a.teaching_assign_course_id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id AND asub.student_id = sp.student_id
                WHERE sp.student_id = @studentId
            """;

      var assignmentStats = await connection.QueryFirstOrDefaultAsync(assignmentStatsSql, new { studentId });

      // Calculate GPA based on real assignment scores
      decimal gpa = 0;
      if (assignmentStats?.average_score > 0)
      {
        // Convert 0-100 score to 4.0 scale
        decimal avgScore = (decimal)(assignmentStats.average_score);
        gpa = avgScore >= 90 ? 4.0m :
              avgScore >= 80 ? 3.0m + (avgScore - 80) * 0.1m :
              avgScore >= 70 ? 2.0m + (avgScore - 70) * 0.1m :
              avgScore >= 60 ? 1.0m + (avgScore - 60) * 0.1m :
              0.0m;
        gpa = Math.Round(gpa, 2);
      }

      // Course progress query with real scores
      var courseProgressSql = """
                SELECT 
                    COALESCE(c.id::text, '') as course_id,
                    COALESCE(c.name, 'Unknown Course') as course_name,
                    COALESCE(c.code, 'N/A') as course_code,
                    COALESCE(u.full_name, 'Chưa phân công') as teacher_name,
                    COALESCE(scp.progress_percentage, 0) as progress_percentage,
                    COALESCE(course_grades.avg_grade, 0) as current_score,
                    CASE 
                        WHEN scp.progress_percentage >= 100 THEN 'Completed'
                        WHEN scp.progress_percentage > 0 THEN 'In Progress'
                        ELSE 'Not Started'
                    END as status,
                    COALESCE(scp.last_accessed::text, NOW()::text) as last_accessed
                FROM programs.table_student_programs sp
                LEFT JOIN programs.classes cls ON sp.program_id = cls.program_id
                LEFT JOIN programs.table_teaching_assign_courses tac ON cls.id = tac.class_id
                LEFT JOIN programs.table_courses c ON tac.course_id = c.id
                LEFT JOIN users.table_users u ON tac.teacher_id = u.id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                    AND tac.id = scp.teaching_assign_course_id
                LEFT JOIN (
                    SELECT 
                        a.teaching_assign_course_id,
                        asub.student_id,
                        AVG(asub.grade) as avg_grade
                    FROM programs.table_assignments a
                    LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                    WHERE asub.grade IS NOT NULL
                    GROUP BY a.teaching_assign_course_id, asub.student_id
                ) course_grades ON tac.id = course_grades.teaching_assign_course_id AND sp.student_id = course_grades.student_id
                WHERE sp.student_id = @studentId AND c.id IS NOT NULL
                ORDER BY c.name
                LIMIT 10
            """;

      var courseProgresses = await connection.QueryAsync(courseProgressSql, new { studentId });

      // Real monthly activities from assignment submissions and course progress
      var monthlyActivitiesSql = """
                SELECT 
                    TO_CHAR(activity_date, 'MM/yyyy') as month,
                    COUNT(DISTINCT assignment_id) as assignments_completed,
                    AVG(grade) as average_score,
                    SUM(study_time) as study_hours,
                    COUNT(DISTINCT quiz_id) as quizzes_taken
                FROM (
                    -- Assignment submissions
                    SELECT 
                        DATE_TRUNC('month', asub.submitted_at) as activity_date,
                        asub.assignment_id,
                        asub.grade,
                        1 as study_time, -- Estimate 1 hour per assignment
                        NULL as quiz_id
                    FROM programs.table_assignment_submissions asub
                    WHERE asub.student_id = @studentId 
                    AND asub.submitted_at IS NOT NULL
                    AND asub.submitted_at >= NOW() - INTERVAL '6 months'
                    
                    UNION ALL
                    
                    -- Course access activities
                    SELECT 
                        DATE_TRUNC('month', scp.last_accessed) as activity_date,
                        NULL as assignment_id,
                        NULL as grade,
                        2 as study_time, -- Estimate 2 hours per course access
                        NULL as quiz_id
                    FROM programs.table_student_course_progress scp
                    WHERE scp.student_id = @studentId 
                    AND scp.last_accessed IS NOT NULL
                    AND scp.last_accessed >= NOW() - INTERVAL '6 months'
                ) activities
                GROUP BY TO_CHAR(activity_date, 'MM/yyyy'), activity_date
                ORDER BY activity_date DESC
                LIMIT 6
            """;

      var monthlyActivitiesData = await connection.QueryAsync(monthlyActivitiesSql, new { studentId });

      var monthlyActivities = monthlyActivitiesData.Select(ma => new MonthlyActivity
      {
        Month = ma.month?.ToString() ?? DateTime.Now.ToString("MM/yyyy"),
        StudyHours = (int)(ma.study_hours ?? 0),
        AssignmentsCompleted = (int)(ma.assignments_completed ?? 0),
        QuizzesTaken = (int)(ma.quizzes_taken ?? 0),
        AverageScore = Math.Round((decimal)(ma.average_score ?? 0), 1)
      }).ToList();

      // If no real data, add current month with zeros
      if (!monthlyActivities.Any())
      {
        monthlyActivities.Add(new MonthlyActivity
        {
          Month = DateTime.Now.ToString("MM/yyyy"),
          StudyHours = 0,
          AssignmentsCompleted = 0,
          QuizzesTaken = 0,
          AverageScore = 0
        });
      }

      // Real subject scores from assignments grouped by course
      var subjectScoresSql = """
                SELECT 
                    c.name as subject,
                    AVG(asub.grade) as score,
                    COUNT(asub.id) as assignment_count,
                    CASE 
                        WHEN AVG(asub.grade) >= 85 THEN 'A'
                        WHEN AVG(asub.grade) >= 70 THEN 'B' 
                        WHEN AVG(asub.grade) >= 55 THEN 'C'
                        ELSE 'D'
                    END as grade
                FROM programs.table_student_programs sp
                LEFT JOIN programs.classes cls ON sp.program_id = cls.program_id
                LEFT JOIN programs.table_teaching_assign_courses tac ON cls.id = tac.class_id
                LEFT JOIN programs.table_courses c ON tac.course_id = c.id
                LEFT JOIN programs.table_assignments a ON tac.id = a.teaching_assign_course_id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id AND asub.student_id = sp.student_id
                WHERE sp.student_id = @studentId 
                AND asub.grade IS NOT NULL
                AND c.name IS NOT NULL
                GROUP BY c.name
                HAVING COUNT(asub.id) > 0
                ORDER BY AVG(asub.grade) DESC
                LIMIT 8
            """;

      var subjectScoresData = await connection.QueryAsync(subjectScoresSql, new { studentId });

      var colors = new[] { "#10B981", "#F59E0B", "#3B82F6", "#EF4444", "#8B5CF6", "#06B6D4", "#EC4899", "#F97316" };
      var subjectScores = subjectScoresData.Select((subject, index) => new SubjectScore
      {
        Subject = subject.subject?.ToString() ?? "Unknown Subject",
        Score = Math.Round((decimal)(subject.score ?? 0), 1),
        Grade = subject.grade?.ToString() ?? "N/A",
        Color = colors[index % colors.Length]
      }).ToList();

      // Real performance stats based on assignment data
      var totalAssignments = (int)(assignmentStats?.total_assignments ?? 0);
      var completedAssignments = (int)(assignmentStats?.completed_assignments ?? 0);
      var averageScore = (decimal)(assignmentStats?.average_score ?? 0);

      // Calculate attendance rate based on course progress and assignment completion
      var attendanceRate = totalAssignments > 0
          ? Math.Round((decimal)completedAssignments / totalAssignments * 100, 1)
          : 0;

      // Calculate study hours from monthly activities
      var totalStudyHours = monthlyActivities.Sum(ma => ma.StudyHours);

      // Calculate rank based on average score
      var rank = averageScore >= 90 ? "Top 5%" :
                averageScore >= 80 ? "Top 10%" :
                averageScore >= 70 ? "Top 20%" :
                averageScore >= 60 ? "Top 30%" :
                "Top 50%";

      var performanceStats = new StudentPerformanceStats
      {
        AverageScore = Math.Round(averageScore, 1),
        AttendanceRate = attendanceRate,
        TotalAssignments = totalAssignments,
        CompletedAssignments = completedAssignments,
        TotalQuizzes = monthlyActivities.Sum(ma => ma.QuizzesTaken),
        QuizAverageScore = averageScore, // Same as assignment average for now
        StudyHours = totalStudyHours,
        Rank = rank
      };

      var response = new GetStudentByIdResponse
      {
        Id = student.id.ToString(),
        Name = student.name ?? "Unknown Student",
        Email = student.email ?? "",
        PhoneNumber = student.phone_number ?? "",
        DateOfBirth = student.date_of_birth?.ToString("yyyy-MM-dd") ?? "",
        EnrollmentDate = student.enrollment_date?.ToString("yyyy-MM-dd") ?? "",
        Status = student.status == 1 ? "Active" : "Inactive",
        Department = student.department ?? "Unknown",
        Program = student.program ?? "Unknown",
        Avatar = student.avatar ?? "",
        Address = student.address ?? "",
        Gpa = gpa,
        TotalCourses = (int)(courseStats?.total_courses ?? 0),
        CompletedCourses = (int)(courseStats?.completed_courses ?? 0),
        InProgressCourses = (int)(courseStats?.in_progress_courses ?? 0),

        PerformanceStats = performanceStats,
        CourseProgresses = courseProgresses.Select(cp => new CourseProgress
        {
          CourseId = cp.course_id?.ToString() ?? "",
          CourseName = cp.course_name?.ToString() ?? "",
          CourseCode = cp.course_code?.ToString() ?? "",
          TeacherName = cp.teacher_name?.ToString() ?? "",
          ProgressPercentage = Convert.ToDecimal(cp.progress_percentage ?? 0),
          CurrentScore = Convert.ToDecimal(cp.current_score ?? 0),
          Status = cp.status?.ToString() ?? "",
          LastAccessed = cp.last_accessed?.ToString() ?? ""
        }).ToList(),
        StudyActivities = monthlyActivities,
        SubjectScores = subjectScores
      };

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error in GetStudentByIdQueryHandler: {ex}");
      return Result.Failure<GetStudentByIdResponse>(
          Error.Failure("GetStudentById.Error", ex.Message));
    }
  }
}
