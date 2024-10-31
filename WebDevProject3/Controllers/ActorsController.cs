using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Entities;
using WebDevProject3.Data;
using API;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Models;

namespace Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private LLM BioLanguageModel;
        private LLM TweetLanguageModel;
        private VaderAPI Vader;

        public ActorsController(ApplicationDbContext context, IOptions<Settings.OAIConfig> oaiConfig)
        {
            _context = context;
            Vader = new VaderAPI();
            BioLanguageModel = new LLM(oaiConfig);
            BioLanguageModel.systemPrompt = "Taking the user prompt as the name of an actor, provide a brief bio for the actor. (Within 40 words)";
            TweetLanguageModel = new LLM(oaiConfig);
            TweetLanguageModel.systemPrompt = "Taking the user prompt as the name of an actor, generate 20 realistic tweets they might make. Do not number your output; delimit each tweet with |";
        }
        
        private async Task<string> GenerateTweets(Actor actor)
        {
            string userExample = "Tom Hanks";
            string assistantExample = "Feeling so lucky today. Grateful for every opportunity, every fan, every friend in this journey. Thanks for sticking with me! |Just had my first pumpkin spice latte of the season. Can’t decide if it’s amazing or overrated... thoughts? |Trying to cook dinner tonight without burning the place down. Wish me luck. 🍝🔥 |Someone just called me \"Spider-Boy\" in the street… close enough, I guess. 😂 |Big shoutout to my incredible stunt team—without them, I'd probably still be on set trying to do a backflip. |Thinking of adopting a dog. Need some name suggestions, hit me up! 🐾 |Finally binge-watched that show everyone’s been talking about. Now I get the hype. |Still not over how amazing the fans were at last night’s premiere. Love you all to the moon and back! 🌙 |Mornings aren’t so bad when you start with a strong coffee and a good playlist. What’s everyone listening to today? ☕🎶 |My brother just beat me at FIFA. Still processing. It’s gonna be a while before I can let this go. 🎮 |Just found out there’s a Spider-Man emoji on here now 🕷️👏 Life goal unlocked! |Thinking about starting a YouTube channel… but do I actually have the skills? (Probably not.) What do you guys think? |That feeling when you get home and realize you left the milk out… again. |Is it weird that I still sometimes catch myself quoting lines from the Spider-Man movies? Guess it's just muscle memory now. |Thank you all for the insane support and love. I honestly don’t know what I did to deserve you guys. You’re the best. ❤️ |Someone told me I should try out skydiving. Who’s done it? Worth the freak-out? |Proud to support amazing projects and causes making a difference. What are some you guys love? Let me know. |Missed my flight today because I stopped to pet a dog. Worth it. |Been reading your messages and honestly can’t stop smiling. You guys are beyond amazing. Much love!|Got a new project I’m really excited about. Can't wait to share it with you all—stay tuned!";
            return await TweetLanguageModel.CallWithSystemPromptAndExample(userExample, assistantExample, actor.Name);
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var movies = await _context.Role
                .Include(r => r.Movie)
                .Where(r => r.ActorId == actor.Id)
                .Select(r => r.Movie)
                .ToListAsync();

            IEnumerable<string> LLMOutput = (await GenerateTweets(actor).ConfigureAwait(false)).Split("|");

            IEnumerable<Tuple<string, string>> tweets = [];

            double averageSentiment = 0.0;

            foreach (string tweet in LLMOutput)
            {
                var sentiment = Vader.AnalyzeSentiment(tweet);
                string sentimentString = $"Positivity: {sentiment.Item1}\n Negativity: {sentiment.Item2}\n Overall: {sentiment.Item3}";
                averageSentiment += sentiment.Item3 / 20;
                tweets = tweets.Append(new Tuple<string, string>(tweet, sentimentString));
            }

            var detailsModel = new ActorDetailsViewModel(actor, movies, tweets, averageSentiment);

            return View(detailsModel);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBLink,Photo, Bio")] Actor actor, IFormFile? PhotoFile)
        {
            ModelState.Remove("Photo");

            if (PhotoFile != null && PhotoFile.Length > 0)
            {

                using (var memoryStream = new MemoryStream())
                {
                    await PhotoFile.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray(); // Convert to byte[]
                }
            }

            actor.Bio = await BioLanguageModel.CallWithSystemPrompt(actor.Name);

            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink,Photo,Bio")] Actor actor, IFormFile? PhotoFile)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await PhotoFile.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray(); // Convert to byte[]
                }
            }
            else
            {
                var existingActor = await _context.Actor.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                actor.Photo = existingActor.Photo; // Keep the existing photo
            }

            if (actor.Bio != null && actor.Bio.Length > 0)
            {
            }
            else
            {
                var existingActor = await _context.Actor.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                actor.Bio = existingActor.Bio; // Keep the existing summary
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
