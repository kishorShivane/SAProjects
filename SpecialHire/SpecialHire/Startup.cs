using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SpecialHire.Startup))]
namespace SpecialHire
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
