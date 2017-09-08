using System;
using System.Security.Principal;
using System.Web.Security;

namespace Helpers.Security
{
    public class EBusIdentity : IIdentity
    {
        private readonly FormsAuthenticationTicket _ticket;

        public EBusIdentity(FormsAuthenticationTicket ticket)
        {
            _ticket = ticket;
        }

        public string Name
        {
            get { return _ticket.Name; }
        }

        public string AuthenticationType
        {
            get { return "eBus"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public FormsAuthenticationTicket Ticket
        {
            get { return _ticket; }
        }
    }
}