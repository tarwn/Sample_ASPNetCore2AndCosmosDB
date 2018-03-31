using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleCosmosCore2App.Core;
using SampleCosmosCore2App.Core.Samples;

namespace SampleCosmosCore2App.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private Persistence _persistence;

        public HomeController(Persistence persistence)
        {
            _persistence = persistence;
        }

        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> IndexAsync()
        {
            var samples = await _persistence.Samples.GetSamplesAsync();
            return View("Index", samples);
        }

        [HttpGet("Create")]
        [Authorize]
        public IActionResult Create()
        {
            var sample = new Sample() { };
            return View("Get", sample);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAsync(string id)
        {
            var sample = await _persistence.Samples.GetSampleAsync(id);
            return View("Get", sample);
        }

        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> PostAsync([FromForm] Sample sample)
        {
            await _persistence.Samples.SaveSampleAsync(sample);
            return RedirectToAction("IndexAsync");
        }
    }
}