using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Services.FightService;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FightController : ControllerBase
    {

        public readonly IFightService _fightsService;

        public FightController(IFightService fightsService)
        {
            _fightsService = fightsService;

        }
        [HttpPost("Weapon")]
        public async Task<ActionResult<ServiceResponse<List<AttackResultDto>>>> WeaponAttack(WeaponAttackDto request)
        {
            return Ok(await _fightsService.WeaponAttack(request));
        }
        [HttpPost("Skill")]
        public async Task<ActionResult<ServiceResponse<List<AttackResultDto>>>> SkillAttack(SkillAttackDto request)
        {
            return Ok(await _fightsService.SkillAttack(request));
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<FightResultDto>>> Fight(FightRequesDto request)
        {
            return Ok(await _fightsService.Fight(request));
        }
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<highScoreDto>>>> GetHighScores()
        {
            return Ok(await _fightsService.GetHighScores());
        }
    }
}