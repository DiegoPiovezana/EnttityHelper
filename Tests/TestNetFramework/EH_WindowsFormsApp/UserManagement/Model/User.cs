using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiegoPiov.UserManagement
{
    [Table("TB_USERS")]
    public class User
    {
        [Key] public long IdUser { get; internal set; }
        [Required] public string Name { get; internal set; }
        [Required][MaxLength(100)] public string Email { get; internal set; }
        [MaxLength(50)] public string Registration { get; internal set; }
        [Required] public bool Active { get; internal set; }
        [Required] public DateTime DtCreate { get; internal set; }
        public DateTime? DtLastLogin { get; internal set; }
        public DateTime? DtModified { get; internal set; }
        public DateTime? DtRevision { get; internal set; }
        [Required][MaxLength(1)] public string InternalUser { get; internal set; } // Y or N
        [InverseProperty("OriginUsers")] public virtual ICollection<Origin> Origins { get; internal set; }
        [ForeignKey(nameof(Career))] public long IdCareer { get; internal set; }
        public virtual Career Career { get; internal set; }
        [ForeignKey(nameof(Group))] public long? IdGroup { get; internal set; } = null;
        public virtual Group Group { get; internal set; }
        [InverseProperty(nameof(Supervision.Supervised))] public virtual ICollection<Supervision> SupervisionGroups { get; set; } = new List<Supervision>();
        [Required][ForeignKey(nameof(CreationTicket))] public Int64? IdCreationTicket { get; internal set; }
        public virtual Ticket CreationTicket { get; internal set; }

        public override string ToString() // EID
        {
            return !string.IsNullOrEmpty(Email) ? Email.Split('@')[0] : "ABSENT";
        }


        public override bool Equals(object obj)
        {
            if (obj is User user)
            {
                return IdUser.Equals(user.IdUser);
            }
            else
            {
                return base.Equals(obj);
            }
        }

    }
}