using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Id4meOwinAuth.Models
{
    public class Id4meClientRegistrations
    {
        [Key]
        [MaxLength(256)]
        [Column(Order = 2)]
        public string Autority { get; set; }
        [Key]
        [MaxLength(256)]
        [Column(Order = 1)]
        public string BaseUrl { get; set; }
        [Required]
        public string RegistrationData { get; set; }
    }
}
