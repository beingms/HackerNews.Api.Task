using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace HackerNewApi.BestStories.Controllers
{

    public class Story
    {
        public Story()
        {

        }
        public string By { get; set; }
        public int Descendants { get; set; }
        public int Id { get; set; }
        public List<int> Kids { get; set; }
        public int Score { get; set; }
        public int Time { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            HackerNewsApiClient client = new HackerNewsApiClient();
            var result = await client.GetBestStoriesIdsAsync();
            return new JsonResult(result.OrderByDescending(x=>x.Score).ToList());
        }
     
      

    }
}
