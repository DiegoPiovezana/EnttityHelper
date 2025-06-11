using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entities
{
    [Table("TB_TICKET")]
    public class Ticket
    {
        [Key] public long? IdLog { get; internal set; }
        [Required] public DateTime? DateCreate { get; set; }
        [ForeignKey(nameof(User))] public long? IdUser { get; set; }
        public virtual User User { get; set; }
        public string Number { get; set; }
        [Required] public string Obs { get; set; }
        [Required] public string Previous { get; set; }
        [Required] public string After { get; set; }

        public Ticket() { }

        public Ticket(User? user, string obs, string num, string previous, string after)
        {
            DateCreate = DateTime.Now;
            IdUser = user?.Id;
            User = user;
            Number = num;
            Obs = obs; Previous = previous;
            After = after;
        }

        public override string ToString()
        {
            return $"{IdLog}{(string.IsNullOrEmpty(Number) ? "" : $"-{Number}")}";
        }
    }
}