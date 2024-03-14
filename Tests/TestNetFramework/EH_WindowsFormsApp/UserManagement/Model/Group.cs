using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiegoPiov.UserManagement
{
    [Table("TB_GROUPS")]
    public class Group
    {
        /// <summary>
        /// Group identifier. E.g.: 1
        /// </summary>
        [Key()]
        public Int64 IdGroup { get; internal set; }

        /// <summary>
        /// Name of the user group. E.g.: "Developers"
        /// </summary>
        //[Required][MaxLength(30)][Index(IsUnique = true)] public string Name { get; internal set;
        [Required][MaxLength(30)] public string Name { get; internal set; }

        /// <summary>
        /// E.g.: "Group with developers of the DiegoPiov application."
        /// </summary>
        [Required] public string Description { get; internal set; }

        /// <summary>
        /// Possible observation regarding the group. E.g.: "All users in this group must have a level 11 position or higher" OR "All users can belong to this group."
        /// </summary>
        public string Observation { get; internal set; }

        /// <summary>
        /// Minimum position required for a user to be added to the group (optional restriction)
        /// </summary>
        [ForeignKey(nameof(MinimumPosition))] public Int64? IdMinimumPosition { get; internal set; }
        public virtual Career MinimumPosition { get; internal set; }

        ///// <summary>
        ///// Origin that the members of this group should belong to (optional restriction)
        ///// </summary>         
        //[ForeignKey(nameof(Origin))] public Int64? IdOrigin { get; internal set; }
        //public virtual Origin Origin { get; internal set; }

        [Required] public bool Active { get; internal set; }

        /// <summary>
        /// Permissions regarding the application's functionalities
        /// </summary>
        //[InverseProperty("UserGroup")] public virtual IEnumerable<GroupPermission> Permissions { get; internal set; }
        //public virtual IEnumerable<Permission> Permissions { get; internal set; }

        //[InverseProperty("Groups")] public virtual IEnumerable<Permission> Permissions { get; internal set; }


        public override string ToString()
        {
            return Name;
        }
    }
}
