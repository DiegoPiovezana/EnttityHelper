﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entitities
{
    [Table("TB_CAREERS")]
    public class Career 
    {
        [Key()] public Int64 IdCareer { get; set; }     
        [Required][MaxLength(200)] public string Name { get; set; }   
        [Required] public double CareerLevel { get; internal set; } 
        [Required] public bool Active { get; set; }



        public Career() { }

        public Career(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
       

    }
}
