using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetProgressData
{
    public class GetProgressDataQueryHandler : IRequestHandler<GetProgressDataQuery, GetProgressDataResponse>
    {
        public async Task<GetProgressDataResponse> Handle(GetProgressDataQuery request, CancellationToken cancellationToken)
        {
            // Mock data for progress over time
            // In a real application, this would come from a database
            var progressData = new List<ProgressDataDto>
            {
                new ProgressDataDto { Month = "Jan", Progress = 10 },
                new ProgressDataDto { Month = "Feb", Progress = 25 },
                new ProgressDataDto { Month = "Mar", Progress = 30 },
                new ProgressDataDto { Month = "Apr", Progress = 45 },
                new ProgressDataDto { Month = "May", Progress = 60 },
                new ProgressDataDto { Month = "Jun", Progress = 75 },
                new ProgressDataDto { Month = "Jul", Progress = 85 }
            };

            return new GetProgressDataResponse
            {
                ProgressData = progressData
            };
        }
    }
}
