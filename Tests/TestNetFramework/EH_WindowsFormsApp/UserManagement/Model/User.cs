using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiegoPiov.UserManagement
{
    [Table("TB_USERS")]
    public class User
    {
        [Key()] public string Id { get; internal set; }
        [Required][MaxLength(100)] public string Email { get; internal set; }
        [Required][MaxLength(50)] public string Login { get; internal set; }
        public string Name { get; internal set; }
        public bool Active { get; internal set; }
        public DateTime DtCreation { get; internal set; }
        public DateTime? DtLastLogin { get; internal set; }
        public DateTime? DtActivation { get; internal set; }
        public DateTime? DtDeactivation { get; internal set; }
        public DateTime? DtAlteration { get; internal set; }
        public DateTime? DtRevision { get; internal set; }
        [Required][MaxLength(1)] public string InternalUser { get; internal set; }
        [ForeignKey(nameof(Supervisor))] public string? IdSupervisor { get; internal set; }
        public virtual User? Supervisor { get; internal set; }
        [ForeignKey(nameof(Career))] public long IdCareer { get; internal set; }
        public virtual Career Career { get; internal set; }
        //[ForeignKey(nameof(Group))] public Int64? IdGroup { get; internal set; }
        //public virtual Group Group { get; internal set; }

        [InverseProperty(nameof(Group.Users))] public virtual Group Groups { get; internal set; }


        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(User other)
        {
            if (other == null) return 1;
            return Name.CompareTo(other.Name);
            //return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

    }
}
