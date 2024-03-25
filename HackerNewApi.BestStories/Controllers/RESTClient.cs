using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HackerNewApi.BestStories.Controllers;
using Newtonsoft.Json;

public class HackerNewsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore;

    public HackerNewsApiClient()
    {
        var httpClientHandler = new HttpClientHandler
        {
            MaxConnectionsPerServer = 10 // Limiting to 10 concurrent connections per server
        };

        _httpClient = new HttpClient(httpClientHandler);
        _semaphore = new SemaphoreSlim(10); //  Limiting to 10 concurrent requests
    }

    public async Task<List<Story>> GetBestStoriesIdsAsync()
    {
        List<Story> storyIds = new List<Story>();

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("https://hacker-news.firebaseio.com/v0/beststories.json");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                string[] splitIds = responseBody.Trim('[', ']').Split(',');

                // Create tasks for fetching stories
                Task[] tasks = new Task[splitIds.Length];
//for (int i = 0; i < splitIds.Length; i++)
              //  {
                    int counter = 0;
                    splitIds.AsParallel().ForAll(x =>
                    {
                        int i = Interlocked.Increment(ref counter) - 1;
                        string id = splitIds[i].Trim();
                        tasks[i] = FetchStoryAsync(id, storyIds);

                    });
           
               // }

                // Await all tasks to complete
                await Task.WhenAll(tasks);
                return storyIds;
            }
            else
            {
                Console.WriteLine($"Failed to fetch best stories IDs. Status code: {response.StatusCode}");
            }

           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while fetching best stories IDs: {ex.Message}");

        }
        return storyIds;
    }

    private async Task FetchStoryAsync(string storyId, List<Story> storyIds)
    {
        try
        {
            // Acquire semaphore to limit concurrent requests
            await _semaphore.WaitAsync();

            HttpResponseMessage response = await _httpClient.GetAsync($"https://hacker-news.firebaseio.com/v0/item/{storyId}.json");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                // Process story data as needed
                var story = JsonConvert.DeserializeObject<Story>(responseBody);
                if (story != null)
                    storyIds.Add(story);
            }
            else
            {
                Console.WriteLine($"Failed to fetch story {storyId}. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while fetching story {storyId}: {ex.Message}");
        }
        finally
        {
            // Release semaphore after request completes
            _semaphore.Release();
        }
    }

}
