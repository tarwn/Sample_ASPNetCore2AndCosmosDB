using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Membership
{
    public static class CustomMembershipExtensions
    {
        public static IServiceCollection AddCustomMembership<T>(this IServiceCollection services, Action<CustomMembershipOptions> options)
            where T : class, ICustomMembership
        {
            services.AddTransient<ICustomMembership, T>();
            services.AddTransient<CustomMembershipOptions>((s) => {
                var opts = new CustomMembershipOptions();
                options(opts);
                return opts;
            });

            return services;
        }
    }
}
