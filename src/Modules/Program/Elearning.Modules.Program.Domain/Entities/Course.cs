using System;

namespace Elearning.Modules.Program.Domain.Entities
{
  public class Course
  {
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Department { get; private set; }
    public Guid TeacherId { get; private set; }
    public string TeacherName { get; private set; }
    public int StudentsCount { get; private set; }
    public decimal Progress { get; private set; }
    public CourseStatus Status { get; private set; }
    public decimal Rating { get; private set; }

    private Course() { }

    public Course(string code, string name, string department, Guid teacherId, string teacherName)
    {
      Id = Guid.NewGuid();
      Code = code;
      Name = name;
      Department = department;
      TeacherId = teacherId;
      TeacherName = teacherName;
      StudentsCount = 0;
      Progress = 0;
      Status = CourseStatus.Pending;
      Rating = 0;
    }

    public void UpdateDetails(string name, string department, Guid teacherId, string teacherName)
    {
      Name = name;
      Department = department;
      TeacherId = teacherId;
      TeacherName = teacherName;
    }

    public void UpdateStatus(CourseStatus status)
    {
      Status = status;
    }

    public void UpdateStatistics(int studentsCount, decimal progress, decimal rating)
    {
      StudentsCount = studentsCount;
      Progress = progress;
      Rating = rating;
    }
  }

  public enum CourseStatus
  {
    Pending,
    Active,
    Completed,
    Cancelled
  }
}
