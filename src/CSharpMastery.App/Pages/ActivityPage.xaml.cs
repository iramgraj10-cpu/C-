using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.App.Views;
using CSharpMastery.Presentation.ViewModels.Activity;
using CSharpMastery.Presentation.ViewModels.Items;

namespace CSharpMastery.App.Pages;

public abstract class ActivityPageBase(ActivityViewModel vm) : PageBase<ActivityViewModel>(vm);

/// <summary>
/// Presenta el paso actual con la vista adecuada. Lleva el reloj de la sesión, guarda borradores periódicamente y,
/// en escritorio, ofrece atajos de teclado: Ctrl+Enter comprueba, F5 ejecuta, Ctrl+H pide pista, Ctrl+→ continúa.
/// </summary>
public partial class ActivityPage : ActivityPageBase
{
    private IDispatcherTimer? _timer;
    private int _ticks;

    public ActivityPage(ActivityViewModel vm) : base(vm)
    {
        InitializeComponent();
        vm.PropertyChanged += OnViewModelChanged;
        BuildShortcuts();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActivityViewModel.CurrentStep))
        {
            Dispatcher.Dispatch(ShowStep);
        }
    }

    private void ShowStep()
    {
        View? view = ViewModel.CurrentStep switch
        {
            ReadingStepViewModel => new ReadingStepView(),
            ChoiceItemViewModel => new ChoiceItemView(),
            ShortAnswerItemViewModel => new ShortAnswerItemView(),
            CodeExerciseViewModel => new CodeExerciseView(),
            SummaryStepViewModel => new SummaryStepView(),
            _ => null,
        };

        if (view is not null)
        {
            view.BindingContext = ViewModel.CurrentStep;
        }

        StepHost.Content = view;
        _ = Scroller.ScrollToAsync(0, 0, false);
        if (view is CodeExerciseView code && DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            code.FocusEditor();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += async (_, _) =>
        {
            ViewModel.Tick();
            if (++_ticks % 15 == 0)
            {
                await ViewModel.SaveCurrentDraftAsync();
            }
        };
        _timer.Start();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        await ViewModel.SaveCurrentDraftAsync();
    }

    private void BuildShortcuts()
    {
        var menu = new MenuBarItem { Text = "Ejercicio" };
        menu.Add(Shortcut("Comprobar", KeyboardAcceleratorModifiers.Ctrl, "Enter", () => Execute(Step switch
        {
            CodeExerciseViewModel c => c.CheckCommand,
            ChoiceItemViewModel c => c.SubmitCommand,
            ShortAnswerItemViewModel s => s.SubmitCommand,
            _ => null,
        })));
        menu.Add(Shortcut("Ejecutar", KeyboardAcceleratorModifiers.None, "F5", () => Execute((Step as CodeExerciseViewModel)?.RunCommand)));
        menu.Add(Shortcut("Pedir pista", KeyboardAcceleratorModifiers.Ctrl, "H", () => Execute((Step as ItemViewModel)?.RevealHintCommand)));
        menu.Add(Shortcut("Continuar", KeyboardAcceleratorModifiers.Ctrl, "Right", () => Execute(Step switch
        {
            ItemViewModel i => i.ContinueCommand,
            ReadingStepViewModel r => r.ContinueCommand,
            SummaryStepViewModel s => s.FinishCommand,
            _ => null,
        })));
        MenuBarItems.Add(menu);
    }

    private object? Step => ViewModel.CurrentStep;

    private static MenuFlyoutItem Shortcut(string text, KeyboardAcceleratorModifiers modifiers, string key, Action action)
    {
        var item = new MenuFlyoutItem { Text = text, Command = new RelayCommand(action) };
        item.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = modifiers, Key = key });
        return item;
    }

    private static void Execute(System.Windows.Input.ICommand? command)
    {
        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
        }
    }
}
