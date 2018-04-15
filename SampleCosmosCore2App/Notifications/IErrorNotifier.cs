using Microsoft.AspNetCore.Http;
using SampleCosmosCore2App.Models.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Notifications
{
    public interface IErrorNotifier
    {
        Task NotifyAsync(DescriptiveError descriptiveError, Exception exc, string path, ClaimsPrincipal user = null);
    }
}
