using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using FacePost.DataModels;
using FFImageLoading;

namespace FacePost.Adapter
{
    class PostAdapter : RecyclerView.Adapter
    {
        public event EventHandler<PostAdapterClickEventArgs> ItemClick;
        public event EventHandler<PostAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<PostAdapterClickEventArgs> LikeClick;
        List<Post> items;

        public PostAdapter(List<Post> data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            View itemView = null;
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.post, parent, false);
            var vh = new PostAdapterViewHolder(itemView, OnClick, OnLongClick, OnLike);

            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as PostAdapterViewHolder;
            //holder.TextView.Text = items[position];

            holder.usernameTextView.Text = item.Author;
            holder.postBodyTextView.Text = item.PostBody;
            holder.likeCountTextView.Text = item.LikeCount.ToString() + " Likes";

            if (item.Liked)
            {
                holder.likeImageView.SetImageResource(Resource.Drawable.redlike);
            }
            else
            {
                holder.likeImageView.SetImageResource(Resource.Drawable.like);
            }

            GetImage(item.DownloadUrl, holder.postImageView);
        }

        void GetImage(string url, ImageView imageView)
        {
            ImageService.Instance.LoadUrl(url)
                .Retry(3, 200)
                .DownSample(400, 400)
                .Into(imageView);
        }

        public override int ItemCount => items.Count;

        void OnClick(PostAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(PostAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        void OnLike(PostAdapterClickEventArgs args) => LikeClick?.Invoke(this, args);

    }

    public class PostAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }

        public TextView usernameTextView { get; set; }
        public TextView postBodyTextView { get; set; }
        public TextView likeCountTextView { get; set; }
        public ImageView postImageView { get; set; }
        public ImageView likeImageView { get; set; }

        public PostAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener,
                            Action<PostAdapterClickEventArgs> longClickListener, Action<PostAdapterClickEventArgs> likeClickListener) : base(itemView)
        {
            usernameTextView = (TextView)itemView.FindViewById(Resource.Id.usernameTextView);
            postBodyTextView = (TextView)itemView.FindViewById(Resource.Id.postBodyTextView);
            likeCountTextView = (TextView)itemView.FindViewById(Resource.Id.likeTextView);
            postImageView = (ImageView)itemView.FindViewById(Resource.Id.postImage);
            likeImageView = (ImageView)itemView.FindViewById(Resource.Id.likeButton);

            itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            likeImageView.Click += (sender, e) => likeClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class PostAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}