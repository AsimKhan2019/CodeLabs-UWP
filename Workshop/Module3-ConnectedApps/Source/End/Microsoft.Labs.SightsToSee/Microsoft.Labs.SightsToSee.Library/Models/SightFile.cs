using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media.Imaging;
using SQLite.Net.Attributes;
using Newtonsoft.Json;

#if !SQLITE
using Microsoft.WindowsAzure.MobileServices.Files;
#endif

namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public enum SightFileType
    {
        General,
        Image
    }

    public class SightFile : IGuidTable, INotifyPropertyChanged
    {
#if SQLITE

        private Guid _id;

        [PrimaryKey]
        public Guid Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

#else
        private Guid _id = Guid.Empty;
        [JsonIgnore]
        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        // string version required by cloud sync
        [JsonProperty(PropertyName = "id")]
        public string DTOId
        {
            get
            {
                return _id.ToString();
            }
            set
            {
                Id = Guid.Parse(value);
            }
        }

        [JsonIgnore]
        public MobileServiceFile File { get; set; }


#endif

        // 0: General, such as Inking   1: Image
        private SightFileType _fileType;

        public SightFileType FileType
        {
            get { return _fileType; }
            set
            {
                if (_fileType == value) return;
                _fileType = value;
                OnPropertyChanged();
            }
        }

        [System.ComponentModel.DataAnnotations.MaxLength(200)]

        private string _fileName;

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName == value) return;
                _fileName = value;
                OnPropertyChanged();
            }
        }

        private string _uri;

        public string Uri
        {
            get { return _uri; }
            set
            {
                if (_uri == value) return;
                _uri = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageUri));
            }
        }

        private string _originalUri;

        public string OriginalUri
        {
            get { return _originalUri; }
            set
            {
                if (_originalUri == value) return;
                _originalUri = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        [Ignore]
        public Sight Sight { get; set; }

        public Guid SightId { get; set; }

        [JsonIgnore]
        [Ignore]
        public BitmapImage ImageUri => new BitmapImage(new Uri(Uri, UriKind.Absolute));



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
