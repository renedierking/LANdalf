using Microsoft.JSInterop;
using MudBlazor;

namespace LANdalf.UI.Services;

public class ThemeService {
    private bool isDarkMode;
    private MudTheme currentTheme = null!;

    public event Action? OnThemeChanged;

    public ThemeService() {
        isDarkMode = false;
        InitializeTheme();
    }

    private void InitializeTheme() {
        currentTheme = isDarkMode ? CreateDarkTheme() : CreateLightTheme();
    }

    public MudTheme GetCurrentTheme() => currentTheme;

    public bool IsDarkMode => isDarkMode;

    public async Task LoadThemeFromStorage(IJSRuntime jsRuntime) {
        try {
            var savedTheme = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme-preference");
            isDarkMode = savedTheme == "dark";
            InitializeTheme();
            OnThemeChanged?.Invoke();
        } catch {
            // Fallback bei localStorage-Fehler
            isDarkMode = false;
            InitializeTheme();
        }
    }

    public async Task ToggleTheme(IJSRuntime jsRuntime) {
        isDarkMode = !isDarkMode;
        InitializeTheme();
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme-preference", isDarkMode ? "dark" : "light");
        OnThemeChanged?.Invoke();
    }

    private static MudTheme CreateLightTheme() {
        return new MudTheme {
            PaletteLight = new PaletteLight {
                Primary = "#1976d2",
                Secondary = "#424242",
                Success = "#33d9b2",
                Info = "#0288d1",
                Warning = "#f57c00",
                Error = "#e53935",
                Dark = "#121212",
                TextPrimary = "#000000",
                TextSecondary = "rgba(0,0,0,0.6)",
                AppbarBackground = "#1976d2",
                DrawerBackground = "#ffffff",
                Background = "#fafafa",
                BackgroundGray = "#eeeeee",
                Surface = "#ffffff",
                DrawerText = "rgba(0,0,0,0.7)",
                ActionDefault = "#ababab",
                ActionDisabled = "rgba(0,0,0,0.26)",
                ActionDisabledBackground = "rgba(0,0,0,0.12)",
                Divider = "rgba(0,0,0,0.12)",
                DividerLight = "rgba(0,0,0,0.06)",
                TableLines = "rgba(0,0,0,0.12)",
                TableStriped = "rgba(0,0,0,0.02)",
                TableHover = "rgba(0,0,0,0.04)",
                LinesDefault = "rgba(0,0,0,0.12)",
                LinesInputs = "rgba(0,0,0,0.23)",
                TextDisabled = "rgba(0,0,0,0.38)",
                OverlayLight = "rgba(255,255,255,0.5)",
                OverlayDark = "rgba(0,0,0,0.25)"
            }
        };
    }

    private static MudTheme CreateDarkTheme() {
        return new MudTheme {
            PaletteDark = new PaletteDark {
                Primary = "#bb86fc",
                Secondary = "#03dac6",
                Success = "#33d9b2",
                Info = "#0288d1",
                Warning = "#ffb300",
                Error = "#cf6679",
                Dark = "#121212",
                TextPrimary = "#ffffff",
                TextSecondary = "rgba(255,255,255,0.7)",
                AppbarBackground = "#1f1f1f",
                DrawerBackground = "#121212",
                Background = "#121212",
                BackgroundGray = "#1f1f1f",
                Surface = "#1f1f1f",
                DrawerText = "rgba(255,255,255,0.7)",
                ActionDefault = "#ababab",
                ActionDisabled = "rgba(255,255,255,0.26)",
                ActionDisabledBackground = "rgba(255,255,255,0.12)",
                Divider = "rgba(255,255,255,0.12)",
                DividerLight = "rgba(255,255,255,0.06)",
                TableLines = "rgba(255,255,255,0.12)",
                TableStriped = "rgba(255,255,255,0.02)",
                TableHover = "rgba(255,255,255,0.04)",
                LinesDefault = "rgba(255,255,255,0.12)",
                LinesInputs = "rgba(255,255,255,0.23)",
                TextDisabled = "rgba(255,255,255,0.38)",
                OverlayLight = "rgba(0,0,0,0.5)",
                OverlayDark = "rgba(255,255,255,0.25)"
            }
        };
    }
}
