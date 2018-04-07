using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SampleCosmosCore2App.Core;
using SampleCosmosCore2App.Core.Users;
using SampleCosmosCore2App.Membership;
using SampleCosmosCore2App.Models.User;

namespace SampleCosmosCore2App.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private ICustomMembership _membership;
        private Persistence _persistence;

        public UserController(ICustomMembership membership, Persistence persistence)
        {
            _membership = membership;
            _persistence = persistence;
        }

        [HttpGet("index")]
        public async Task<IActionResult> IndexAsync()
        {
            var sessionId = _membership.GetSessionId(HttpContext.User);
            var user = await _persistence.Users.GetUserBySessionIdAsync(sessionId);
            var auths = await _persistence.Users.GetUserAuthenticationsAsync(user.Id);

            var groupedAuths = auths.GroupBy(ua => ua.Scheme.ToString(), ua => new UserAuthenticationModel()
                {
                    Id = ua.Id,
                    Identity = ua.Identity,
                    Name = ua.Name,
                    CreationTime = ua.CreationTime
                })
                .ToDictionary(g => g.Key, g => g.ToList());

            var model = new UserIndexModel()
            {
                User = new UserModel()
                {
                    Id = user.Id,
                    Username = user.Username,
                    CreationTime = user.CreationTime
                },
                UserAuthentications = groupedAuths
            };

            return View("Index", model);
        }

        // YOU ARE HERE: 
        //  - add a view for AddKey
        //  - finish functions and add view for PostAddKeyAsync
        //  - test view for IndexAsync
        //  - maybe dump db if new creationtime fields are junky?
        //  - add "My Account" link to viewcomponent

        [HttpGet("addKey")]
        public IActionResult AddKey()
        {
            var model = new NewKeyModel();
            return View("AddKey", model);
        }

        [HttpPost("addKey")]
        public async Task<IActionResult> PostAddKeyAsync(NewKeyModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AddKey", model);
            }

            var sessionId = _membership.GetSessionId(HttpContext.User);
            var user = await _persistence.Users.GetUserBySessionIdAsync(sessionId);

            var generatedKey = _membership.GenerateAPIKey(user.Id);
            var result = await _membership.AddAuthenticationAsync(user.Id, "APIKey", generatedKey, model.Name);
            var resultModel = new UserAuthenticationModel()
            {
                Id = result.Id,
                Identity = result.Identity,
                Name = result.Name,
                CreationTime = result.CreationTime
            };

            return View("ShowKey", resultModel);
        }
    }
}