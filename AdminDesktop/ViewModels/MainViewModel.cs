using System.Windows;
using AdminDesktop.Models;
using AdminDesktop.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Extensions;
using SkiaSharp;

namespace AdminDesktop.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly AdminApiService _api;

    public AdminApiService Api => _api;

    // === Auth ===
    private bool _isLoggedIn;
    public bool IsLoggedIn { get => _isLoggedIn; set { SetProperty(ref _isLoggedIn, value); OnPropertyChanged(nameof(IsNotLoggedIn)); } }
    public bool IsNotLoggedIn => !_isLoggedIn;

    private string _loginEmail = "";
    public string LoginEmail { get => _loginEmail; set => SetProperty(ref _loginEmail, value); }

    private string _loginPassword = "";
    public string LoginPassword { get => _loginPassword; set => SetProperty(ref _loginPassword, value); }

    private string? _loginError;
    public string? LoginError { get => _loginError; set => SetProperty(ref _loginError, value); }

    private bool _isLoggingIn;
    public bool IsLoggingIn { get => _isLoggingIn; set => SetProperty(ref _isLoggingIn, value); }

    private string _adminName = "";
    public string AdminName { get => _adminName; set => SetProperty(ref _adminName, value); }

    // === Navigation ===
    private int _selectedTab;
    public int SelectedTab { get => _selectedTab; set { if (SetProperty(ref _selectedTab, value)) _ = LoadTabDataAsync(); } }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    private string? _statusMessage;
    public string? StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    // === Dashboard ===
    private AdminStats? _stats;
    public AdminStats? Stats
    {
        get => _stats;
        set
        {
            if (SetProperty(ref _stats, value) && value != null)
                BuildChartData(value);
        }
    }

    // Chart data
    private ISeries[] _contentPieSeries = [];
    public ISeries[] ContentPieSeries { get => _contentPieSeries; set => SetProperty(ref _contentPieSeries, value); }

    private ISeries[] _metricsBarSeries = [];
    public ISeries[] MetricsBarSeries { get => _metricsBarSeries; set => SetProperty(ref _metricsBarSeries, value); }

    private Axis[] _metricsXAxes = [];
    public Axis[] MetricsXAxes { get => _metricsXAxes; set => SetProperty(ref _metricsXAxes, value); }

    private Axis[] _metricsYAxes = [];
    public Axis[] MetricsYAxes { get => _metricsYAxes; set => SetProperty(ref _metricsYAxes, value); }

    private ISeries[] _activityGaugeSeries = [];
    public ISeries[] ActivityGaugeSeries { get => _activityGaugeSeries; set => SetProperty(ref _activityGaugeSeries, value); }

    // === Movies ===
    private List<AdminMovie> _movies = new();
    public List<AdminMovie> Movies { get => _movies; set => SetProperty(ref _movies, value); }

    private AdminMovie? _selectedMovie;
    public AdminMovie? SelectedMovie { get => _selectedMovie; set => SetProperty(ref _selectedMovie, value); }

    private bool _showMovieEditor;
    public bool ShowMovieEditor { get => _showMovieEditor; set => SetProperty(ref _showMovieEditor, value); }

    private string _editTitle = "", _editDescription = "", _editCountry = "", _editDirector = "";
    public string EditTitle { get => _editTitle; set => SetProperty(ref _editTitle, value); }
    public string EditDescription { get => _editDescription; set => SetProperty(ref _editDescription, value); }
    public string EditCountry { get => _editCountry; set => SetProperty(ref _editCountry, value); }
    public string EditDirector { get => _editDirector; set => SetProperty(ref _editDirector, value); }

    private string _editVkVideoUrl = "";
    public string EditVkVideoUrl { get => _editVkVideoUrl; set => SetProperty(ref _editVkVideoUrl, value); }

    private int? _editYear, _editDuration;
    public int? EditYear { get => _editYear; set => SetProperty(ref _editYear, value); }
    public int? EditDuration { get => _editDuration; set => SetProperty(ref _editDuration, value); }

    private bool _editNeedSub;
    public bool EditNeedSub { get => _editNeedSub; set => SetProperty(ref _editNeedSub, value); }

    private long? _editMovieId;

    private List<GenreItem> _genres = new();
    public List<GenreItem> Genres { get => _genres; set => SetProperty(ref _genres, value); }

    private List<long> _editGenreIds = new();
    public List<long> EditGenreIds { get => _editGenreIds; set => SetProperty(ref _editGenreIds, value); }

    // === Users ===
    private List<AdminUser> _users = new();
    public List<AdminUser> Users { get => _users; set => SetProperty(ref _users, value); }

    // === Content ===
    private List<AdminComment> _comments = new();
    public List<AdminComment> Comments { get => _comments; set => SetProperty(ref _comments, value); }

    private List<AdminCollection> _collections = new();
    public List<AdminCollection> Collections { get => _collections; set => SetProperty(ref _collections, value); }

    private string _newGenreName = "";
    public string NewGenreName { get => _newGenreName; set => SetProperty(ref _newGenreName, value); }

    // === Commands ===
    public AsyncRelayCommand LoginCommand { get; }
    public RelayCommand LogoutCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddMovieCommand { get; }
    public AsyncRelayCommand SaveMovieCommand { get; }
    public RelayCommand CancelEditCommand { get; }
    public AsyncRelayCommand AddGenreCommand { get; }

    public MainViewModel()
    {
        _api = new AdminApiService();
        _api.OnSessionExpired += msg =>
        {
            IsLoggedIn = false;
            AdminName = "";
            LoginError = msg;
            StatusMessage = msg;
        };
        LoginCommand = new AsyncRelayCommand(DoLoginAsync);
        LogoutCommand = new RelayCommand(() => { _api.Logout(); IsLoggedIn = false; AdminName = ""; });
        RefreshCommand = new AsyncRelayCommand(LoadTabDataAsync);
        AddMovieCommand = new RelayCommand(DoAddMovie);
        SaveMovieCommand = new AsyncRelayCommand(DoSaveMovieAsync);
        CancelEditCommand = new RelayCommand(() => ShowMovieEditor = false);
        AddGenreCommand = new AsyncRelayCommand(DoAddGenreAsync);
    }

    private async Task DoLoginAsync()
    {
        LoginError = null;
        IsLoggingIn = true;
        var (ok, error) = await _api.LoginAsync(LoginEmail, LoginPassword);
        IsLoggingIn = false;

        if (ok)
        {
            AdminName = _api.CurrentUser?.Name ?? "Admin";
            IsLoggedIn = true;
            LoginPassword = "";
            await LoadTabDataAsync();
        }
        else
        {
            LoginError = error;
        }
    }

    public async Task LoadTabDataAsync()
    {
        if (!IsLoggedIn) return;
        IsBusy = true;
        StatusMessage = "Загрузка...";
        try
        {
            switch (SelectedTab)
            {
                case 0:
                    Stats = await _api.GetStatsAsync();
                    break;
                case 1:
                    Movies = await _api.GetMoviesAsync() ?? new();
                    Genres = await _api.GetGenresAsync() ?? new();
                    break;
                case 2:
                    Users = await _api.GetUsersAsync() ?? new();
                    break;
                case 3:
                    Genres = await _api.GetGenresAsync() ?? new();
                    Collections = await _api.GetCollectionsAsync() ?? new();
                    Comments = await _api.GetCommentsAsync() ?? new();
                    break;
            }
            StatusMessage = $"Обновлено {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    // === Chart builders ===
    private void BuildChartData(AdminStats s)
    {
        var accent = new SKColor(0xFF, 0x6B, 0x9D);
        var colors = new SKColor[]
        {
            new(0xFF, 0x6B, 0x9D),  // pink
            new(0x9D, 0x6B, 0xFF),  // purple
            new(0x6B, 0xC5, 0xFF),  // blue
            new(0xFF, 0xC8, 0x6B),  // orange
            new(0x6B, 0xFF, 0xAA),  // green
        };

        // --- Pie: content distribution ---
        ContentPieSeries = new ISeries[]
        {
            new PieSeries<int> { Values = new[] { s.MoviesCount },      Name = "Фильмы",     Fill = new SolidColorPaint(colors[0]) },
            new PieSeries<int> { Values = new[] { s.GenresCount },      Name = "Жанры",       Fill = new SolidColorPaint(colors[1]) },
            new PieSeries<int> { Values = new[] { s.CollectionsCount }, Name = "Коллекции",   Fill = new SolidColorPaint(colors[2]) },
            new PieSeries<int> { Values = new[] { s.CommentsCount },    Name = "Комментарии", Fill = new SolidColorPaint(colors[3]) },
        };

        // --- Bar: key metrics ---
        var labels = new[] { "Пользователи", "Комментарии", "Подписки", "Оплаты", "Фильмы" };
        var values = new double[] { s.UsersCount, s.CommentsCount, s.ActiveSubscriptions, s.PaidPayments, s.MoviesCount };

        MetricsBarSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = values,
                Fill = new SolidColorPaint(accent),
                Stroke = null,
                MaxBarWidth = 32,
                Rx = 4,
                Ry = 4,
            }
        };

        var labelPaint = new SolidColorPaint(new SKColor(0x8A, 0x7A, 0x80));
        MetricsXAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsPaint = labelPaint,
                TextSize = 11,
                SeparatorsPaint = new SolidColorPaint(new SKColor(0x3D, 0x15, 0x25)),
            }
        };
        MetricsYAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = labelPaint,
                TextSize = 11,
                SeparatorsPaint = new SolidColorPaint(new SKColor(0x3D, 0x15, 0x25)) { StrokeThickness = 0.5f },
                MinLimit = 0,
            }
        };

        // --- Gauge: subscriptions vs total users ---
        var subPercent = s.UsersCount > 0 ? (double)s.ActiveSubscriptions / s.UsersCount * 100 : 0;
        ActivityGaugeSeries = GaugeGenerator.BuildSolidGauge(
            new GaugeItem(subPercent, g =>
            {
                g.Fill = new SolidColorPaint(accent);
                g.MaxRadialColumnWidth = 24;
            }),
            new GaugeItem(GaugeItem.Background, g =>
            {
                g.Fill = new SolidColorPaint(new SKColor(0x3D, 0x15, 0x25));
                g.MaxRadialColumnWidth = 24;
            })
        ).ToArray();
    }

    // === Movie CRUD ===
    private void DoAddMovie()
    {
        _editMovieId = null;
        EditTitle = ""; EditDescription = ""; EditCountry = ""; EditDirector = "";
        EditVkVideoUrl = ""; EditYear = null; EditDuration = null;
        EditNeedSub = false; EditGenreIds = new();
        ShowMovieEditor = true;
    }

    public void DoEditMovie(AdminMovie m)
    {
        _editMovieId = m.Id;
        EditTitle = m.Title; EditDescription = m.Description ?? "";
        EditCountry = m.Country ?? ""; EditDirector = m.Director ?? "";
        EditVkVideoUrl = m.VkVideoUrl ?? "";
        EditYear = m.ReleaseYear; EditDuration = m.DurationMinutes;
        EditNeedSub = m.NeedSubscription;
        EditGenreIds = new(m.GenreIds);
        ShowMovieEditor = true;
    }

    private async Task DoSaveMovieAsync()
    {
        if (string.IsNullOrWhiteSpace(EditTitle)) { StatusMessage = "Укажите название"; return; }

        var body = new
        {
            Title = EditTitle,
            Description = string.IsNullOrWhiteSpace(EditDescription) ? null : EditDescription,
            ReleaseYear = EditYear,
            DurationMinutes = EditDuration,
            VideoUrl = (string?)null,
            VkVideoUrl = string.IsNullOrWhiteSpace(EditVkVideoUrl) ? null : EditVkVideoUrl,
            Country = string.IsNullOrWhiteSpace(EditCountry) ? null : EditCountry,
            Director = string.IsNullOrWhiteSpace(EditDirector) ? null : EditDirector,
            NeedSubscription = EditNeedSub,
            GenreIds = EditGenreIds
        };

        bool ok = _editMovieId.HasValue
            ? await _api.PutAsync($"/api/movies/{_editMovieId}", body)
            : await _api.PostAsync("/api/movies", body);

        if (ok)
        {
            ShowMovieEditor = false;
            StatusMessage = "Фильм сохранён";
            Movies = await _api.GetMoviesAsync() ?? new();
        }
        else StatusMessage = "Ошибка сохранения";
    }

    public async Task DeleteMovieAsync(long id)
    {
        if (MessageBox.Show("Удалить фильм?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        await _api.DeleteAsync($"/api/movies/{id}");
        Movies = await _api.GetMoviesAsync() ?? new();
        StatusMessage = "Фильм удалён";
    }

    // === Users ===
    public async Task ChangeRoleAsync(long userId, string role)
    {
        await _api.PutAsync($"/api/admin/users/{userId}/role", new { Role = role });
        Users = await _api.GetUsersAsync() ?? new();
    }

    public async Task DeleteUserAsync(long userId)
    {
        if (MessageBox.Show("Удалить пользователя?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        await _api.DeleteAsync($"/api/admin/users/{userId}");
        Users = await _api.GetUsersAsync() ?? new();
    }

    // === Content ===
    private async Task DoAddGenreAsync()
    {
        if (string.IsNullOrWhiteSpace(NewGenreName)) return;
        var code = NewGenreName.Trim().ToLower().Replace(" ", "_");
        await _api.PostAsync("/api/admin/genres", new { Name = code, DisplayName = NewGenreName.Trim() });
        NewGenreName = "";
        Genres = await _api.GetGenresAsync() ?? new();
    }

    public async Task DeleteGenreAsync(long id)
    {
        await _api.DeleteAsync($"/api/admin/genres/{id}");
        Genres = await _api.GetGenresAsync() ?? new();
    }

    public async Task ToggleCommentAsync(long id)
    {
        await _api.PutAsync($"/api/admin/comments/{id}/toggle", new { });
        Comments = await _api.GetCommentsAsync() ?? new();
    }

    public async Task DeleteCommentAsync(long id)
    {
        await _api.DeleteAsync($"/api/admin/comments/{id}");
        Comments = await _api.GetCommentsAsync() ?? new();
    }

    // === VK Video helper ===
    public void ExtractVkUrl(string iframeCode)
    {
        if (string.IsNullOrWhiteSpace(iframeCode)) return;
        var code = iframeCode.Trim();

        if (code.StartsWith("https://vkvideo.ru/video_ext.php") || code.StartsWith("https://vk.com/video_ext.php"))
        {
            EditVkVideoUrl = code;
            return;
        }

        var srcIdx = code.IndexOf("src=\"", StringComparison.OrdinalIgnoreCase);
        if (srcIdx >= 0)
        {
            srcIdx += 5;
            var endIdx = code.IndexOf('"', srcIdx);
            if (endIdx > srcIdx)
            {
                EditVkVideoUrl = code[srcIdx..endIdx];
                return;
            }
        }

        srcIdx = code.IndexOf("src='", StringComparison.OrdinalIgnoreCase);
        if (srcIdx >= 0)
        {
            srcIdx += 5;
            var endIdx = code.IndexOf('\'', srcIdx);
            if (endIdx > srcIdx)
            {
                EditVkVideoUrl = code[srcIdx..endIdx];
                return;
            }
        }
    }
}
