using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.UserReference
{
    public class UserSkill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RowId { get; set; }
        public string UserId { get; set; }
        public string Skill { get; set; }
        public string Level { get; set; }
    }
}
