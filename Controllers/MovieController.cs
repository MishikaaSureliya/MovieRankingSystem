using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieRankingSystem.Services;

namespace MovieRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovieController : ControllerBase
    {
        private readonly RankingService _rankingService;

        public MovieController(RankingService rankingService)
        {
            _rankingService = rankingService;
        }

        [HttpGet("search")]
        public IActionResult Search(string query)
        {
            var allMovies = _rankingService.GetMoviesFromCsv("Dataset/movies.csv");

            // Filter based on query
            var filteredMovies = allMovies
                .Where(m => m.Query.ToLower() == query.ToLower())
                .ToList();

            // Apply ranking
            var ranked = filteredMovies
                .Select(m => new
                {
                    Movie = m.Movie,
                    Score = _rankingService.Predict(m)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            return Ok(ranked);
        }

        [HttpGet("compare")]
        public IActionResult Compare(string query)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Dataset", "movies.csv");
            var allMovies = _rankingService.GetMoviesFromCsv(filePath);

            var filtered = allMovies
                .Where(m => m.Query.ToLower() == query.ToLower())
                .ToList();

            // 🔹 POINTWISE (simple scoring)
            var pointwise = filtered
                .OrderByDescending(m => m.Rating) // basic logic
                .Select(m => new { m.Movie, Score = m.Rating })
                .ToList();

            // 🔹 PAIRWISE (simulated)
            var pairwise = filtered
                .OrderByDescending(m => (m.Rating * 0.7 + m.Popularity * 0.3))
                .Select(m => new { m.Movie, Score = (m.Rating * 0.7 + m.Popularity * 0.3) })
                .ToList();

            // 🔹 LISTWISE (your ML model 🔥)
            var listwise = filtered
                .Select(m => new
                {
                    m.Movie,
                    Score = _rankingService.Predict(m)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            return Ok(new
            {
                pointwise,
                pairwise,
                listwise
            });
        }
    }
}
