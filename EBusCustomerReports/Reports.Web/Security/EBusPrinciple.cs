using System.Security.Principal;

namespace Helpers.Security
{
    public class EBusPrinciple :IPrincipal
    {
        public EBusPrinciple(EBusIdentity identity, PrincipleProperties properties)
        {
            this.Identity = identity;
            this.Properties = properties;
        }

        public PrincipleProperties Properties { get; set; }

        public bool IsInRole(string role)
        {
            return true;
        }

        public IIdentity Identity { get; private set; }
    }
}
