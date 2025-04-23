using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetCourseMaterials;

public sealed record GetCourseMaterialsQuery(
    Guid course_id
) : IQuery<List<GetCourseMaterialsResponse>>;
