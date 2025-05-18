namespace Elearning.Modules.Program.Application.Program.GetWeeklyStudyData
{
    public class GetWeeklyStudyDataResponse
    {
        public List<StudyTimeDataDto> WeeklyStudyData { get; set; }
    }

    public class StudyTimeDataDto
    {
        public string Day { get; set; }
        public double Hours { get; set; }
    }
}
