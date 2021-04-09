using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace LentaNewsReader.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        const string NEWS_URL = "https://lenta.ru/rss/news";
        ApplicationContext AppContext;
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                _ErrorMessage = "Warning! Now in design mode.";
            }
            AppContext = new ApplicationContext();
            UpdateNews();
        }

        public ObservableCollection<SyndicationItem> CollectionNews { get; set; } = new ObservableCollection<SyndicationItem>();

        private void UpdateNews()
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(NEWS_URL))
                {
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    for (int i = 0; i < 3; i++)
                    {
                        if (feed.Items.Count() > i)
                        {
                            CollectionNews.Add(feed.Items.ElementAt(i));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        RelayCommand _CommandSave;
        public RelayCommand CommandSave
        {
            get => _CommandSave ?? (_CommandSave = new RelayCommand(() => {
                SaveNewsToDb();
            }));
        }

        private void SaveNewsToDb()
        {
            foreach (var item in CollectionNews)
            {
                var news = new News();
                var sb = new StringBuilder();
                using (var writer = XmlWriter.Create(sb))
                {
                    item.SaveAsRss20(writer);
                    news.XmlText = sb.ToString();
                } 
                AppContext.News.Add(news);
            }
            AppContext.SaveChanges();

        }

        string _ErrorMessage;
        public string ErrorMessage
        {
            get => _ErrorMessage;
            set => Set(ref _ErrorMessage, value);
        }

    }


}