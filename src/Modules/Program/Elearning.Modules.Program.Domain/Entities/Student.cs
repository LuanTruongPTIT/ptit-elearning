using System;

namespace Elearning.Modules.Program.Domain.Entities
{
    public class Student
    {
        public Guid Id { get; private set; }
        public string StudentId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Department { get; private set; }
        public int EnrolledCourses { get; private set; }
        public decimal Progress { get; private set; }
        public decimal GPA { get; private set; }

        private Student() { }

        public Student(string studentId, string name, string email, string department)
        {
            Id = Guid.NewGuid();
            StudentId = studentId;
            Name = name;
            Email = email;
            Department = department;
            EnrolledCourses = 0;
            Progress = 0;
            GPA = 0;
        }

        public void UpdateDetails(string name, string email, string department)
        {
            Name = name;
            Email = email;
            Department = department;
        }

        public void UpdateProgress(int enrolledCourses, decimal progress, decimal gpa)
        {
            EnrolledCourses = enrolledCourses;
            Progress = progress;
            GPA = gpa;
        }
    }
}
