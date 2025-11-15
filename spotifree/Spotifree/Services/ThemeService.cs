using MaterialDesignThemes.Wpf;
using Spotifree.IServices;

namespace Spotifree.Services;
#pragma warning disable CA1416

public class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private readonly PaletteHelper _paletteHelper = new();

    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async void ApplyThemeOnStart()
    {
        var settings = await _settingsService.GetAsync();
        SetTheme(settings.IsDarkTheme);
    }

    public void SetTheme(bool isDark)
    {
        var baseTheme = isDark ? BaseTheme.Dark : BaseTheme.Light;

        Theme theme = _paletteHelper.GetTheme();

        theme.SetBaseTheme(baseTheme);
        _paletteHelper.SetTheme(theme);
    }
}
#pragma warning restore CA1416