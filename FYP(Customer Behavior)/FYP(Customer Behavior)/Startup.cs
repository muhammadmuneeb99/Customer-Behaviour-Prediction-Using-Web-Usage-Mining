using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FYP_Customer_Behavior_.Startup))]
namespace FYP_Customer_Behavior_
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
