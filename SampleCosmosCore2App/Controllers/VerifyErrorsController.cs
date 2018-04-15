using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleCosmosCore2App.Models.Shared;

namespace SampleCosmosCore2App.Controllers
{
    [Route("")]
    public class VerifyErrorsController : Controller
    {
        [Authorize(Policy = "APIAccessOnly")]
        [HttpGet("verify/api/ok")]
        public IActionResult GetApiOk()
        {
            return Ok(new VerifyModel()
            {
                SampleNumber = 42,
                SampleString = "Hello"
            });
        }

        [Authorize(Policy = "APIAccessOnly")]
        [HttpGet("verify/api/exception")]
        public IActionResult GetApiException()
        {
            int[] x = null;
            return Ok(x.Length);
        }

        [Authorize(Policy = "APIAccessOnly")]
        [HttpPost("verify/api/400")]
        public IActionResult PostApiBadRequest(VerifyModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [Authorize(Policy = "APIAccessOnly")]
        [HttpGet("verify/api/404")]
        public IActionResult GetApiNotFound()
        {
            return NotFound(new ApiError(404, "Sample thing not found"));
        }

        [Authorize]
        [HttpGet("verify/exception")]
        public IActionResult GetException()
        {
            int[] x = null;
            return View("Ok", x.Length);
        }

        [Authorize]
        [HttpGet("verify/404")]
        public IActionResult GetNotFound()
        {
            Response.StatusCode = 404;
            return View("Custom404");
        }

    }

    public class VerifyModel
    {
        public VerifyModel() { }

        [Required]
        [StringLength(maximumLength: 10, MinimumLength = 3)]
        public string SampleString { get; set; }

        [Required]
        public int SampleNumber { get; set; }

    }
}