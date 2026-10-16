using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.Presentation.ViewModels.Pages;

public sealed record ProjectLine(string Id, string Title, string Description, string Status, bool Completed);

public sealed partial class ProjectsViewModel(LearningService learning, INavigationService navigation) : ViewModelBase
{
    public ObservableCollection<ProjectLine> Projects { get; } = [];

    public bool IsEmpty => Projects.Count == 0;

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            Projects.Clear();
            foreach (var project in learning.Catalog.Projects)
            {
                var status = await learning.GetProjectStatusAsync(project.Id);
                var passed = status.Milestones.Count(m => m.Passed);
                Projects.Add(new ProjectLine(project.Id, project.Title, project.Description,
                    status.Progress.Completed ? "Completado" : $"{passed} de {status.Milestones.Count} hitos superados", status.Progress.Completed));
            }

            OnPropertyChanged(nameof(IsEmpty));
        }, "No se pudieron cargar los proyectos.");
    }

    [RelayCommand]
    private Task OpenAsync(ProjectLine line) =>
        navigation.GoToAsync(Routes.ProjectDetail, new Dictionary<string, object> { [Routes.ProjectParameter] = line.Id });
}

public sealed record MilestoneLine(string Title, string Description, string ExerciseId, bool Passed)
{
    public string StatusText => Passed ? "Pruebas superadas" : "Pendiente";
}

public sealed partial class RubricLine(string name, string insufficient, string adequate, string excellent, int value) : ObservableObject
{
    public string Name { get; } = name;

    public IReadOnlyList<string> Levels { get; } = [$"Insuficiente: {insufficient}", $"Adecuado: {adequate}", $"Excelente: {excellent}"];

    [ObservableProperty]
    public partial int SelectedIndex { get; set; } = value;
}

/// <summary>Proyecto integrador: requisitos, criterios de aceptación, hitos con pruebas automáticas y autoevaluación con rúbrica.</summary>
public sealed partial class ProjectDetailViewModel(LearningService learning, INavigationService navigation) : ViewModelBase, INavigationAware
{
    private string? _projectId;

    [ObservableProperty]
    public partial string Title { get; set; } = "";

    [ObservableProperty]
    public partial string Description { get; set; } = "";

    [ObservableProperty]
    public partial string Notes { get; set; } = "";

    [ObservableProperty]
    public partial string? StatusText { get; set; }

    public ObservableCollection<string> Requirements { get; } = [];

    public ObservableCollection<string> AcceptanceCriteria { get; } = [];

    public ObservableCollection<MilestoneLine> Milestones { get; } = [];

    public ObservableCollection<RubricLine> Rubric { get; } = [];

    public async Task OnNavigatedToAsync(IReadOnlyDictionary<string, object> parameters)
    {
        if (parameters.TryGetValue(Routes.ProjectParameter, out var value) && value is string id)
        {
            await LoadAsync(id);
        }
    }

    public async Task LoadAsync(string projectId)
    {
        _projectId = projectId;
        await RunSafeAsync(async () =>
        {
            var status = await learning.GetProjectStatusAsync(projectId);
            var p = status.Project;
            Title = p.Title;
            Description = p.Description;
            Notes = status.Progress.Notes;
            Requirements.Clear();
            foreach (var r in p.Requirements)
            {
                Requirements.Add(r);
            }

            AcceptanceCriteria.Clear();
            foreach (var c in p.AcceptanceCriteria)
            {
                AcceptanceCriteria.Add(c);
            }

            Milestones.Clear();
            foreach (var (m, passed) in status.Milestones)
            {
                Milestones.Add(new MilestoneLine(m.Title, m.Description, m.ExerciseId, passed));
            }

            Rubric.Clear();
            foreach (var c in p.Rubric)
            {
                Rubric.Add(new RubricLine(c.Name, c.Insufficient, c.Adequate, c.Excellent,
                    status.Progress.RubricSelfAssessment.TryGetValue(c.Name, out var v) ? v : -1));
            }

            StatusText = Describe(status);
        }, "No se pudo abrir el proyecto.");
    }

    private static string Describe(ProjectStatus status) =>
        status.Progress.Completed ? "Proyecto completado."
        : !status.AllMilestonesPassed ? "Supera las pruebas de todos los hitos y luego autoevalúate con la rúbrica."
        : "Todos los hitos pasan. Completa la rúbrica (ningún criterio insuficiente) para cerrar el proyecto.";

    [RelayCommand]
    private Task WorkOnMilestoneAsync(MilestoneLine line) =>
        navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Practice, [line.ExerciseId], Title: $"{Title}: {line.Title}")));

    [RelayCommand]
    private async Task SaveReviewAsync()
    {
        if (_projectId is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            var rubric = Rubric.Where(r => r.SelectedIndex >= 0).ToDictionary(r => r.Name, r => r.SelectedIndex);
            var status = await learning.SaveProjectReviewAsync(_projectId, rubric, Notes);
            StatusText = Describe(status);
        }, "No se pudo guardar la autoevaluación.");
    }
}
