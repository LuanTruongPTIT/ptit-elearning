namespace Elearning.Modules.Program.Application.Program.GetProgressData
{
  public class GetProgressDataResponse
  {
    public List<ProgressDataDto> ProgressData { get; set; }
  }

  public class ProgressDataDto
  {
    public DateTime Date { get; set; }
    public int Progress { get; set; }
  }
}
