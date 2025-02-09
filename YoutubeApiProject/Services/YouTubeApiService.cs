using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
        }

        public async Task<(List<YouTubeVideoModel> videos, string nextPageToken, string prevPageToken)> SearchVideosAsync(string query, string pageToken = null, string duration = "")
        {
            try
            {
                Console.WriteLine($"[DEBUG] Searching: {query}, PageToken: {pageToken}, Duration: {duration}");

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = _apiKey,
                    ApplicationName = "YoutubeProject"
                });

                var searchRequest = youtubeService.Search.List("snippet");
                searchRequest.Q = query;
                searchRequest.MaxResults = 10;
                searchRequest.Type = "video"; // Hanya ambil video

                if (!string.IsNullOrEmpty(pageToken))
                {
                    searchRequest.PageToken = pageToken;
                }

                // Convert string ke VideoDurationEnum
                if (!string.IsNullOrEmpty(duration))
                {
                    if (Enum.TryParse(duration, true, out SearchResource.ListRequest.VideoDurationEnum videoDuration))
                    {
                        searchRequest.VideoDuration = videoDuration;
                    }
                }

                var searchResponse = await searchRequest.ExecuteAsync();

                var videos = searchResponse.Items
                    .Where(item => item.Id.Kind == "youtube#video")
                    .Select(item => new YouTubeVideoModel
                    {
                        Title = item.Snippet.Title,
                        Description = item.Snippet.Description,
                        ThumbnailUrl = item.Snippet.Thumbnails.Medium?.Url,
                        VideoUrl = "https://www.youtube.com/watch?v=" + item.Id.VideoId,
                        PublishedDate = item.Snippet.PublishedAt?.ToString("MMMM, yyyy"),
                        ChannelName = item.Snippet.ChannelTitle
                    })
                    .ToList();

                // Filter tambahan: pastikan tajuk ada keyword dari query
                videos = videos.Where(v => v.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                Console.WriteLine($"[DEBUG] Found {videos.Count} videos.");

                if (videos.Count == 0)
                {
                    Console.WriteLine("[DEBUG] Filtered all videos out, returning empty list.");
                    return (new List<YouTubeVideoModel>(), "", "");
                }

                Console.WriteLine($"[DEBUG] NextPageToken: {searchResponse.NextPageToken}, PrevPageToken: {searchResponse.PrevPageToken}");

                return (videos, searchResponse.NextPageToken ?? "", searchResponse.PrevPageToken ?? "");
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.BadRequest && ex.Message.Contains("API key not valid"))
                {
                    Console.WriteLine("[ERROR] Invalid API Key.");
                    return (new List<YouTubeVideoModel>(), "", "INVALID_API_KEY");
                }

                Console.WriteLine($"[ERROR] Google API Error: {ex.Message}");
                return (new List<YouTubeVideoModel>(), "", "");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Connection Error: {ex.Message}");
                return (new List<YouTubeVideoModel>(), "", "NO_INTERNET");
            }


        }
    }
}
