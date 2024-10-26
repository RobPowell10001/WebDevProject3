using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Movie
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    public string IMBDLink { get; set; }
    public string? Genre { get; set; }
    public int ReleaseYear { get; set; }
    public byte[]? Poster { get; set; }
    public string? Summary { get; set; }

}
/*
Title
IMDB hyperlink
Genre
Year of release
Poster or media
A section of the details page that calls the AI and sentiment analysis APIs with:
A list of actors in the movie
A table containing two columns:
Ten AI generated reviews relating to the selected movie
The analyzed sentiment of the reviews
A heading with the overall sentiment analysis about your chosen movie should be above the table
At least four movies you have entered into the database
*/

