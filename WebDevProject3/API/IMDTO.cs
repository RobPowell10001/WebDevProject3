namespace API;

public class IMDTO
{
    public byte[]? Poster { get; set; }
    public string? Summary { get; set; }
    public string? Genre { get; set; }
    public string? Title { get; set; }
    public string? IMBDLink { get; set; }
    public int? ReleaseYear { get; set; }

    public IMDTO(string? summary, string? genre, string? title, string? iMBDLink, int? releaseYear, byte[]? poster)
    { 
        Summary = summary;
        Genre = genre;
        Title = title;
        IMBDLink = iMBDLink;
        ReleaseYear = releaseYear;
        Poster = poster;
    }
}
