using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleCosmosCore2App.Controllers;
using SampleCosmosCore2App.Core;
using SampleCosmosCore2App.Membership;
using SampleCosmosCore2App.Notifications;

namespace SampleCosmosCore2App
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                // Add XML Content Negotiation
                options.InputFormatters.Add(new XmlSerializerInputFormatter());
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                // Don't default to JSON when there isn't a good answer
                options.ReturnHttpNotAcceptable = true;
            });

            services.AddSingleton<Persistence>((s) =>
            {
                var p = new Persistence(
                    new Uri(Configuration["CosmosDB:URL"]),
                            Configuration["CosmosDB:PrimaryKey"],
                            Configuration["CosmosDB:DatabaseId"]);
                p.EnsureSetupAsync().Wait();
                return p;
            });

            services.AddScoped<SmtpClient>((s) => {
                if (Configuration["smtp:DeliveryMethod"] == "directory")
                {
                    return new SmtpClient()
                    {
                        DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                        PickupDirectoryLocation = Path.Combine(Environment.ContentRootPath, "..", "mail")
                    };
                }
                else
                {
                    return new SmtpClient(Configuration["smtp:Host"], int.Parse(Configuration["smtp:Port"]))
                    {
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        EnableSsl = bool.Parse(Configuration["smtp:EnableSsl"]),
                        Credentials = new NetworkCredential(Configuration["smtp:Username"], Configuration["smtp:Password"])
                    };
                }
            });

            services.AddEmailErrorNotifier((options) => {
                options.EnvironmentName = Environment.EnvironmentName;
                options.FromAddress = Configuration["EmailAddresses:ErrorsFrom"];
                options.ToAddress = Configuration["EmailAddresses:ErrorsTo"];
            });

            services.AddCustomMembership<CosmosDBMembership>((options) =>
            {
                options.InteractiveAuthenticationType = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultPathAfterLogin = "/";

                options.OneTimeAuthenticationType = "APIToken";
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

            services.AddAuthorization(options =>
            {
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
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            app.UseExceptionHandler("/error");
            //}
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
