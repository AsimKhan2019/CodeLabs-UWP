using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public class Review : INotifyPropertyChanged
    {
        private string _extract;
        private int _id;
        private int _publicationId;
        private string _publicationnName;
        private string _publicationRatingName;
        private string _sourceUrl;

        [JsonProperty("id")]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("publication_id")]
        public int PublicationId
        {
            get { return _publicationId; }
            set
            {
                if (_publicationId == value) return;
                _publicationId = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("publication_name")]
        public string PublicationnName
        {
            get { return _publicationnName; }
            set
            {
                if (_publicationnName == value) return;
                _publicationnName = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("extract")]
        public string Extract
        {
            get { return _extract; }
            set
            {
                if (_extract == value) return;
                _extract = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("source_url")]
        public string SourceUrl
        {
            get { return _sourceUrl; }
            set
            {
                if (_sourceUrl == value) return;
                _sourceUrl = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("publication_rating_name")]
        public string PublicationRatingName
        {
            get { return _publicationRatingName; }
            set
            {
                if (_publicationRatingName == value) return;
                _publicationRatingName = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}