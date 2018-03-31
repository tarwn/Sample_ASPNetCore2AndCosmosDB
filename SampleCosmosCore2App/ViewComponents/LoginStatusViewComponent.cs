using Microsoft.AspNetCore.Mvc;
using SampleCosmosCore2App.Membership;
using SampleCosmosCore2App.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.ViewComponents
{
    // based on: https://andrewlock.net/an-introduction-to-viewcomponents-a-login-status-view-component/

    public class LoginStatusViewComponent : ViewComponent
    {
        private ICustomMembership _membership;

        public LoginStatusViewComponent(ICustomMembership membership)
        {
            _membership = membership;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (await _membership.ValidateLoginAsync(HttpContext.User))
            {
                var sessionDetails = await _membership.GetSessionDetailsAsync(HttpContext.User);
                var session = new SessionDetailsModel()
                {
                    Id = sessionDetails.Id,
                    CreationTime = sessionDetails.CreationTime,
                    Username = sessionDetails.User.Username
                };
                return View("LoggedIn", session);
            }
            else
            {
                return View();
            }
        }
    }
}
