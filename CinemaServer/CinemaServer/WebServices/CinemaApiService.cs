using System.Net.Http.Json;
using CinemaServer.DTOs;

namespace CinemaServer.WebServices;

public class CinemaApiService
{
    private readonly HttpClient _httpClient;

    public CinemaApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5103";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    // Movies
    public async Task<PagedResponse<MovieResponse>?> GetMoviesAsync(MovieSearchRequest? request = null)
    {
        var query = request == null ? "" : BuildQuery(request);
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<MovieResponse>>>($"/api/movies{query}");
        return response?.Data;
    }

    public async Task<MovieDetailResponse?> GetMovieByIdAsync(long id, string? token = null)
    {
        if (!string.IsNullOrEmpty(token))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/movies/{id}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var resp = await _httpClient.SendAsync(request);
            if (!resp.IsSuccessStatusCode) return null;
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<MovieDetailResponse>>();
            return result?.Data;
        }
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<MovieDetailResponse>>($"/api/movies/{id}");
        return response?.Data;
    }

    public async Task<List<string>?> GetCountriesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<string>>>("/api/movies/countries");
        return response?.Data;
    }

    // Genres
    public async Task<List<GenreResponse>?> GetGenresAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<GenreResponse>>>("/api/genres");
        return response?.Data;
    }

    // Collections
    public async Task<List<CollectionResponse>?> GetFeaturedCollectionsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CollectionResponse>>>("/api/collections/featured");
        return response?.Data;
    }

    // Subscriptions
    public async Task<List<SubscriptionPlanResponse>?> GetSubscriptionPlansAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SubscriptionPlanResponse>>>("/api/subscriptions");
        return response?.Data;
    }

    public async Task<bool> ActivateSubscriptionAsync(long subscriptionId, string paymentMethod, string token)
    {
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/payments");
        createReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        createReq.Content = JsonContent.Create(new { SubscriptionId = subscriptionId, PaymentMethod = paymentMethod });
        var createResp = await _httpClient.SendAsync(createReq);
        if (!createResp.IsSuccessStatusCode) return false;

        var paymentResult = await createResp.Content.ReadFromJsonAsync<ApiResponse<PaymentResponse>>();
        if (paymentResult?.Data == null) return false;

        var processReq = new HttpRequestMessage(HttpMethod.Post, $"/api/payments/{paymentResult.Data.Id}/process");
        processReq.Content = JsonContent.Create(new { Success = true });
        var processResp = await _httpClient.SendAsync(processReq);
        return processResp.IsSuccessStatusCode;
    }

    // Auth
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            return result?.Data;
        }
        return null;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            return result?.Data;
        }
        return null;
    }

    // Comments & Ratings
    public async Task<bool> PostCommentAsync(long movieId, string content, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/movies/{movieId}/comments");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { Content = content });
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RateMovieAsync(long movieId, int rating, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/movies/{movieId}/rate");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { Rating = rating });
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    // Random movie for banner
    public async Task<MovieResponse?> GetRandomMovieAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<MovieResponse>>("/api/movies/random");
        return response?.Data;
    }

    // User
    public async Task<UserResponse?> GetCurrentUserAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        return result?.Data;
    }

    public async Task<List<MovieResponse>?> GetUserFavoritesAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/favorites");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<MovieResponse>>>();
        return result?.Data;
    }

    // User Settings
    public async Task<(bool ok, string? error)> UpdateProfileAsync(string name, string email, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/user/profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { Name = name, Email = email });
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode) return (true, null);
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, err?.Message ?? "Ошибка обновления профиля");
    }

    public async Task<(bool ok, string? error)> ChangePasswordAsync(string currentPassword, string newPassword, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/user/password");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { CurrentPassword = currentPassword, NewPassword = newPassword });
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode) return (true, null);
        var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, err?.Message ?? "Ошибка смены пароля");
    }

    private static string BuildQuery(MovieSearchRequest request)
    {
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(request.Search))
            queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");
        if (request.GenreId.HasValue)
            queryParams.Add($"genreId={request.GenreId}");
        if (request.YearFrom.HasValue)
            queryParams.Add($"yearFrom={request.YearFrom}");
        if (request.YearTo.HasValue)
            queryParams.Add($"yearTo={request.YearTo}");
        if (!string.IsNullOrEmpty(request.Country))
            queryParams.Add($"country={Uri.EscapeDataString(request.Country)}");
        if (request.MinRating.HasValue)
            queryParams.Add($"minRating={request.MinRating}");
        if (!string.IsNullOrEmpty(request.SortBy))
            queryParams.Add($"sortBy={request.SortBy}");
        queryParams.Add($"sortDescending={request.SortDescending}");
        queryParams.Add($"page={request.Page}");
        queryParams.Add($"pageSize={request.PageSize}");

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    }

    // ==================== ADMIN ====================

    private HttpRequestMessage AuthRequest(HttpMethod method, string url, string token)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    public async Task<T?> AdminGetAsync<T>(string url, string token)
    {
        var req = AuthRequest(HttpMethod.Get, url, token);
        var resp = await _httpClient.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return default;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>();
        return result != null ? result.Data : default;
    }

    public async Task<bool> AdminPostAsync(string url, object body, string token)
    {
        var req = AuthRequest(HttpMethod.Post, url, token);
        req.Content = JsonContent.Create(body);
        var resp = await _httpClient.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> AdminPutAsync(string url, object body, string token)
    {
        var req = AuthRequest(HttpMethod.Put, url, token);
        req.Content = JsonContent.Create(body);
        var resp = await _httpClient.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> AdminDeleteAsync(string url, string token)
    {
        var req = AuthRequest(HttpMethod.Delete, url, token);
        var resp = await _httpClient.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<string?> AdminUploadAsync(string url, byte[] fileBytes, string fileName, string token)
    {
        var req = AuthRequest(HttpMethod.Post, url, token);
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileBytes), "file", fileName);
        req.Content = content;
        var resp = await _httpClient.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return null;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<string>>();
        return result?.Data;
    }
}
