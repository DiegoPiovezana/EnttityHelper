using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiegoPiov.UserManagement
{
    [Table("TB_TICKET")]
    public class Ticket
    {
        [Key] public long? IdLog { get; internal set; }
        [Required] public DateTime DateCreate { get; set; }       
        [ForeignKey(nameof(User))] public long IdUser { get; set; }
        public virtual User User { get; set; } // Not required
        public string Number { get; set; } // In some cases it may be empty
        [ForeignKey(nameof(Responsible))] public long IdResponsible { get; set; }
        public virtual User Responsible { get; set; }
        public string CodeProblem { get; set; }
        [Required] public string Obs { get; set; }
        [Required] public string Before { get; set; }
        [Required] public string After { get; set; }


        public Ticket() { }

        public Ticket(User usuario, User responsavel, string obs, string number, string before, string after)
        {
            DateCreate = DateTime.Now;
            IdUser = usuario.IdUser;
            User = usuario;
            Number = number;
            IdResponsible = responsavel?.IdUser ?? 0;
            Responsible = responsavel;
            Obs = obs;
            Before = before;
            After = after;
        }

        public override string ToString()
        {
            return $"{IdLog}{(string.IsNullOrEmpty(Number) ? "" : $"-{Number}")}";
        }

    }
}