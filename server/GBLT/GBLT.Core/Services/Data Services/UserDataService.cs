using AutoMapper;
using Core.Entity;
using Core.Specification;
using Microsoft.AspNetCore.Identity;
using Shared.Network;

namespace Core.Service
{
    public class UserDataService : IUserDataService
    {
        private readonly UserManager<TIdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IRepository<TUser> _userRepository;
        private readonly IRedisDataService _redisDataService;

        public UserDataService(
            UserManager<TIdentityUser> userManager,
            IMapper mapper,
            IRepository<TUser> userRepository,
            IRedisDataService redisDataService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
            _redisDataService = redisDataService;
        }

        public async Task<GeneralResponse> Create(RegisterRequest message)
        {
            var identityUser = new TIdentityUser { Email = message.Email, UserName = message.Username };
            var identityResult = await _userManager.CreateAsync(identityUser, message.Password);

            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(identityUser, message.Role);
                var user = new TUser { IdentityId = identityUser.Id, Username = identityUser.UserName };
                await _userRepository.AddAsync(user);
            }

            return new GeneralResponse { Success = identityResult.Succeeded, Message = identityResult.Succeeded ? null : identityResult.Errors.First().Description };
        }

        public async Task<TUser> Find(string identityId)
        {
            TIdentityUser identityUser = await _userManager.FindByIdAsync(identityId);
            return identityUser == null ? null : _mapper.Map(identityUser, await _userRepository.SingleOrDefaultAsync(new UserSpecification(identityUser.Id)));
        }

        public async Task<TUser> FindByName(string userName)
        {
            TIdentityUser identityUser = await _userManager.FindByNameAsync(userName);
            return identityUser == null ? null : _mapper.Map(identityUser, await _userRepository.SingleOrDefaultAsync(new UserSpecification(identityUser.Id)));
        }

        public async Task<string[]> GetUserRoles(string identityId)
        {
            TIdentityUser identityUser = await _userManager.FindByIdAsync(identityId);
            string[] roles = (await _userManager.GetRolesAsync(identityUser)).ToArray();
            return roles;
        }

        public async Task<bool> CheckPassword(TUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(_mapper.Map<TIdentityUser>(user), password);
        }

        public async Task Update(TUser user)
        {
            await _userRepository.UpdateAsync(user);
        }
    }
}