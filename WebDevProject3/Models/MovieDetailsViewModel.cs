using Entities;
namespace Models;

public class MovieDetailsViewModel
{
    public Movie Movie { get; set; }
    public IEnumerable<Actor> Actors { get; set; }

    public IEnumerable<Tuple<string, string>> Reviews { get; set; }

    public double OverallSentiment;

    public MovieDetailsViewModel(Movie movie, IEnumerable<Actor> actors, IEnumerable<Tuple<string, string>> reviews, double overallSentiment)
    {
        Movie = movie;
        Actors = actors;
        Reviews = reviews;
        OverallSentiment = overallSentiment;
    }
}
