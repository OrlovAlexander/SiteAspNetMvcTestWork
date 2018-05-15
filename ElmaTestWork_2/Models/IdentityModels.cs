using Microsoft.AspNet.Identity;
using NHibernate.AspNet.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ElmaTestWork_2.Models
{
    public class ApplicationUser : IdentityUser
    {
        //public int Age { get; set; }
        //public string Gender { get; set; }

        public async virtual Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            ClaimsIdentity userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            //userIdentity.AddClaim(new Claim(ClaimTypes.Gender, this.Gender));
            //userIdentity.AddClaim(new Claim("age", this.Age.ToString()));
            return userIdentity;
        }
    }
}