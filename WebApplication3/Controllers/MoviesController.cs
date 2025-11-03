using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsFolder;

        public MoviesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _uploadsFolder = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
        }

        // GET: Movies
        public async Task<IActionResult> Index(int? genreId, int? directorId, string? title)
        {
            var moviesQuery = _context.Movies.Include(m => m.Director).Include(m => m.Genre).AsQueryable();
            if (genreId.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.GenreId == genreId.Value);
            }
            if (directorId.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.DirectorId == directorId.Value);
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                var term = title.Trim();
                moviesQuery = moviesQuery.Where(m => m.Title.Contains(term));
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", genreId);
            ViewData["DirectorId"] = new SelectList(_context.Directors, "Id", "Name", directorId);
            ViewData["TitleFilter"] = title;
            return View(await moviesQuery.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            ViewData["DirectorId"] = new SelectList(_context.Directors, "Id", "Name");
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            ViewData["ActorIds"] = new MultiSelectList(_context.Actors, "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Synopsis,Duration,ReleaseDate,Image,GenreId,DirectorId")] Movie movie, IFormFile? poster, int[]? selectedActors)
        {
            if (ModelState.IsValid)
            {
                if (poster != null && poster.Length > 0)
                {
                    Directory.CreateDirectory(_uploadsFolder);
                    var ext = Path.GetExtension(poster.FileName);
                    var fileName = $"movie_{Guid.NewGuid():N}{ext}";
                    var savePath = Path.Combine(_uploadsFolder, fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await poster.CopyToAsync(stream);
                    }
                    movie.Image = $"/uploads/{fileName}";
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();

                if (selectedActors != null && selectedActors.Length > 0)
                {
                    var links = selectedActors.Distinct().Select(actorId => new MovieActor
                    {
                        MovieId = movie.Id,
                        ActorId = actorId
                    });
                    _context.MovieActors.AddRange(links);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["DirectorId"] = new SelectList(_context.Directors, "Id", "Name", movie.DirectorId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            ViewData["ActorIds"] = new MultiSelectList(_context.Actors, "Id", "Name", selectedActors);
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            ViewData["DirectorId"] = new SelectList(_context.Directors, "Id", "Name", movie.DirectorId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            ViewData["ActorIds"] = new MultiSelectList(_context.Actors, "Id", "Name", movie.MovieActors.Select(ma => ma.ActorId));
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Synopsis,Duration,ReleaseDate,Image,GenreId,DirectorId")] Movie input, IFormFile? poster, int[]? selectedActors)
        {
            if (id != input.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var movie = await _context.Movies
                    .Include(m => m.MovieActors)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (movie == null)
                {
                    return NotFound();
                }

                movie.Title = input.Title;
                movie.Synopsis = input.Synopsis;
                movie.Duration = input.Duration;
                movie.ReleaseDate = input.ReleaseDate;
                movie.GenreId = input.GenreId;
                movie.DirectorId = input.DirectorId;

                if (poster != null && poster.Length > 0)
                {
                    Directory.CreateDirectory(_uploadsFolder);
                    var ext = Path.GetExtension(poster.FileName);
                    var fileName = $"movie_{Guid.NewGuid():N}{ext}";
                    var savePath = Path.Combine(_uploadsFolder, fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await poster.CopyToAsync(stream);
                    }
                    movie.Image = $"/uploads/{fileName}";
                }
                else
                {
                    movie.Image = input.Image;
                }

                var selected = new HashSet<int>(selectedActors ?? Array.Empty<int>());
                var current = new HashSet<int>(movie.MovieActors.Select(ma => ma.ActorId));

                foreach (var actorId in selected.Except(current))
                {
                    movie.MovieActors.Add(new MovieActor { MovieId = movie.Id, ActorId = actorId });
                }
                movie.MovieActors = movie.MovieActors.Where(ma => selected.Contains(ma.ActorId)).ToList();

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DirectorId"] = new SelectList(_context.Directors, "Id", "Name", input.DirectorId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", input.GenreId);
            ViewData["ActorIds"] = new MultiSelectList(_context.Actors, "Id", "Name", selectedActors);
            return View(input);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
