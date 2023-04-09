using AutoMapper;
using LMS.Server.Core.Domain;
using LMS.Server.Core.Dto;
using LMS.Server.Core.Interfaces;
using LMS.Server.Core.Specifications;
using LMS.Server.Infrastructure.Data;
using LMS.Server.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Server.Infrastructure.Repositories
{
    public sealed class UserRepository : EfRepository<TUser>, IUserRepository
    {
        private readonly UserManager<TIdentityUser> _userManager;
        private readonly IMapper _mapper;

        public UserRepository(UserManager<TIdentityUser> userManager, IMapper mapper, AppDbContext appDbContext) : base(appDbContext)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<CreateUserResponse> Create(RegisterRequest message)
        {
            var identityUser = new TIdentityUser { Email = message.Email, UserName = message.UserName };
            var identityResult = await _userManager.CreateAsync(identityUser, message.Password);

            if (!identityResult.Succeeded) return new CreateUserResponse(identityUser.Id, false, identityResult.Errors.Select(e => new ErrorMessage(e.Code, e.Description)));

            await _userManager.AddToRoleAsync(identityUser, message.Role);
            var user = new TUser(message.FirstName, message.LastName, identityUser.Id, identityUser.UserName);
            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();

            return new CreateUserResponse(identityUser.Id, identityResult.Succeeded, identityResult.Succeeded ? null : identityResult.Errors.Select(e => new ErrorMessage(e.Code, e.Description)));
        }

        public async Task<TUser> FindByName(string userName)
        {
            TIdentityUser identityUser = await _userManager.FindByNameAsync(userName);
            return identityUser == null ? null : _mapper.Map(identityUser, await GetSingleBySpec(new UserSpecification(identityUser.Id)));
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
    }
}