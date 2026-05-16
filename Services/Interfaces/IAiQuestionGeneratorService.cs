using TinHocTHPT.Services;

namespace TinHocTHPT.Services.Interfaces;

public interface IAiQuestionGeneratorService
{
    Task<List<AiGeneratedQuestion>> GenerateAsync(AiQuestionPrompt prompt, CancellationToken cancellationToken = default);
}
