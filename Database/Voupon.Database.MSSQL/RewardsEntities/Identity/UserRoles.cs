using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class UserRoles : IdentityUserRole<Guid>
    {
    }

    public partial class UserClaims : IdentityUserClaim<Guid>
    {
    }

    public partial class UserLogins : IdentityUserLogin<Guid>
    {
    }

    public partial class UserTokens : IdentityUserToken<Guid>
    {
    }

    public partial class RoleClaims : IdentityRoleClaim<Guid>
    {
    }
}
