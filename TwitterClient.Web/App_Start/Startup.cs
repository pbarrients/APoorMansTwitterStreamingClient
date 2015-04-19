using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using TwitterClient.Infrastructure.Config;
using TwitterClient.Infrastructure.Utility;

[assembly: OwinStartup(typeof(TwitterClient.Web.App_Start.Startup))]

namespace TwitterClient.Web.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            app.MapSignalR();
        }
    }
}
