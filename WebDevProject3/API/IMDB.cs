namespace API;

public class IMDB
{
    IMDb.IMDb Database { get; set; }

    public IMDB()
    {
        Database = new IMDb.IMDb();
    }

    public static async Task<byte[]> GetImageByteArrayFromUrl(string imageUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            // Download image as a byte array
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
            return imageBytes;
        }
    }

    public async Task<IMDTO> TitleSearch(string title)
    {
        System.Diagnostics.Debug.WriteLine($"In Title Search");
        IMDb.Results results = Database.search(title);
        IMDb.Title movieResult = Database.title(results.titles[0].id);
        System.Diagnostics.Debug.WriteLine($"Past the Database Query!, movie id is {movieResult.id}");
        string summary = movieResult.plot;
        string genre = "";
        foreach (var item in movieResult.genres)
        {
            genre += $"{item.name}, ";
        }
        System.Diagnostics.Debug.WriteLine($"Past Genres");
        genre = genre.Substring(0, genre.Length - 2);
        System.Diagnostics.Debug.WriteLine($"Past Substring");
        string realTitle = movieResult.title;
        System.Diagnostics.Debug.WriteLine($"Past Title");
        string IMDBLink = $"https://www.imdb.com/title/{movieResult.id}/";
        System.Diagnostics.Debug.WriteLine($"Past Link");
        int releaseYear = Int32.Parse(movieResult.year);
        System.Diagnostics.Debug.WriteLine($"Past Year");
        var poster = await GetImageByteArrayFromUrl(movieResult.image.url);
        System.Diagnostics.Debug.WriteLine($"Image Constructed!");


        return new IMDTO(summary,genre,realTitle,IMDBLink,releaseYear,poster);
    }

}
