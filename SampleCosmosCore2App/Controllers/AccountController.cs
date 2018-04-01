using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleCosmosCore2App.Core;
using SampleCosmosCore2App.Membership;
using SampleCosmosCore2App.Models.Account;

namespace SampleCosmosCore2App.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private ICustomMembership _membership;

        public AccountController(ICustomMembership membership)
        {
            _membership = membership;
        }

        [HttpGet("register")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPostAsync(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register");
            }

            var result = await _membership.RegisterAsync(model.UserName, model.Email, model.Password);
            if (result.Failed)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View("Register", model);
            }

            return LocalRedirect(_membership.Options.DefaultPathAfterLogin);
        }

        [HttpGet("register/twitter")]
        [AllowAnonymous]
        public IActionResult RegisterWithTwitter()
        {
            var props = new AuthenticationProperties()
            {
                RedirectUri = "account/register/twitter/continue"
            };
            return Challenge(props, "Twitter");
        }

        [HttpGet("register/twitter/continue")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterWithTwitterContinueAsync()
        {
            // use twitter info to set some sensible defaults
            var cookie = await HttpContext.AuthenticateAsync("ExternalCookie");
            var twitterId = cookie.Principal.FindFirst("urn:twitter:userid");
            var twitterUsername = cookie.Principal.FindFirst("urn:twitter:screenname");
            var suggestedUsername = await FindUniqueSuggestionAsync(twitterUsername.Value);

            var model = new RegisterWithTwitterModel() {
                TwitterId = twitterId.Value,
                TwitterUsername = twitterUsername.Value,
                UserName = suggestedUsername
            };

            return View("RegisterWithTwitterContinue", model);
        }

        private async Task<string> FindUniqueSuggestionAsync(string startingName, int numAttempts = 5)
        {
            string suggestedUsername = startingName;
            var rand = new Random();
            for (int attemptCount = 0; attemptCount < numAttempts; attemptCount++)
            {
                if (await _membership.IsUsernameAvailable(suggestedUsername))
                {
                    return suggestedUsername;
                }
                else
                {
                    suggestedUsername = $"{startingName}{Math.Floor(rand.NextDouble() * 10000)}";
                }
            }
            return "";
        }

        [HttpPost("register/twitter/continue")]
        public async Task<IActionResult> RegisterWithTwitterContinueAsync(RegisterWithTwitterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("RegisterWithTwitterContinue", model);
            }

            var result = await _membership.RegisterExternalAsync(model.UserName, model.Email, "Twitter", model.TwitterId);
            if (result.Failed)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View("RegisterWithTwitterContinue", model);
            }

            await HttpContext.SignOutAsync("ExternalCookie");

            return LocalRedirect(_membership.Options.DefaultPathAfterLogin);
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(string returnUrl = null)
        {
            // ensure we're starting w/ a clean slate
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("ExternalCookie");

            TempData["returnUrl"] = returnUrl;
            return View("Login");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPostAsync(LoginModel user, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Username or password is incorrect");
                return View("Login", user);
            }

            var result = await _membership.LoginAsync(user.UserName, user.Password);
            if (result.Failed)
            {
                ModelState.AddModelError("", "Username or password is incorrect");
                return View("Login", user);
            }

            return LocalRedirect(returnUrl ?? _membership.Options.DefaultPathAfterLogin);
        }

        [HttpGet("login/twitter")]
        [AllowAnonymous]
        public IActionResult LoginWithTwitter(string returnUrl = null)
        {
            var props = new AuthenticationProperties()
            {
                RedirectUri = "account/login/twitter/continue?returnUrl=" + HttpUtility.UrlEncode(returnUrl)
            };
            return Challenge(props, "Twitter");
        }

        [HttpGet("login/twitter/continue")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithTwitterContinueAsync(string returnUrl = null)
        {
            // use twitter info to create a session
            var cookie = await HttpContext.AuthenticateAsync("ExternalCookie");
            var twitterId = cookie.Principal.FindFirst("urn:twitter:userid");

            var result = await _membership.LoginExternalAsync("Twitter", twitterId.Value);
            if (result.Failed)
            {
                ModelState.AddModelError("", "Twitter account not recognized, have you registered yet?");
                return View("Login");
            }

            await HttpContext.SignOutAsync("ExternalCookie");

            return LocalRedirect(returnUrl ?? _membership.Options.DefaultPathAfterLogin);
        }


        [HttpGet("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> LogoutAsync()
        {
            await _membership.LogoutAsync();

            if (_membership.Options.DefaultPathAfterLogout != null)
            {
                return Redirect(_membership.Options.DefaultPathAfterLogout);
            }
            else
            {
                return RedirectToAction("LoginAsync");
            }
        }

        [HttpGet("protected")]
        [Authorize]
        public async Task<IActionResult> Protected()
        {
            var session = await _membership.GetSessionDetailsAsync(HttpContext.User);
            return View("Protected", new { UserId = session.User.Id, UserName = session.User.Username, SessionId = session.Id, SessionCreated = session.CreationTime });
        }
    }





}