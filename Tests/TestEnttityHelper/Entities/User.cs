using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestEH_UnitTest.Entities;

namespace TestEH_UnitTest.Entities
{
    [Table("TB_USERS")]
    public class User
    {
        [Key()] public long Id { get; internal set; }
        [Required][MaxLength(300)] public string Name { get; internal set; }
        [Required][MaxLength(100)] public string GitHub { get; internal set; }
        public DateTime DtCreation { get; internal set; }
        [ForeignKey(nameof(Career))] public long? IdCareer { get; internal set; }
        public virtual Career? Career { get; internal set; }
        [InverseProperty(nameof(Group.Users))] public virtual ICollection<Group> Groups { get; internal set; } = new List<Group>();
        [ForeignKey(nameof(Supervisor))] public long? IdSupervisor { get; internal set; }
        public virtual User? Supervisor { get; internal set; }


        //[NotMapped] public Classification? Classification { get; internal set; }

        public User() { }

        public User(string name)
        {
            Name = name;
        }

    }
}