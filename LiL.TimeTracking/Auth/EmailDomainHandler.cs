using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LiL.TimeTracking.Auth;

public class EmailDomainHandler:AuthorizationHandler<EmailDomainRequirement>
{
    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailDomainRequirement requirement)
    {
        // requirement tells us what the details are or what the config is or what it needs to be
        // context gives us the claims in the token about what it is
        // and then we can do our checks and find out if set of claims that come from the client meet the requirement.
        var emailClaim = context.User.FindFirst(c=>c.Type == ClaimTypes.Email);
        if(emailClaim is null){
            return;
        }
        if(emailClaim.Value.EndsWith(requirement.Domain, StringComparison.InvariantCultureIgnoreCase)){
            context.Succeed(requirement);
        }
        return;
    }
}