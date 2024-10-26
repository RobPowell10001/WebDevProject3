using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Settings;
using System.Diagnostics;
using WebDevProject3.Models;

namespace Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<Settings.OAIConfig> _oai;

        public HomeController(ILogger<HomeController> logger, IOptions<Settings.OAIConfig> oaiConfig)
        {
            _logger = logger;
            _oai = oaiConfig?.Value ?? throw new ArgumentNullException(nameof(oaiConfig));
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChatGPT()
        {
            ChatClient client = new(model: "gpt-35-turbo", apiKey: _oai.Key);

            ChatCompletion completion = client.CompleteChat("Say 'this is a test.'");

            Console.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");
            ViewData["Message"] = "ChatGPT Message:" + completion.Content[0].Text;
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