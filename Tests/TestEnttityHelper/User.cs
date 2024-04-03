using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEnttityHelper
{
    [Table("TB_USERS")]
    internal class User
    {
        [Key()] public int Id { get; internal set; }
        [Required][MaxLength(300)] public string Name { get; internal set; }
        [Required][MaxLength(100)] public string GitHub { get; internal set; }        
        public DateTime DtCreation { get; internal set; }
    }
}