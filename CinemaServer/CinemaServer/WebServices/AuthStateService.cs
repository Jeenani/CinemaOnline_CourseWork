using Microsoft.JSInterop;
using CinemaServer.DTOs;

namespace CinemaServer.WebServices;

/// <summary>
/// Сервис для хранения состояния авторизации пользователя
/// Использует localStorage браузера для сохранения токена между сессиями
/// </summary>
public class AuthStateService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly CinemaApiService _apiService;
    private UserResponse? _currentUser;
    private string? _token;
    private bool _isInitialized;

    public event Action? OnAuthStateChanged;

    public AuthStateService(IJSRuntime jsRuntime, CinemaApiService apiService)
    {
        _jsRuntime = jsRuntime;
        _apiService = apiService;
    }

    public UserResponse? CurrentUser => _currentUser;
    public string? Token => _token;
    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_token);

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Если токен уже в памяти (например, после UpdateUserAsync) — используем его
            _token ??= await GetFromLocalStorageAsync("auth_token");

            if (!string.IsNullOrEmpty(_token))
            {
                // Всегда подтягиваем свежие данные с сервера
                var user = await _apiService.GetCurrentUserAsync(_token);
                if (user != null)
                {
                    _currentUser = user;
                    await SetToLocalStorageAsync("auth_user", System.Text.Json.JsonSerializer.Serialize(user));
                }
                else
                {
                    // Токен невалиден — чистим
                    _token = null;
                    _currentUser = null;
                    await RemoveFromLocalStorageAsync("auth_token");
                    await RemoveFromLocalStorageAsync("auth_user");
                }
            }
        }
        catch
        {
            // При ошибке сети — если уже есть данные в памяти, оставляем их
            if (_currentUser == null)
            {
                try
                {
                    var userJson = await GetFromLocalStorageAsync("auth_user");
                    if (!string.IsNullOrEmpty(userJson))
                    {
                        _currentUser = System.Text.Json.JsonSerializer.Deserialize<UserResponse>(userJson);
                    }
                }
                catch { }
            }
        }

        _isInitialized = true;
    }

    public async Task LoginAsync(UserResponse user, string token)
    {
        _currentUser = user;
        _token = token;

        try
        {
            await SetToLocalStorageAsync("auth_token", token);
            await SetToLocalStorageAsync("auth_user", System.Text.Json.JsonSerializer.Serialize(user));
        }
        catch { }

        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _token = null;

        try
        {
            await RemoveFromLocalStorageAsync("auth_token");
            await RemoveFromLocalStorageAsync("auth_user");
        }
        catch { }

        NotifyStateChanged();
    }

    public async Task UpdateUserAsync(UserResponse user)
    {
        _currentUser = user;

        try
        {
            await SetToLocalStorageAsync("auth_user", System.Text.Json.JsonSerializer.Serialize(user));
        }
        catch { }

        // Сбрасываем флаг инициализации — при следующем InitializeAsync()
        // данные будут заново проверены с сервера (а не из кэша)
        _isInitialized = false;

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnAuthStateChanged?.Invoke();

    private async Task<string?> GetFromLocalStorageAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    private async Task SetToLocalStorageAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    private async Task RemoveFromLocalStorageAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
