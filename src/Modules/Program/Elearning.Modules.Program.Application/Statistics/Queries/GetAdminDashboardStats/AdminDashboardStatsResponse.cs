namespace Elearning.Modules.Program.Application.Statistics.Queries.GetAdminDashboardStats;

public sealed class AdminDashboardStatsResponse
{
  public OverviewStats Overview { get; set; } = new();
  public List<EnrollmentTrend> EnrollmentTrends { get; set; } = [];
  public List<DepartmentStat> DepartmentStats { get; set; } = [];
  public PerformanceMetric PerformanceMetrics { get; set; } = new();
  public List<RecentActivity> RecentActivities { get; set; } = [];
}

public sealed class OverviewStats
{
  public int TotalStudents { get; set; }
  public int TotalTeachers { get; set; }
  public int TotalCourses { get; set; }
  public int ActiveCourses { get; set; }
  public decimal AverageGpa { get; set; }
  public decimal CompletionRate { get; set; }
}

public sealed class EnrollmentTrend
{
  public string Month { get; set; } = string.Empty;
  public int Students { get; set; }
  public int Courses { get; set; }
}

public sealed class DepartmentStat
{
  public string Department { get; set; } = string.Empty;
  public int Students { get; set; }
  public int Courses { get; set; }
  public decimal AverageGpa { get; set; }
}

public sealed class PerformanceMetric
{
  public int ExcellentStudents { get; set; } // GPA >= 8.5
  public int GoodStudents { get; set; } // GPA 7.0-8.4
  public int AverageStudents { get; set; } // GPA 5.5-6.9
  public int BelowAverageStudents { get; set; } // GPA < 5.5
}

public sealed class RecentActivity
{
  public string Id { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string Timestamp { get; set; } = string.Empty;
}
