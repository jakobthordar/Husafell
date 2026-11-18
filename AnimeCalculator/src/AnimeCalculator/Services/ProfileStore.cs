using System.Text.Json;
using AnimeCalculator.Core.Progress;
using Microsoft.JSInterop;

namespace AnimeCalculator.Services;

/// <summary>Saves the player's profile in the browser's localStorage.</summary>
public sealed class ProfileStore(IJSRuntime js)
{
    private const string StorageKey = "anime-calculator.profile.v1";

    public async Task<PlayerProfile> LoadAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("animeCalc.load", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var profile = JsonSerializer.Deserialize<PlayerProfile>(json);
                if (profile is not null) return profile.Normalize();
            }
        }
        catch (Exception ex) when (ex is JSException or JsonException)
        {
            // Private browsing, blocked storage or a corrupt save: just start fresh.
        }

        return new PlayerProfile();
    }

    public async Task SaveAsync(PlayerProfile profile)
    {
        try
        {
            await js.InvokeVoidAsync("animeCalc.save", StorageKey, JsonSerializer.Serialize(profile));
        }
        catch (JSException)
        {
            // Saving is a nice-to-have.
        }
    }

    public async Task ResetAsync()
    {
        try
        {
            await js.InvokeVoidAsync("animeCalc.remove", StorageKey);
        }
        catch (JSException)
        {
        }
    }
}
