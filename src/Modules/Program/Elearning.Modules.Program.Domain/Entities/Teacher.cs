using System;

namespace Elearning.Modules.Program.Domain.Entities
{
    public class Teacher
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Department { get; private set; }
        public int CoursesCount { get; private set; }
        public int StudentsCount { get; private set; }
        public decimal Rating { get; private set; }

        private Teacher() { }

        public Teacher(string name, string email, string department)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Department = department;
            CoursesCount = 0;
            StudentsCount = 0;
            Rating = 0;
        }

        public void UpdateDetails(string name, string email, string department)
        {
            Name = name;
            Email = email;
            Department = department;
        }

        public void UpdateStatistics(int coursesCount, int studentsCount, decimal rating)
        {
            CoursesCount = coursesCount;
            StudentsCount = studentsCount;
            Rating = rating;
        }
    }
}
