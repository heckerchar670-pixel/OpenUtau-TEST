using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using OpenUtau.Core.Util;
using OpenUtauMobile.Helpers;
using OpenUtauMobile.Services;
using OpenUtauMobile.Services.Projects;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime.Platform;
using OpenUtauMobile.ViewModels;
using OpenUtauMobile.Views;

namespace OpenUtauMobile;

public partial class App : Application
{
    internal static MainView? ActivityMainView { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        PreferencesWriter.Inst.Install();
        // Load language preference before constructing any UI/view-models.
        LocalizationManager.LoadLanguage(Preferences.Default.Language);

        RequestedThemeVariant = ParseThemePreference(Preferences.Default.ThemeName);

        ServiceHub.SystemAccentColorProvider ??= new DefaultSystemAccentColorProvider();

        // Fuente pixelada del tema UTAU (si el workflow no la descargo, queda la normal).
        Uri pixelFont = new("avares://OpenUtauMobile/Assets/Fonts/DotGothic16-Regular.ttf");
        if (LunaiThemes.IsRetro && AssetLoader.Exists(pixelFont))
        {
            Resources["ContentControlThemeFontFamily"] =
                new FontFamily(pixelFont.AbsoluteUri + "#DotGothic16");
        }

        // Lunai: estilos de pulido (solo se cargan con este tema, los demas no se tocan)
        if (LunaiThemes.Current == LunaiThemes.Lunai)
        {
            Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri("avares://OpenUtauMobile/"))
            {
                Source = new Uri("avares://OpenUtauMobile/Themes/OpenUtauMobile/Styles/LunaiPolish.axaml"),
            });
        }

        // Initialize runtime theme resources before creating UI.
        ThemeManagerV2.Initialize();
        ThemeManagerV2.ApplyConfiguredTheme(ServiceHub.SystemAccentColorProvider, out _, out _);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainViewModel main = new();
            desktop.MainWindow = ServiceHub.DesktopWindowFactory?.Invoke(main) ?? new MainWindow
            {
                DataContext = main
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activityPlatform)
        {
            activityPlatform.MainViewFactory = CreateActivityMainView;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();

        // Follow system light/dark changes with runtime-generated semantic theme.
        ActualThemeVariantChanged += (_, _) => ThemeManagerV2.OnThemeVariantChanged();

        IPlatformSettings? platformSettings = PlatformSettings;
        if (platformSettings is not null)
        {
            platformSettings.ColorValuesChanged += (_, _) => RefreshSystemThemeColor();
        }
    }

    private static MainView CreateActivityMainView()
    {
        MainView mainView = new MainView
        {
            DataContext = new MainViewModel()
        };
        ActivityMainView = mainView;
        return mainView;
    }

    internal static void RefreshSystemThemeColor()
    {
        if (Preferences.Default.ThemeColorMode != (int)ThemeColorMode.FollowSystem)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(RefreshSystemThemeColor);
            return;
        }

        ThemeManagerV2.ApplyConfiguredTheme(ServiceHub.SystemAccentColorProvider, out _, out _);
    }

    private static ThemeVariant ParseThemePreference(string? value) => value switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => ThemeVariant.Default,
    };
}
