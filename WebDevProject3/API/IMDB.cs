using IMDb;
using System.Text.RegularExpressions;
using WebDevProject3.Data.Migrations;

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
        string titlesearch = Regex.Replace(title, "[^a-zA-Z0-9]", "");
        IMDb.Results results = Database.search(titlesearch, eSearch.All, false);
        IMDb.Title movieResult = Database.title(results.titles[0].id);
        string summary = movieResult.plot;
        string genre = "";
        foreach (var item in movieResult.genres)
        {
            genre += $"{item.name}, ";
        }
        genre = genre.Substring(0, genre.Length - 2);
        string realTitle = movieResult.originalTitle;
        string IMDBLink = $"https://www.imdb.com/title/{movieResult.id}/";
        int releaseYear = Int32.Parse(movieResult.year);
        var poster = await GetImageByteArrayFromUrl(movieResult.image.url);


        return new IMDTO(summary,genre,realTitle,IMDBLink,releaseYear,poster);
    }

}
