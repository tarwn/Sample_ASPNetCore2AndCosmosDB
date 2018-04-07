using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleCosmosCore2App.Controllers;
using SampleCosmosCore2App.Core;
using SampleCosmosCore2App.Membership;

namespace SampleCosmosCore2App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<Persistence>((s) =>
            {
                var p = new Persistence(
                    new Uri(Configuration["CosmosDB:URL"]),
                            Configuration["CosmosDB:PrimaryKey"],
                            Configuration["CosmosDB:DatabaseId"]);
                p.EnsureSetupAsync().Wait();
                return p;
            });

            services.AddCustomMembership<CosmosDBMembership>((options) =>
            {
                options.AuthenticationType = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultPathAfterLogin = "/";
                //options.DefaultPathAfterLogout = "/account/login";
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                /* Custom Membership API Provider */
                .AddCustomMembershipAPIAuth("APIToken", "SampleCosmosCore2App")
                /* External Auth Providers */
                .AddCookie("ExternalCookie")
                .AddTwitter("Twitter", options =>
                {
                    options.SignInScheme = "ExternalCookie";

                    options.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                    options.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                })
                /* 'Session' Cookie Provider */
                .AddCookie((options) =>
                {
                    options.LoginPath = new PathString("/account/login");
                    options.LogoutPath = new PathString("/account/logout");
                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnValidatePrincipal = async (c) =>
                        {
                            var membership = c.HttpContext.RequestServices.GetRequiredService<ICustomMembership>();
                            var isValid = await membership.ValidateLoginAsync(c.Principal);
                            if (!isValid)
                            {
                                c.RejectPrincipal();
                            }
                        }
                    };
                });

            services.AddAuthorization(options => {
                options.AddPolicy("APIAccessOnly", policy =>
                {
                    policy.AddAuthenticationSchemes("APIToken");
                    policy.RequireAuthenticatedUser();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
