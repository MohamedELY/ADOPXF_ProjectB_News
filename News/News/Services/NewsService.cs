//#define UseNewsApiSample  // Remove or undefine to use your own code to read live data

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; //Requires nuget package System.Net.Http.Json
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using News.Models;

namespace News.Services
{
    public class NewsService
    {
        readonly string apiKey = "faaeddb19bf843498b06c6ad5ee94edf";
        HttpClient httpClient = new();
        //part of your event code here      
        //public event EventHandler<string> NewsAvailable;
        private ConcurrentDictionary<(DateTime, NewsCategory), NewsGroup> _cachedNews = new();

        public async Task<NewsGroup> GetNewsAsync(NewsCategory category)
        {
            //Varible for logic
            DateTime timeNow = DateTime.Now;

            //Variable that will hold data and be return
            NewsGroup newsResult = new();

            //Check every key..
            foreach (var kvp in _cachedNews)
            {

                //if 1 min has not passed since last call for the same city..
                if (kvp.Key.Item2 == category)
                {
                    //hold the cash for that location.
                    if (kvp.Key.Item1.AddMinutes(1) > timeNow)
                    {
                        // Fire event and return cached forecast
                        //NewsAvailable.Invoke(this, $"Cached News in category is availble: {category}");
                        return kvp.Value;

                    }
                    //remove old cash
                    _cachedNews.TryRemove(kvp.Key, out _);
                }

            }//If no key found

            //Call api for data
            newsResult = await ReadWebApiAsync(category);

            //Uppdate cache
            if (!_cachedNews.TryAdd((timeNow, category), newsResult))
                throw new Exception("News could not be cached");

            // Fire event
            //NewsAvailable.Invoke(this, $"News in category is availble: {category}");

            //return result
            return newsResult;
        }

        private async Task<NewsGroup> ReadWebApiAsync(NewsCategory category)
        {

#if UseNewsApiSample
            NewsApiData nd = await NewsApiSampleData.GetNewsApiSampleAsync(category);
#else
            //https://newsapi.org/docs/endpoints/top-headlines
            var uri = $"https://newsapi.org/v2/top-headlines?country=se&category={category}&apiKey={apiKey}";

            // Your code here to get live data
            var responseMessage = await httpClient.GetAsync(uri);
            responseMessage.EnsureSuccessStatusCode();
            var nd = await responseMessage.Content.ReadFromJsonAsync<NewsApiData>();
#endif
            //Convert NewsApiData to News class
            var news = new NewsGroup()
            {
                Category = category,
                Articles = new()
            };

            nd.Articles.ForEach(x =>
                news.Articles.Add(new NewsItem()
                {
                    DateTime = x.PublishedAt,
                    Title = x.Title,
                    Description = x.Description,
                    Url = x.Url,
                    UrlToImage = x.UrlToImage
                }));

            
            return news;
        }

    }
}
