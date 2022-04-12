using System;
using System.Threading.Tasks;
using News.Models;
using News.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace News.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewsPage : ContentPage
    {
        NewsService _service;
        public NewsPage()
        {
            InitializeComponent();
            _service = new NewsService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Header.Text = Title;
            //This is making the first load of data
            MainThread.BeginInvokeOnMainThread(async () => { await LoadNews(); });
        }

        private async Task LoadNews()
        {
            try
            {
                NewsCategory selectedCategory = (NewsCategory)Enum.Parse(typeof(NewsCategory), Title);

                var news = await _service.GetNewsAsync(selectedCategory);
                newsList.ItemsSource = news.Articles;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void RefreshButton(object sender, EventArgs e)
        {
            AnimationLoader.IsRunning = true;
            await Task.Delay(4000);
            await LoadNews();
            AnimationLoader.IsRunning = false;
        }
        
        private async void newsList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var newsTapped = (NewsItem)e.Item;

            await Navigation.PushAsync(new ArticleView(newsTapped.Url));
        }       
    }
}