using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Entities;
using WebDevProject3.Data;
using Microsoft.Extensions.Options;
using API;
using Models;

namespace Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private VaderAPI Vader;
        private LLM SummaryLanguageModel;
        private LLM ReviewLanguageModel;

        

        public MoviesController(ApplicationDbContext context, IOptions<Settings.OAIConfig> oaiConfig)
        {
            _context = context;
            Vader = new VaderAPI();
            SummaryLanguageModel = new LLM(oaiConfig);
            SummaryLanguageModel.systemPrompt = "Taking the user prompt as the name of a movie, provide a brief synopsis of the movie. (Within 40 words)";
            ReviewLanguageModel = new LLM(oaiConfig);
            ReviewLanguageModel.systemPrompt = "The user prompt will be the name of a movie. Provide ten 30-80 word reviews of the movie, each with varying opinions. Ensure the reviews are realistic and full-length, not singular sentences. Do not number your responses; provide only the reviews, delimited with |";
        }
        private async Task<string> GenerateReviews(Movie movie)
        {
            string userExample = "Titanic";
            string assistantExample = "Titanic is a cinematic masterpiece with breathtaking visuals and a heartbreaking love story. Leonardo DiCaprio and Kate Winslet are perfectly cast, and their chemistry makes the tragedy even more impactful.|A bit too long for my taste, but I have to admit the special effects are impressive. The romance is a little melodramatic, but it’s iconic for a reason.|I love Titanic for its blend of history and drama. The characters bring the era to life, and the sinking scenes are intense. It’s a classic that keeps you invested from start to finish.|Honestly, I don’t see why everyone loves this movie so much. It’s well-made, sure, but the romance feels forced, and the ending is frustrating. Not for me.|Titanic is more than a movie—it’s an experience. The score, the acting, and the attention to detail create an emotional journey. It’s easy to see why it’s considered one of the best.|Despite being a bit melodramatic, Titanic succeeds in balancing romance and tragedy. The performances are memorable, and the cinematography beautifully captures the grandeur of the ship.|Watching Titanic is an emotional rollercoaster. Jack and Rose’s love story against the backdrop of the sinking ship is unforgettable, and the final scenes are genuinely moving.|While Titanic is visually impressive, it feels overly sentimental. The love story, while engaging, overshadows the historical event, which I would have liked more focus on.|James Cameron outdid himself with Titanic. The movie is visually stunning, and the love story adds depth to the tragedy. It’s a film that will stand the test of time.|Not everyone’s cup of tea, especially with its length, but Titanic has undeniable charm. The sinking scenes are gripping, and the movie makes you feel like you’re truly aboard the ill-fated ship.";
            return await ReviewLanguageModel.CallWithSystemPromptAndExample(userExample, assistantExample, movie.Title);
        }


        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            var actors = await _context.Role
                .Include(r => r.Actor)
                .Where(r => r.MovieId == movie.Id)
                .Select(r => r.Actor)
                .ToListAsync();
            
            IEnumerable<string> LLMOutput = (await GenerateReviews(movie).ConfigureAwait(false)).Split("|");

            IEnumerable<Tuple<string, string>> reviews = [];

            double averageSentiment = 0.0;
            
            foreach (string review in LLMOutput)
            { 
                var sentiment = Vader.AnalyzeSentiment(review);
                string sentimentString = $"Positivity: {sentiment.Item1}\n Negativity: {sentiment.Item2}\n Overall: {sentiment.Item3}";
                averageSentiment += sentiment.Item3 / 10;
                reviews = reviews.Append(new Tuple<string, string>(review, sentimentString));
            }

            var detailsModel = new MovieDetailsViewModel(movie, actors, reviews, averageSentiment);

            return View(detailsModel);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Title,IMBDLink,Genre,ReleaseYear,Poster,Summary")] Movie movie, IFormFile? PosterFile)
        //{
        //    ModelState.Remove("Poster");

        //    if (PosterFile != null && PosterFile.Length > 0)
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await PosterFile.CopyToAsync(memoryStream);
        //            movie.Poster = memoryStream.ToArray(); // Convert to byte[]
        //        }
        //    }
        //    movie.Summary = await SummaryLanguageModel.CallWithSystemPrompt(movie.Title);

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(movie);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(movie);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMBDLink,Genre,ReleaseYear,Poster,Summary")] Movie movie)
        {
            ModelState.Remove("Poster");

            IMDTO dto = await new IMDB().TitleSearch(movie.Title);
            System.Diagnostics.Debug.WriteLine($"Past TitleSearch");
            movie.Title = dto.Title;
            movie.IMBDLink = dto.IMBDLink;
            movie.ReleaseYear = (int)dto.ReleaseYear;
            movie.Genre = dto.Genre;
            movie.Poster = dto.Poster; 
            movie.Summary = dto.Summary;
            System.Diagnostics.Debug.WriteLine($"DTO Resolved, Summary is {movie.Summary}");

            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"Model State is Valid");
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            } else
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
                    }
                }
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMBDLink,Genre,ReleaseYear,Poster,Summary")] Movie movie, IFormFile? PosterFile)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (PosterFile != null && PosterFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await PosterFile.CopyToAsync(memoryStream);
                    movie.Poster = memoryStream.ToArray(); // Convert to byte[]
                }
            }
            else
            {
                var existingMovie = await _context.Movie.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                movie.Poster = existingMovie.Poster; // Keep the existing photo
            }

            if (movie.Summary != null && movie.Summary.Length > 0)
            {
            }
            else
            {
                var existingMovie = await _context.Movie.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                movie.Summary = existingMovie.Summary; // Keep the existing summary
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
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
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
