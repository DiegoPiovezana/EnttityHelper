using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestEH_UnitTest.Entities;

namespace TestEH_UnitTest.Entitities
{
    [Table("TB_GROUP_USERS")]
    internal class Group
    {
        [Key()] public int Id { get; internal set; }
        [Required] public string Name { get; internal set; }
        [Required] public string Description { get; internal set; }
        [InverseProperty(nameof(User.Groups))] public virtual ICollection<User> Users { get; internal set; } = new List<User>();

    }
}
