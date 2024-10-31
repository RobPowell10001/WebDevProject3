using Entities;
namespace Models;

public class ActorDetailsViewModel
{
    public Actor Actor { get; set; }
    public IEnumerable<Movie> Movies { get; set; }

    public IEnumerable<Tuple<string, string>> Tweets { get; set; }

    public double OverallSentiment;

    public ActorDetailsViewModel(Actor actor, IEnumerable<Movie> movies, IEnumerable<Tuple<string,string>> tweets, double overallSentiment)
    {
        Actor = actor;
        Movies = movies;
        Tweets = tweets;
        OverallSentiment = overallSentiment;
    }
}
