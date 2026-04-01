using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly CinemaOnlineContext _context;

    public AdminController(CinemaOnlineContext context)
    {
        _context = context;
    }

    // ==================== STATS ====================

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats([FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var stats = new
        {
            MoviesCount = await _context.Movies.CountAsync(),
            UsersCount = await _context.Users.CountAsync(),
            CommentsCount = await _context.Comments.CountAsync(),
            GenresCount = await _context.Genres.CountAsync(),
            CollectionsCount = await _context.Collections.CountAsync(),
            ActiveSubscriptions = await _context.Users.CountAsync(u => u.HasSubscription == true),
            TotalViews = await _context.Movies.SumAsync(m => m.ViewCount ?? 0),
            PaidPayments = await _context.Payments.CountAsync(p => p.Status == "paid")
        };
        return Ok(new ApiResponse<object> { Success = true, Data = stats });
    }

    // ==================== USERS ====================

    [HttpGet("users")]
    public async Task<ActionResult> GetUsers([FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var users = await _context.Users
            .Include(u => u.Subscription)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id, u.Email, u.Name, u.Role,
                HasSubscription = u.HasSubscription ?? false,
                SubscriptionName = u.Subscription != null ? u.Subscription.Name : null,
                SubscriptionEnd = u.SubscriptionEndDate,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<object> { Success = true, Data = users });
    }

    [HttpPut("users/{id}/role")]
    public async Task<ActionResult> UpdateUserRole(
        long id, [FromBody] UpdateRoleRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Role = request.Role;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(
        long id, [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    // ==================== GENRES ====================

    [HttpPost("genres")]
    public async Task<ActionResult> CreateGenre(
        [FromBody] GenreRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var genre = new Genre { Name = request.Name, DisplayName = request.DisplayName };
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<long> { Success = true, Data = genre.Id });
    }

    [HttpPut("genres/{id}")]
    public async Task<ActionResult> UpdateGenre(
        long id, [FromBody] GenreRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var genre = await _context.Genres.FindAsync(id);
        if (genre == null) return NotFound();

        genre.Name = request.Name;
        genre.DisplayName = request.DisplayName;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    [HttpDelete("genres/{id}")]
    public async Task<ActionResult> DeleteGenre(
        long id, [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var genre = await _context.Genres.FindAsync(id);
        if (genre == null) return NotFound();

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    // ==================== COLLECTIONS ====================

    [HttpGet("collections")]
    public async Task<ActionResult> GetCollections([FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var collections = await _context.Collections
            .Include(c => c.CollectionMovies)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.Id, c.Name, c.Description,
                IsFeatured = c.IsFeatured ?? false,
                DisplayOrder = c.DisplayOrder ?? 0,
                MoviesCount = c.CollectionMovies.Count
            })
            .ToListAsync();

        return Ok(new ApiResponse<object> { Success = true, Data = collections });
    }

    [HttpPost("collections")]
    public async Task<ActionResult> CreateCollection(
        [FromBody] CollectionRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var collection = new Collection
        {
            Name = request.Name,
            Description = request.Description,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder
        };
        _context.Collections.Add(collection);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<long> { Success = true, Data = collection.Id });
    }

    [HttpPut("collections/{id}")]
    public async Task<ActionResult> UpdateCollection(
        long id, [FromBody] CollectionRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var collection = await _context.Collections.FindAsync(id);
        if (collection == null) return NotFound();

        collection.Name = request.Name;
        collection.Description = request.Description;
        collection.IsFeatured = request.IsFeatured;
        collection.DisplayOrder = request.DisplayOrder;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    [HttpDelete("collections/{id}")]
    public async Task<ActionResult> DeleteCollection(
        long id, [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var collection = await _context.Collections.FindAsync(id);
        if (collection == null) return NotFound();

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    [HttpPost("collections/{id}/movies")]
    public async Task<ActionResult> AddMovieToCollection(
        long id, [FromBody] CollectionMovieRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var exists = await _context.CollectionMovies
            .AnyAsync(cm => cm.CollectionId == id && cm.MovieId == request.MovieId);
        if (exists) return Ok(new ApiResponse<bool> { Success = true, Data = true });

        _context.CollectionMovies.Add(new CollectionMovie
        {
            CollectionId = id,
            MovieId = request.MovieId,
            Position = request.Position
        });
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    [HttpDelete("collections/{id}/movies/{movieId}")]
    public async Task<ActionResult> RemoveMovieFromCollection(
        long id, long movieId,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var cm = await _context.CollectionMovies
            .FirstOrDefaultAsync(x => x.CollectionId == id && x.MovieId == movieId);
        if (cm == null) return NotFound();

        _context.CollectionMovies.Remove(cm);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    // ==================== COMMENTS ====================

    [HttpGet("comments")]
    public async Task<ActionResult> GetAllComments([FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var comments = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Movie)
            .OrderByDescending(c => c.CreatedAt)
            .Take(200)
            .Select(c => new
            {
                c.Id,
                UserName = c.User != null ? c.User.Name : "Unknown",
                MovieTitle = c.Movie != null ? c.Movie.Title : "Unknown",
                c.Content,
                IsVisible = c.IsVisible ?? true,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<object> { Success = true, Data = comments });
    }

    [HttpPut("comments/{id}/toggle")]
    public async Task<ActionResult> ToggleCommentVisibility(
        long id, [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        comment.IsVisible = !(comment.IsVisible ?? true);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    [HttpDelete("comments/{id}")]
    public async Task<ActionResult> DeleteComment(
        long id, [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }

    // ==================== MOVIES (all, including unpublished) ====================

    [HttpGet("movies")]
    public async Task<ActionResult> GetAllMovies([FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        var movies = await _context.Movies
            .Include(m => m.Genres)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id, m.Title, m.ReleaseYear, m.Country, m.Director,
                m.PosterUrl, m.BannerUrl, m.VkVideoUrl,
                m.VideoUrl, m.Description, m.DurationMinutes,
                IsPublished = m.IsPublished ?? true,
                NeedSubscription = m.NeedSubscription ?? false,
                AverageRating = m.AverageRating ?? 0,
                ViewCount = m.ViewCount ?? 0,
                CommentCount = m.CommentCount ?? 0,
                Genres = m.Genres.Select(g => g.DisplayName).ToList(),
                GenreIds = m.Genres.Select(g => g.Id).ToList()
            })
            .ToListAsync();

        return Ok(new ApiResponse<object> { Success = true, Data = movies });
    }

    // ==================== UPLOAD ====================

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UploadImage(
        IFormFile file,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        if (!IsAdmin(authorization)) return Unauthorized(new ErrorResponse { Message = "Требуются права администратора" });

        if (file == null || file.Length == 0)
            return BadRequest(new ErrorResponse { Message = "Файл не выбран" });

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
            return BadRequest(new ErrorResponse { Message = "Допустимы только jpg, png, webp" });

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "posters");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/posters/{fileName}";
        return Ok(new ApiResponse<string> { Success = true, Data = url });
    }

    private static bool IsAdmin(string? authorization)
    {
        var role = AuthController.GetUserRoleFromToken(authorization);
        return role == "admin";
    }
}

// Admin DTOs
public class UpdateRoleRequest
{
    public string Role { get; set; } = "user";
}

public class GenreRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class CollectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}

public class CollectionMovieRequest
{
    public long MovieId { get; set; }
    public int Position { get; set; }
}
