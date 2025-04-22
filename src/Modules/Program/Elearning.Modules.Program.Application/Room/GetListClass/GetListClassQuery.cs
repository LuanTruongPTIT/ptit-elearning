using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Room.GetListClass;

public sealed class GetListClassQuery : IQuery<List<GetListClassResponse>>
{
    public int? page { get; set; }
    public int? page_size { get; set; }
    
    public GetListClassQuery(int? page, int? page_size)
    {
        this.page = page;
        this.page_size = page_size;
    }
}