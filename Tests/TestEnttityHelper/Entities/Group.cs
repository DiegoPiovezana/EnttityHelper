using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entities
{
    [Table("TB_GROUP_USERS")]
    public class Group
    {
        [Key()] public int Id { get; internal set; }
        [Required] public string Name { get; internal set; }
        [Required] public string Description { get; internal set; }
        [InverseProperty(nameof(User.Groups))] public virtual ICollection<User> Users { get; internal set; } = new List<User>();
        public bool IsActive { get; internal set; } = true;
    }
}
