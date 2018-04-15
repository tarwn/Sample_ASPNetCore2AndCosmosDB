using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Notifications
{
    public static class EmailErrorNotifierExtensions
    {
        public static IServiceCollection AddEmailErrorNotifier(this IServiceCollection services, Action<EmailErrorNotifierOptions> options)
        {
            services.AddScoped<IErrorNotifier, EmailErrorNotifier>();
            services.AddScoped<EmailErrorNotifierOptions>((s) => {
                var opts = new EmailErrorNotifierOptions();
                options(opts);
                return opts;
            });

            return services;
        }
    }
}
