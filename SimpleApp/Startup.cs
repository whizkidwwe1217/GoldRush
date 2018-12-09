using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoldRush.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SimpleApp
{
    public class Startup : NewStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override bool UseDeveloperException => false;
        protected override bool UseSignalR => false;
        protected override bool UseSpa => false;
    }
}
