using Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

public class Actor
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string Gender { get; set; }
    public int Age { get; set; }
    [IMDBLinkValidation]
    public string IMDBLink { get; set; }
    public byte[]? Photo { get; set; }

    public string? Bio {  get; set; }

    [NotMapped]
    public IFormFile? PhotoFile { get; set; }
}
/*Name
Gender
Age
IMDB Hyperlink
Photo of the actor
*/