using System;
using System.Collections.Generic;

namespace Elearning.Modules.Users.Application.Students.GetStudents;

public class GetStudentsWithPaginationResponse
{
  public List<GetStudentsResponse> Students { get; set; } = new();
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int TotalPages { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}
