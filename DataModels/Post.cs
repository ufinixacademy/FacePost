using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FacePost.DataModels
{
    public class Post
    {
        public string Author { get; set; }
        public string ImageId { get; set; }
        public string DownloadUrl { get; set; }
        public string PostBody { get; set; }
        public int LikeCount { get; set; }
        public bool Liked { get; set; }
        public string ID { get; set; }
        public string OwnerId { get; set; }
        public DateTime PostDate { get; set; }
    }
}