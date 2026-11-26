using Microsoft.FluentUI.AspNetCore.Components;
using snowcoreBlog.Frontend.SharedComponents.Models;

namespace snowcoreBlog.Frontend.SharedComponents.Services;

// Single source of truth for the public frontend theme preference.
//
// Persistence is fully owned by the FluentDesignTheme component
// (StorageName="theme"): its fluent-design-theme web component reads and writes
// the localStorage entry consumed by <loading-theme storage-name="theme" />
// (JSON { mode, primaryColor, neutralColor }) and applies the stored mode over
// our rendered attribute before it initializes, so we never clobber a saved
// preference. Every applied change (initial load, user switch, OS theme flip
// while on Device) is reported back through SynchronizeFromDesignTheme.
//
// Switching a preference here only maps it onto DesignThemeModes
// (Device -> System) and raises OnThemeChanged; the layout re-renders
// FluentDesignTheme, which updates the mode attribute, applies luminance,
// resolves body[data-theme] and writes localStorage.
public class ThemeService
{
    private ThemePreference _currentPreference = ThemePreference.Dark;
    private DesignThemeModes _currentMode = DesignThemeModes.Dark;

    public event Action? OnThemeChanged;

    public ThemePreference CurrentPreference => _currentPreference;

    public DesignThemeModes CurrentMode => _currentMode;

    public void SetPreference(ThemePreference preference)
    {
        if (_currentPreference == preference)
        {
            return;
        }

        _currentPreference = preference;
        _currentMode = preference switch
        {
            ThemePreference.Light => DesignThemeModes.Light,
            ThemePreference.Device => DesignThemeModes.System,
            _ => DesignThemeModes.Dark
        };

        OnThemeChanged?.Invoke();
    }

    // Adopts a mode reported by FluentDesignTheme (JS-invoked OnChangeRaisedAsync).
    // System-dark/system-light values arrive as System; unresolved values are
    // normalized by FluentUI itself before they reach ModeChanged.
    public void SynchronizeFromDesignTheme(DesignThemeModes mode)
    {
        var preference = mode switch
        {
            DesignThemeModes.Light => ThemePreference.Light,
            DesignThemeModes.System => ThemePreference.Device,
            _ => ThemePreference.Dark
        };

        if (_currentPreference == preference)
        {
            return;
        }

        _currentPreference = preference;
        _currentMode = mode;
        OnThemeChanged?.Invoke();
    }
}
