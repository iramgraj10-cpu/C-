using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Mastery;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Learning.Analytics;

public sealed record SkillReportRow(
    string SkillId,
    string Title,
    string ModuleTitle,
    int Level,
    Tier Tier,
    ContentStatus Content,
    MasteryLevel Mastery,
    double? Accuracy,
    int Attempts,
    bool IndependentCode,
    DateTimeOffset? NextReview,
    bool NeedsReinforcement,
    IReadOnlyList<string> Pending);

/// <summary>
/// Panel de progreso que separa contenido visto, ejercicios completados, práctica y dominio demostrado.
/// Deliberadamente no ofrece un porcentaje único de "avance".
/// </summary>
public sealed record ProgressReport(
    int SkillsWithLessons,
    int LessonsSeen,
    int ExercisesCompleted,
    int ItemsAttempted,
    int SkillsPracticedOnly,
    int SkillsProvisional,
    int SkillsRetained,
    int SkillsPendingDemonstration,
    int SkillsIndependent,
    int ReviewsDue,
    TimeSpan PracticeTime,
    int SessionsCompleted,
    double HintsPerAttempt,
    double IndependentRate,
    IReadOnlyList<(ErrorCategory Category, int Count)> FrequentErrors,
    IReadOnlyList<SkillCalibration> Calibration,
    IReadOnlyList<SkillReportRow> Skills);

public sealed class ProgressReportBuilder(CurriculumCatalog catalog, MasteryEvaluator evaluator)
{
    public ProgressReport Build(LearnerSnapshot snapshot, DateTimeOffset now, TimeSpan practiceTime, int sessionsCompleted)
    {
        var rows = new List<SkillReportRow>();
        foreach (var module in catalog.Modules)
        {
            foreach (var skill in module.Units.SelectMany(u => u.Skills))
            {
                var progress = snapshot.Skill(skill.Id);
                var attempts = snapshot.AttemptsForSkill(skill.Id).ToList();
                var items = catalog.ItemsForSkill(skill.Id);
                var evaluation = evaluator.Evaluate(progress, attempts,
                    new SkillEvidenceProfile(items.Any(i => i.IsCode), items.Any(i => i.Purpose == Domain.Items.ItemPurpose.Transfer)), now);
                rows.Add(new SkillReportRow(
                    skill.Id, skill.Title, module.Title, module.Level, skill.Tier, skill.Status, progress.Level,
                    snapshot.SkillAccuracy(skill.Id), attempts.Count, evaluation.IndependentCodeSolutions > 0,
                    progress.Review.DueAt, progress.NeedsReinforcement, evaluation.Pending));
            }
        }

        var graded = snapshot.Attempts.Where(a => a.AutoGraded).ToList();
        return new ProgressReport(
            SkillsWithLessons: rows.Count(r => r.Content == ContentStatus.Complete),
            LessonsSeen: snapshot.Skills.Values.Count(p => p.SeenAt is not null),
            ExercisesCompleted: graded.Where(a => a.Correct).Select(a => a.ItemId).Distinct().Count(),
            ItemsAttempted: snapshot.Attempts.Select(a => a.ItemId).Distinct().Count(),
            SkillsPracticedOnly: rows.Count(r => r.Mastery == MasteryLevel.Practiced),
            SkillsProvisional: rows.Count(r => r.Mastery == MasteryLevel.ProvisionallyMastered),
            SkillsRetained: rows.Count(r => r.Mastery == MasteryLevel.Retained),
            SkillsPendingDemonstration: rows.Count(r => r.Content == ContentStatus.Complete && r.Mastery < MasteryLevel.ProvisionallyMastered),
            SkillsIndependent: rows.Count(r => r.IndependentCode),
            ReviewsDue: snapshot.Skills.Values.Count(p => p.Review.IsDue(now)),
            PracticeTime: practiceTime,
            SessionsCompleted: sessionsCompleted,
            HintsPerAttempt: graded.Count == 0 ? 0 : graded.Average(a => a.HintsUsed + (a.SolutionViewed ? 1 : 0)),
            IndependentRate: graded.Count == 0 ? 0 : graded.Count(a => a.Independent) / (double)graded.Count,
            FrequentErrors: ErrorPatternAnalyzer.MostFrequent(snapshot.Attempts),
            Calibration: new CalibrationAnalyzer().Analyze(snapshot.Attempts),
            Skills: rows);
    }
}
