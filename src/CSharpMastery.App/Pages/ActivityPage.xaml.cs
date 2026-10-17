using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.App.Views;
using CSharpMastery.Presentation.ViewModels.Activity;
using CSharpMastery.Presentation.ViewModels.Items;

namespace CSharpMastery.App.Pages;

public abstract class ActivityPageBase(ActivityViewModel vm) : PageBase<ActivityViewModel>(vm);

/// <summary>
/// Presenta el paso actual con la vista adecuada y anima los cambios (entrada del paso, franja de resultado, XP).
/// Lleva el reloj de la sesión, guarda borradores periódicamente y, en escritorio, ofrece atajos de teclado:
/// Ctrl+Enter comprueba o continúa, F5 ejecuta y Ctrl+H pide pista.
/// </summary>
public partial class ActivityPage : ActivityPageBase
{
    private IDispatcherTimer? _timer;
    private int _ticks;
    private StepViewModel? _observed;

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
        else if (e.PropertyName == nameof(ActivityViewModel.XpEarned) && ViewModel.XpEarned > 0)
        {
            Dispatcher.Dispatch(async () =>
            {
                await XpLabel.ScaleToAsync(1.5, 120, Easing.CubicOut);
                await XpLabel.ScaleToAsync(1, 160, Easing.CubicIn);
            });
        }
    }

    private void OnStepChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StepViewModel.Banner) && _observed?.HasBanner == true)
        {
            Dispatcher.Dispatch(AnimateBanner);
        }
    }

    private async void AnimateBanner()
    {
        BannerPanel.Opacity = 0;
        BannerPanel.TranslationY = 30;
        await Task.WhenAll(BannerPanel.FadeToAsync(1, 180), BannerPanel.TranslateToAsync(0, 0, 220, Easing.CubicOut));
        if (_observed?.IsIncorrectBanner == true)
        {
            // Pequeña sacudida del paso al fallar.
            for (var i = 0; i < 2; i++)
            {
                await StepHost.TranslateToAsync(-8, 0, 50);
                await StepHost.TranslateToAsync(8, 0, 50);
            }

            await StepHost.TranslateToAsync(0, 0, 40);
        }
    }

    private async void ShowStep()
    {
        if (_observed is not null)
        {
            _observed.PropertyChanged -= OnStepChanged;
        }

        _observed = ViewModel.CurrentStep;
        if (_observed is not null)
        {
            _observed.PropertyChanged += OnStepChanged;
        }

        View? view = ViewModel.CurrentStep switch
        {
            IntroStepViewModel => new IntroStepView(),
            ConceptCardViewModel => new ConceptCardView(),
            ClozeViewModel => new ClozeStepView(),
            ParsonsViewModel => new ParsonsStepView(),
            WalkthroughViewModel => new WalkthroughStepView(),
            SpotBugViewModel => new SpotBugStepView(),
            CompareCodeViewModel => new CompareCodeStepView(),
            ChoiceItemViewModel => new ChoiceItemView(),
            ShortAnswerItemViewModel => new ShortAnswerItemView(),
            CodeExerciseViewModel => new CodeExerciseView(),
            SummaryStepViewModel => new SummaryStepView(),
            _ => null,
        };

        if (view is not null)
        {
            view.BindingContext = ViewModel.CurrentStep;
            view.Opacity = 0;
            view.TranslationX = 40;
        }

        StepHost.Content = view;
        _ = Scroller.ScrollToAsync(0, 0, false);
        if (view is null)
        {
            return;
        }

        if (view is CodeExerciseView code && DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            code.FocusEditor();
        }

        await Task.WhenAll(view.FadeToAsync(1, 200), view.TranslateToAsync(0, 0, 240, Easing.CubicOut));
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
        menu.Add(Shortcut("Comprobar / Continuar", KeyboardAcceleratorModifiers.Ctrl, "Enter", () => Execute(ViewModel.CurrentStep?.PrimaryCommand)));
        menu.Add(Shortcut("Ejecutar", KeyboardAcceleratorModifiers.None, "F5", () => Execute((ViewModel.CurrentStep as CodeExerciseViewModel)?.RunCommand)));
        menu.Add(Shortcut("Pedir pista", KeyboardAcceleratorModifiers.Ctrl, "H", () => Execute((ViewModel.CurrentStep as ItemViewModel)?.RevealHintCommand)));
        MenuBarItems.Add(menu);
    }

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
