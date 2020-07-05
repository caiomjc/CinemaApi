using CinameApi.Data;
using CinameApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace CinameApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly CinemaDbContext _dbContext;

        public MoviesController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpGet("[action]")]
        public IActionResult AllMovies(string sort = "asc", int pageNumber = 1, int pageSize = 3)
        {
            var movies = from movie in _dbContext.Movies
                         select new
                         {
                             movie.Id,
                             movie.Name,
                             movie.Duration,
                             movie.Language,
                             movie.Rating,
                             movie.Genre,
                             movie.ImageUrl
                         };

            if (sort.Equals("asc"))
            {
                movies = movies.OrderBy(m => m.Rating);
            }
            else
            {
                movies = movies.OrderByDescending(m => m.Rating);
            }

            return Ok(movies.Skip((pageNumber - 1) * pageSize).Take(pageSize));
        }

        [Authorize]
        [HttpGet("[action]/{id}")]
        public IActionResult MovieDetail(int id)
        {
            var movie = _dbContext.Movies.Find(id);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        [Authorize]
        [HttpGet("[action]")]
        public IActionResult FindMovies(string movieName)
        {
            var movies = from movie in _dbContext.Movies
                         where movie.Name.StartsWith(movieName)
                         select new
                         {
                             movie.Id,
                             movie.Name,
                             movie.ImageUrl
                         };

            return Ok(movies);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromForm] Movie movie)
        {
            if (movie.Image != null)
            {
                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot", $"{guid}.jpg");

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    movie.Image.CopyTo(fileStream);
                }

                movie.ImageUrl = filePath.Remove(0, 7);
            }

            _dbContext.Movies.Add(movie);
            _dbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Movie movieObj)
        {
            var movie = _dbContext.Movies.Find(id);

            if (movie == null)
            {
                return NotFound("Movie has not been found!");
            }
            else
            {
                string oldFilePath = string.Empty;

                if (movieObj.Image != null)
                {
                    oldFilePath = $"{Path.GetFullPath("wwwroot")}{movie.ImageUrl}";

                    var guid = Guid.NewGuid();
                    var filePath = Path.Combine("wwwroot", $"{guid}.jpg");

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        movieObj.Image.CopyTo(fileStream);
                    }

                    movie.ImageUrl = filePath.Remove(0, 7);
                }

                movie.Name = movieObj.Name;
                movie.Description = movieObj.Description;
                movie.Language = movieObj.Language;
                movie.Duration = movieObj.Duration;
                movie.PlayingDate = movieObj.PlayingDate;
                movie.PlayingTime = movieObj.PlayingTime;
                movie.Rating = movieObj.Rating;
                movie.Genre = movieObj.Genre;
                movie.TrailorUrl = movieObj.TrailorUrl;
                movie.TicketPrice = movieObj.TicketPrice;
                _dbContext.SaveChanges();

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                return Ok("Movie updated successfully!");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _dbContext.Movies.Find(id);

            if (movie == null)
            {
                return NotFound("Movie has not been found!");
            }
            else
            {
                var filePath = $"{Path.GetFullPath("wwwroot")}{movie.ImageUrl}";

                _dbContext.Movies.Remove(movie);
                _dbContext.SaveChanges();

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return Ok("Movie deleted!");
            }
        }
    }
}