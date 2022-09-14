using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        public readonly DataContext _context;

        public readonly IMapper _mapper;



        public FightService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequesDto request)
        {
            var response = new ServiceResponse<FightResultDto>()
            {
                Data = new FightResultDto()
            };
            try
            {
                var characters = await _context.Characters
                     .Include(w => w.Weapon)
                     .Include(s => s.Skills)
                     .Where(c => request.CharactresIds.Contains(c.Id))
                     .ToListAsync();

                bool defated = false;
                while (!defated)
                {
                    foreach (Character attacker in characters)
                    {
                        var oponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        var oponent = oponents[new Random().Next(oponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;

                        bool useWeapon = new Random().Next(2) == 0;
                        if (useWeapon)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, oponent);
                        }
                        else
                        {
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                            damage = DoSkillAttack(attacker, oponent, skill);
                            attackUsed = skill.Name;
                        }
                        response.Data.Log.Add($"{attacker.Name} attacks {oponent.Name} using {attackUsed} with {(damage >= 0 ? damage : 0)} damage.");

                        if (oponent.HitPoints <= 0)
                        {
                            defated = true;
                            attacker.Victories++;
                            oponent.Defeats++;
                            response.Data.Log.Add($"{oponent.Name} has been defeated! ");
                            response.Data.Log.Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");
                            break;
                        }

                    }
                }
                characters.ForEach(c =>
                {
                    c.Fights++;
                    c.HitPoints = 100;
                });

                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                response.Succes = false;
                response.Message = ex.Message;

            }
            return response;
        }
        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                  .Include(w => w.Skills)
                  .FirstOrDefaultAsync(c => c.Id == request.AttackerId);

                var oponent = await _context.Characters
              .Include(w => w.Skills)
              .FirstOrDefaultAsync(c => c.Id == request.OpponenId);

                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);

                if (skill == null)
                {
                    response.Succes = false;
                    response.Message = $"{attacker.Name} dosen't know that skill.";
                    return response;
                }

                int damage = DoSkillAttack(attacker, oponent, skill);

                if (oponent.HitPoints <= 0)
                    response.Message = $"{oponent.Name} HashCode benn defeated!";

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    AttackerName = attacker.Name,
                    Oponont = oponent.Name,
                    AttackerHp = attacker.HitPoints,
                    OponontHp = oponent.HitPoints,
                    Damage = damage
                };

            }
            catch (Exception ex)
            {
                response.Succes = false;
                response.Message = ex.Message;

            }
            return response;
        }
        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                  .Include(w => w.Weapon)
                  .FirstOrDefaultAsync(c => c.Id == request.AttackerId);

                var oponent = await _context.Characters
              .Include(w => w.Weapon)
              .FirstOrDefaultAsync(c => c.Id == request.OpponenId);

                int damage = DoWeaponAttack(attacker, oponent);

                if (oponent.HitPoints <= 0)
                    response.Message = $"{oponent.Name} HashCode benn defeated!";

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    AttackerName = attacker.Name,
                    Oponont = oponent.Name,
                    AttackerHp = attacker.HitPoints,
                    OponontHp = oponent.HitPoints,
                    Damage = damage
                };

            }
            catch (Exception ex)
            {
                response.Succes = false;
                response.Message = ex.Message;

            }
            return response;
        }
        public async Task<ServiceResponse<List<highScoreDto>>> GetHighScores()
        {
            var characters = await _context.Characters
                             .Where(c => c.Fights > 0)
                             .OrderByDescending(c => c.Victories)
                             .ThenBy(c => c.Defeats)
                             .ToListAsync();

            var response = new ServiceResponse<List<highScoreDto>>
            {
                Data =
                characters.Select(c => _mapper.Map<highScoreDto>(c)).ToList()
            };
            return response;
        }

        private static int DoSkillAttack(Character? attacker, Character? oponent, Skill? skill)
        {
            int damage = skill.Damage + (new Random().Next(attacker.intelligence));
            damage -= new Random().Next(oponent.Defense);

            if (damage > 0)
                oponent.HitPoints -= damage;
            return damage;
        }
        private static int DoWeaponAttack(Character? attacker, Character? oponent)
        {
            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(oponent.Defense);

            if (damage > 0)
                oponent.HitPoints -= damage;
            return damage;
        }
    }
}