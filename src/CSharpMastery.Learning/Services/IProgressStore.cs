using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;

namespace CSharpMastery.Learning.Services;

/// <summary>Estado de un proyecto integrador.</summary>
public sealed record ProjectProgress(
    string ProjectId,
    IReadOnlyDictionary<string, int> RubricSelfAssessment,
    string Notes,
    bool Completed,
    DateTimeOffset? CompletedAt);

public sealed record StoredAssessment(Assessment Assessment, AssessmentResult? Result);

/// <summary>Copia completa del progreso para exportar y restaurar.</summary>
public sealed record ProgressExport(
    int FormatVersion,
    DateTimeOffset ExportedAt,
    LearnerProfile? Profile,
    IReadOnlyList<SkillProgress> Skills,
    IReadOnlyList<AttemptRecord> Attempts,
    IReadOnlyList<StudySession> Sessions,
    IReadOnlyList<StoredAssessment> Assessments,
    IReadOnlyList<ProjectProgress> Projects,
    IReadOnlyDictionary<string, string> Settings)
{
    public const int CurrentFormatVersion = 1;
}

/// <summary>Persistencia local del progreso.</summary>
public interface IProgressStore
{
    Task<LearnerProfile?> GetProfileAsync();
    Task SaveProfileAsync(LearnerProfile profile);

    Task<IReadOnlyDictionary<string, SkillProgress>> GetSkillsAsync();
    Task<SkillProgress> GetSkillAsync(string skillId);
    Task SaveSkillAsync(SkillProgress progress);

    Task AddAttemptAsync(AttemptRecord attempt);
    Task<IReadOnlyList<AttemptRecord>> GetAttemptsAsync(string? skillId = null, string? contextId = null);

    Task SaveSessionAsync(StudySession session);
    Task<StudySession?> GetUnfinishedSessionAsync();
    Task<IReadOnlyList<StudySession>> GetSessionsAsync();

    Task SaveAssessmentAsync(Assessment assessment, AssessmentResult? result);
    Task<StoredAssessment?> GetAssessmentAsync(Guid id);
    Task<IReadOnlyList<StoredAssessment>> GetAssessmentsAsync();

    Task SaveProjectAsync(ProjectProgress project);
    Task<IReadOnlyList<ProjectProgress>> GetProjectsAsync();

    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value);

    Task AddSubmissionAsync(CodeSubmission submission);
    Task<IReadOnlyList<CodeSubmission>> GetSubmissionsAsync(string itemId);

    Task<string?> GetDraftAsync(string itemId);
    Task SaveDraftAsync(string itemId, string text);

    Task<ProgressExport> ExportAsync(DateTimeOffset now);
    Task ImportAsync(ProgressExport export);
}
