using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;
using YouTubeApiProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // Display Search Page
        public IActionResult Index()
        {
            return View(new List<YouTubeVideoModel>()); // Pass an empty list initially
        }

        // Handle search query and pagination
        [HttpPost]
        public async Task<IActionResult> Search(string query, string pageToken = null, string duration = "")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.ErrorMessage = "Please enter a search query.";
                return View("Index", new List<YouTubeVideoModel>());
            }

            try
            {
                var (videos, nextPageToken, prevPageToken) = await _youtubeService.SearchVideosAsync(query, pageToken ?? "", duration);

                if (videos.Count == 0)
                {
                    if (prevPageToken == "INVALID_API_KEY")
                    {
                        ViewBag.ErrorMessage = "Failed to connect to YouTube. API key may be invalid or expired.";
                    }
                    else if (prevPageToken == "NO_INTERNET")
                    {
                        ViewBag.ErrorMessage = "Failed to connect to YouTube. Please check your internet connection.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "No videos. Try searching for something else.";
                    }
                }


                ViewBag.Query = query;
                ViewBag.Duration = duration;
                ViewBag.NextPageToken = nextPageToken ?? "";
                ViewBag.PrevPageToken = prevPageToken ?? "";

                return View("Index", videos);
            }
            catch (Google.GoogleApiException ex)
            {
                ViewBag.ErrorMessage = "Failed to connect to YouTube. API key may be invalid or expired.";
                Console.WriteLine($"Google API Error: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = "Failed to connect to YouTube. Please check your internet connection.";
                Console.WriteLine($"API Request Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again.";
                Console.WriteLine($"General Error: {ex.Message}");
            }


            // Kekalkan query supaya user tak perlu taip semula
            ViewBag.Query = query;
            return View("Index", new List<YouTubeVideoModel>());

        }
    }
}
