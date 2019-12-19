using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.UserReference
{
    public class UserInformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RowId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public string Title { get; set; }
        public string Locations { get; set; }
        public string Geolocation { get; set; }
        public string Certifications { get; set; }

    }
}
