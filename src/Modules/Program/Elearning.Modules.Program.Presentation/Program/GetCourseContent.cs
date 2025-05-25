using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetCourseContent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class GetCourseContent : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("student/courses/{courseId}/content", async (
        Guid courseId,
        [FromQuery] Guid? studentId,
        ISender sender,
        HttpContext httpContext) =>
    {
      // Lấy thông tin về vai trò của người dùng
      var userRoles = httpContext.User.GetUserRoles();
      var isAdmin = userRoles.Contains("Administrator");
      var isTeacher = userRoles.Contains("Teacher") || userRoles.Contains("Lecturer");
      var isStudent = userRoles.Contains("Student");

      Guid effectiveStudentId;

      // Xác định studentId hiệu lực
      if (studentId.HasValue)
      {
        // Nếu studentId được cung cấp, kiểm tra quyền truy cập
        if (isAdmin || isTeacher)
        {
          // Admin và Teacher có thể truy cập dữ liệu của bất kỳ học sinh nào
          effectiveStudentId = studentId.Value;
        }
        else if (isStudent)
        {
          // Student chỉ có thể truy cập dữ liệu của chính mình
          var currentUserId = Guid.Parse(httpContext.User.GetUserId());
          if (studentId.Value != currentUserId)
          {
            return Results.Forbid();
          }
          effectiveStudentId = studentId.Value;
        }
        else
        {
          // Vai trò không hợp lệ
          return Results.Forbid();
        }
      }
      else
      {
        // Nếu studentId không được cung cấp, sử dụng ID của người dùng hiện tại
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
          effectiveStudentId = Guid.Parse(httpContext.User.GetUserId());
        }
        else
        {
          // Người dùng chưa đăng nhập và không cung cấp studentId
          return Results.Unauthorized();
        }
      }

      var result = await sender.Send(new GetCourseContentQuery(courseId, effectiveStudentId));

      return result.Match(
          data => Results.Ok(new
          {
            status = StatusCodes.Status200OK,
            message = "Get course content successfully",
            data
          }),
          ApiResults.Problem
      );
    })
    .RequireAuthorization() // Yêu cầu người dùng đã đăng nhập
    .WithTags("Student");
  }
}
