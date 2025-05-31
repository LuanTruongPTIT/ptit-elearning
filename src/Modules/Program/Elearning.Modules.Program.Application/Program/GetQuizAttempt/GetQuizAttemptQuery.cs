using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetQuizAttempt;

public sealed record GetQuizAttemptQuery(Guid attempt_id) : IQuery<GetQuizAttemptResponse>;

public sealed record GetQuizAttemptResponse(
    Guid attempt_id,
    Guid quiz_id,
    Guid student_id,
    int attempt_number,
    string status,
    DateTime started_at,
    DateTime? submitted_at,
    int? time_taken_seconds,
    decimal total_score,
    decimal max_possible_score,
    decimal percentage_score,
    bool? passed,
    int? remaining_time_seconds,
    List<QuizAttemptResponseDetail> responses
);

public sealed record QuizAttemptResponseDetail(
    Guid response_id,
    Guid question_id,
    string question_text,
    string question_type,
    decimal points_possible,
    string selected_answer_ids,
    string text_response,
    bool is_correct,
    decimal points_earned,
    int? time_spent_seconds,
    DateTime? answered_at
);
