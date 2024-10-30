using API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI.Chat;
using Settings;
using System.Diagnostics;
using System.Text;
using Models;

namespace Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private LLM LLM;

        public HomeController(ILogger<HomeController> logger, IOptions<Settings.OAIConfig> oaiConfig)
        {
            _logger = logger;
            LLM = new LLM(oaiConfig);
        }

        public async Task<IActionResult> ChatGPT()
        {   

            string completion = await LLM.CallChatGPT("Say 'this is a test.'");

            ViewData["Message"] = "ChatGPT Message:" + completion;
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


public partial class ChatExamples
{
    public void Example01_SimpleChat()
    {
        
    }
}