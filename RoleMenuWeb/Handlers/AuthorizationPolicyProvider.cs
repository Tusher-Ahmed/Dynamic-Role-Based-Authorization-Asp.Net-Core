using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace RoleMenuWeb.Handlers
{
    public class AuthorizationPolicyProvider:DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;
        public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
            _options = options.Value;
        }
       
    }
}
