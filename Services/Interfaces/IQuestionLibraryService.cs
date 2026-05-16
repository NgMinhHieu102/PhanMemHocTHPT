using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Services.Interfaces;

public interface IQuestionLibraryService
{
    Task<List<Question>> GetRandomQuestionsAsync(int count, int? topicId = null, string? difficulty = null);
    Task<(Exam exam, int questionCount)> CreateExamFromLibraryAsync(
        string title, int grade, int totalQuestions, int? topicId = null, string? difficulty = null);
}