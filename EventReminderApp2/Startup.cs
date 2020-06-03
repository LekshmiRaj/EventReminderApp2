using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EventReminderApp2.Startup))]
namespace EventReminderApp2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
