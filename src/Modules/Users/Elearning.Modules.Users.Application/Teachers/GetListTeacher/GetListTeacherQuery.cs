using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Users.Application.Teachers.GetListTeacher;

public sealed class GetListTeacherQuery : IQuery<List<GetListTeacherResponse>>
{
  public GetListTeacherQuery() { }
  public string? keyword { get; set; }
  public int? page { get; set; }
  public int? page_size { get; set; }
  public GetListTeacherQuery(string? keyword, int? page, int? page_size)
  {
    this.keyword = keyword;
    this.page = page;
    this.page_size = page_size;
  }

}