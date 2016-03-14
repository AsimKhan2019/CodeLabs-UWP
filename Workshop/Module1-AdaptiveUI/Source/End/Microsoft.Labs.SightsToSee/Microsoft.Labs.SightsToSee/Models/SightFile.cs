using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Microsoft.Labs.SightsToSee.Models
{
    public class SightFile
    {
#if EFCORE
        public Guid Id { get; set; }

        // 0: Image   1: Inking input
        public int FileType { get; set; }

        [MaxLength(200)]
        public string FileName { get; set; }

        public string Uri { get; set; }

        // Sqlite does not support cascade delete
        //[Required]
        public Sight Sight { get; set; }

        [NotMapped]
        public BitmapImage ImageUri => new BitmapImage(new Uri(Uri, UriKind.Absolute));


#else
        public string Id { get; set; }

        public MobileServiceFile File { get; private set; }

        // 0: Image   1: Inking input
        public int FileType { get; set; }

        [MaxLength(200)]
        public string FileName { get; set; }

        public string Uri { get; set; }

        public SightFile(MobileServiceFile file, TodoItem todoItem)
        {
            FileName = file.Name;
            File = file;

            //FileHelper.GetLocalFilePathAsync(todoItem.Id, file.Name).ContinueWith(x => this.Uri = x.Result);
        }
#endif
    }
}
