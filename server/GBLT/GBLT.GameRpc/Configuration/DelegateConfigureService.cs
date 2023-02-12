using Core.Entity;
using Core.Service;

namespace RpcService.Configuration
{
    public delegate ILoginService LoginServiceResolver(AccountType type);
}