using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacherService : ICharacherService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessort;


        public CharacherService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessort = httpContextAccessor;
            _context = context;
            _mapper = mapper;

        }

        private int GetUserId() => int.Parse(_httpContextAccessort.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            Character character = _mapper.Map<Character>(newCharacter);

            character.User = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == GetUserId());

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            serviceResponse.Data = await _context.Characters
              .Include(s => s.Skills)
            .Where(u => u.Id == GetUserId())
            .Select(c => _mapper.Map<GetCharacterDto>(c))
            .ToListAsync();
            return serviceResponse;
        }
        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var response = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _context.Characters
            .Where(c => c.User.Id == GetUserId())

    .Include(w => w.Weapon)
                .Include(s => s.Skills)
                .ToListAsync();

            response.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return response;
        }
        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacter = await _context.Characters
                 .Include(w => w.Weapon)
                .Include(s => s.Skills)
            .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<Character, GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto UpdateedCharacter)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();
            try
            {
                var character = await _context.Characters
                .Include(w => w.Weapon)
                .Include(s => s.Skills)
                .FirstOrDefaultAsync(c => c.Id == UpdateedCharacter.Id);
                ;
                if (character.User.Id == GetUserId())
                {
                    _mapper.Map(UpdateedCharacter, character);
                    //   _context.Update(character);
                    response.Data = _mapper.Map<GetCharacterDto>(character);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    response.Succes = false;
                    response.Message = "Character not found Or Not Your Character";
                }


            }
            catch (Exception ex)
            {
                response.Succes = false;
                response.Message = ex.Message;

            }
            return response;


        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            ServiceResponse<List<GetCharacterDto>> response = new ServiceResponse<List<GetCharacterDto>>();
            try
            {
                var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
                if (character != null)
                {
                    _context.Characters.Remove(character);
                    await _context.SaveChangesAsync();

                    response.Data = await _context.Characters
                    .Where(c => c.User.Id == GetUserId())
                    .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();

                }
                else
                {
                    response.Succes = false;
                    response.Message = "Character not found";
                }

            }
            catch (Exception ex)
            {
                response.Succes = false;
                response.Message = ex.Message;

            }
            return response;



        }

        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto addCharacterSkillDto)
        {
            var response = new ServiceResponse<GetCharacterDto>();
            try
            {
                var character = await _context.Characters
                .Include(w => w.Weapon)
                .Include(s => s.Skills)
                 .FirstOrDefaultAsync(c => c.Id == addCharacterSkillDto.CharacterId
                 && c.User.Id == GetUserId());

                if (character == null)
                {
                    response.Succes = false;
                    response.Message = "Character not found";
                    return response;
                }
                var skill = await _context.Skills
                   .Include(c => c.Characters)
                     .FirstOrDefaultAsync(c => c.Id == addCharacterSkillDto.SkillId);
                if (character == null)
                {
                    response.Succes = false;
                    response.Message = "Skill not found";
                    return response;
                }

                character.Skills.Add(skill);
                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterDto>(character);
                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                response.Succes = false;
                response.Message = ex.Message;
            }
            return response;

        }
    }
}