using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using TranslateGPT.DTOs;
using TranslateGPT.Models;

namespace TranslateGPT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly List<string> mostUsedLanguages = new List<string>()
        {
            "English",
            "Mandarin Chinese",
            "Spanish",
            "Hindi",
            "Arabic",
            "Bengali",
            "Portuguese",
            "Russian",
            "Japanese",
            "French",
            "German",
            "Urdu",
            "Italian",
            "Indonesian",
            "Vietnamese",
            "Turkish",
            "Korean",
            "Tamil",
            "Albanian"
        };

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            ViewBag.Languages = new SelectList(mostUsedLanguages);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OpenAIGPT(string query, string selectedLanguage)
        {
            var openAPIKey = _configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAPIKey}");

            //Define the request payload
            var payload = new
            {
                model = "gpt-4",
                messages = new object[]
                {
                    new { role = "system", content = $"Translate to {selectedLanguage}"},
                    new {role = "user", content = query}
                },
                temperature = 0,
                max_tokens = 256
            };
            string jsonPayload = JsonConvert.SerializeObject(payload);
            HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            //Send the request
            var responseMessage = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);
            var responseMessageJson = await responseMessage.Content.ReadAsStringAsync();

            //Return a response
            var response = JsonConvert.DeserializeObject<OpenAIResponse>(responseMessageJson);

            ViewBag.Result = response.Choices[0].Message.Content;
            ViewBag.Languages = new SelectList(mostUsedLanguages);

            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}