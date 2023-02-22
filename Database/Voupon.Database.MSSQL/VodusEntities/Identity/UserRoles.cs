using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class UserRoles : IdentityUserRole<string>
    {
    }

    public partial class UserClaims : IdentityUserClaim<string>
    {
    }

    public partial class UserLogins : IdentityUserLogin<string>
    {
    }

    public partial class UserTokens : IdentityUserToken<string>
    {
    }

    public partial class RoleClaims : IdentityRoleClaim<string>
    {
    }
}
