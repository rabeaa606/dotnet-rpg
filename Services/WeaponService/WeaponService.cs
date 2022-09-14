using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dotnet_rpg.Services.WeaponService
{
    public class WeaponService : IWeaponService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessort;


        public WeaponService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessort = httpContextAccessor;
            _context = context;
            _mapper = mapper;

        }
        private int GetUserId() => int.Parse(_httpContextAccessort.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWapon)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == newWapon.CharacteId &&
                c.User.Id == GetUserId());

                if (character == null)
                {
                    response.Succes = false;
                    response.Message = "Character not found";
                    return response;
                }
                else
                {
                    var weapon = new Weapon
                    {
                        Name = newWapon.Name,
                        Damage = newWapon.Damage,
                        Character = character
                    };
                    _context.Wepons.Add(weapon);
                    await _context.SaveChangesAsync();
                    response.Data = _mapper.Map<GetCharacterDto>(character);

                }
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