using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetWeeklyStudyData
{
    public class GetWeeklyStudyDataQueryHandler : IRequestHandler<GetWeeklyStudyDataQuery, GetWeeklyStudyDataResponse>
    {
        public async Task<GetWeeklyStudyDataResponse> Handle(GetWeeklyStudyDataQuery request, CancellationToken cancellationToken)
        {
            // Mock data for weekly study time
            // In a real application, this would come from a database
            var weeklyStudyData = new List<StudyTimeDataDto>
            {
                new StudyTimeDataDto { Day = "Mon", Hours = 2.5 },
                new StudyTimeDataDto { Day = "Tue", Hours = 1.8 },
                new StudyTimeDataDto { Day = "Wed", Hours = 3.2 },
                new StudyTimeDataDto { Day = "Thu", Hours = 2.0 },
                new StudyTimeDataDto { Day = "Fri", Hours = 1.5 },
                new StudyTimeDataDto { Day = "Sat", Hours = 4.0 },
                new StudyTimeDataDto { Day = "Sun", Hours = 3.5 }
            };

            return new GetWeeklyStudyDataResponse
            {
                WeeklyStudyData = weeklyStudyData
            };
        }
    }
}
