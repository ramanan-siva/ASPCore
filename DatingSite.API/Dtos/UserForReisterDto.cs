using System.ComponentModel.DataAnnotations;

namespace DatingSite.API.Dtos
{
    public class UserForReisterDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(8, MinimumLength=4, ErrorMessage="Should be between 4 and  8")]
        public string Password { get; set; }
    }
}