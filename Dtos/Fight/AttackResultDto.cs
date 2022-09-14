using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Fight
{
    public class AttackResultDto
    {
        public string AttackerName { get; set; } = string.Empty;
        public string Oponont { get; set; } = string.Empty;

        public int AttackerHp { get; set; }
        public int OponontHp { get; set; }
        public int Damage { get; set; }

    }
}