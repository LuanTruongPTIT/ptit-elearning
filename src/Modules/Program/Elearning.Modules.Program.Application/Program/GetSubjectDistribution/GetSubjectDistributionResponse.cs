namespace Elearning.Modules.Program.Application.Program.GetSubjectDistribution
{
    public class GetSubjectDistributionResponse
    {
        public List<SubjectDistributionDto> SubjectDistribution { get; set; }
    }

    public class SubjectDistributionDto
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Color { get; set; }
    }
}
