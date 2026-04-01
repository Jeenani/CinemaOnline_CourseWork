using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdminDesktop.Models;

namespace AdminDesktop.Services;

public class AdminApiService
{
    private readonly HttpClient _http;
    private string? _token;

    public string? Token => _token;
    public UserInfo? CurrentUser { get; private set; }
    public bool IsAuthenticated => _token != null && CurrentUser?.Role == "admin";

    public event Action<string>? OnSessionExpired;

    public AdminApiService(string baseUrl = "http://localhost:5103")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(15) };
    }

    public async Task<(bool ok, string error)> LoginAsync(string email, string password)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
            if (!resp.IsSuccessStatusCode)
                return (false, $"Сервер вернул {(int)resp.StatusCode}");

            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            if (result?.Data == null)
                return (false, "Некорректный ответ сервера");

            if (result.Data.User.Role != "admin")
                return (false, "Доступ только для администраторов");

            _token = result.Data.Token;
            CurrentUser = result.Data.User;
            return (true, string.Empty);
        }
        catch (HttpRequestException)
        {
            return (false, "Не удалось подключиться к серверу");
        }
        catch (TaskCanceledException)
        {
            return (false, "Таймаут подключения");
        }
    }

    public void Logout()
    {
        _token = null;
        CurrentUser = null;
    }

    private HttpRequestMessage Auth(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        if (_token != null)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return req;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var req = Auth(HttpMethod.Get, url);
        var resp = await _http.SendAsync(req);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized) { HandleSessionExpired(); return default; }
        if (!resp.IsSuccessStatusCode) return default;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>();
        return result != null ? result.Data : default;
    }

    public async Task<bool> PostAsync(string url, object body)
    {
        var req = Auth(HttpMethod.Post, url);
        req.Content = JsonContent.Create(body);
        var resp = await _http.SendAsync(req);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized) { HandleSessionExpired(); return false; }
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> PutAsync(string url, object body)
    {
        var req = Auth(HttpMethod.Put, url);
        req.Content = JsonContent.Create(body);
        var resp = await _http.SendAsync(req);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized) { HandleSessionExpired(); return false; }
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string url)
    {
        var req = Auth(HttpMethod.Delete, url);
        var resp = await _http.SendAsync(req);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized) { HandleSessionExpired(); return false; }
        return resp.IsSuccessStatusCode;
    }

    private void HandleSessionExpired()
    {
        Logout();
        OnSessionExpired?.Invoke("Сессия истекла. Войдите заново.");
    }

    public async Task<string?> UploadAsync(string url, byte[] data, string fileName)
    {
        var req = Auth(HttpMethod.Post, url);
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(data), "file", fileName);
        req.Content = content;
        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return null;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<string>>();
        return result?.Data;
    }

    // === Shortcuts ===
    public Task<AdminStats?> GetStatsAsync() => GetAsync<AdminStats>("/api/admin/stats");
    public Task<List<AdminMovie>?> GetMoviesAsync() => GetAsync<List<AdminMovie>>("/api/admin/movies");
    public Task<List<AdminUser>?> GetUsersAsync() => GetAsync<List<AdminUser>>("/api/admin/users");
    public Task<List<GenreItem>?> GetGenresAsync() => GetAsync<List<GenreItem>>("/api/genres");
    public Task<List<AdminCollection>?> GetCollectionsAsync() => GetAsync<List<AdminCollection>>("/api/admin/collections");
    public Task<List<AdminComment>?> GetCommentsAsync() => GetAsync<List<AdminComment>>("/api/admin/comments");
}
