namespace snowcoreBlog.Frontend.SharedComponents.Models;

// User-selectable theme preference for the public frontend. Dark is the default
// when nothing is persisted; Device follows the OS prefers-color-scheme and is
// kept live (FluentUI re-resolves it when the OS theme flips).
public enum ThemePreference
{
    Dark,
    Light,
    Device
}
