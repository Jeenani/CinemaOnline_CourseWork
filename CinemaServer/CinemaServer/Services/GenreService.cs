using CinemaServer.DTOs;
using CinemaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaServer.Services;

public class GenreService
{
    private readonly CinemaOnlineContext _context;

    public GenreService(CinemaOnlineContext context)
    {
        _context = context;
    }

    public async Task<List<GenreResponse>> GetAllAsync()
    {
        return await _context.Genres
            .OrderBy(g => g.DisplayName)
            .Select(g => new GenreResponse
            {
                Id = g.Id,
                Name = g.Name,
                DisplayName = g.DisplayName
            })
            .ToListAsync();
    }
}
