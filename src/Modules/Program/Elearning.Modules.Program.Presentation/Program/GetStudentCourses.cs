using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetStudentCourses;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class GetStudentCourses : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("student/courses", async (
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

      // Xác định studentId dựa trên vai trò
      if (studentId.HasValue)
      {
        // Nếu studentId được cung cấp trong query, sử dụng nó (chỉ cho admin và teacher)
        if (isAdmin || isTeacher)
        {
          effectiveStudentId = studentId.Value;
        }
        else if (isStudent)
        {
          // Sinh viên chỉ có thể xem khóa học của chính mình
          var currentUserId = Guid.Parse(httpContext.User.GetUserId());
          if (studentId.Value != currentUserId)
          {
            return Results.Forbid(); // Trả về lỗi 403 nếu sinh viên cố gắng xem khóa học của người khác
          }
          effectiveStudentId = currentUserId;
        }
        else
        {
          // Người dùng không có quyền xem
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

      var result = await sender.Send(new GetStudentCoursesQuery(effectiveStudentId));

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Get student courses successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization() // Yêu cầu người dùng đã đăng nhập
    .WithTags(Tags.Program);
  }
}
