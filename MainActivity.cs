using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using Android.Support.V7.Widget;
using FacePost.Adapter;
using System.Collections.Generic;
using FacePost.DataModels;
using FacePost.Actvities;
using FacePost.EventListeners;
using System.Linq;
using FacePost.Helpers;
using Firebase.Storage;
using FacePost.Fragments;

namespace FacePost
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar toolbar;
        RecyclerView postRecyclerView;
        PostAdapter postAdapter;
        List<Post> ListOfPost;
        RelativeLayout layStatus;
        ImageView cameraImage;

        PostEventListener postEventListener;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            toolbar = (Android.Support.V7.Widget.Toolbar) FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            postRecyclerView = (RecyclerView)FindViewById(Resource.Id.postRecycleView);

            layStatus = (RelativeLayout)FindViewById(Resource.Id.layStatus);
            layStatus.Click += LayStatus_Click;
            cameraImage = (ImageView)FindViewById(Resource.Id.camera);
            cameraImage.Click += LayStatus_Click;

            // Retreives fullname on Login
            FullnameListener fullnameListener = new FullnameListener();
            fullnameListener.FetchUser();

            // Dummy Data to setup recycler view
            //  CreateData();
            FetchPost();
            
        }

        private void LayStatus_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(CreatePostActivity));
        }

        void FetchPost()
        {
            postEventListener = new PostEventListener();
            postEventListener.FetchPost();
            postEventListener.OnPostRetrieved += PostEventListener_OnPostRetrieved;
        }

        private void PostEventListener_OnPostRetrieved(object sender, PostEventListener.PostEventArgs e)
        {
            ListOfPost = new List<Post>();
            ListOfPost = e.Posts;

            if(ListOfPost != null)
            {
                ListOfPost = ListOfPost.OrderByDescending(x => x.PostDate).ToList();
            }

            SetupRecyclerView();
        }

        void CreateData()
        {
            ListOfPost = new List<Post>();
            ListOfPost.Add(new Post { PostBody = "The United States has been lobbying for months to prevent its western allies from using Huawei equipment in their 5G deployment, and on Wednesday, Washington made it more difficult for the Chinese telecom ", Author = "Uchenna Nnodim", LikeCount = 12 });
            ListOfPost.Add(new Post { PostBody = "TE Connectivity is a technology company that designs and manufactures connectivity and sensor products for harsh environments in a variety of industries, such as automotive, industrial equipment, ", Author = "Johan Gasierel", LikeCount = 34 });
            ListOfPost.Add(new Post { PostBody = "Singapore-based startup YouTrip  thinks consumers of Southeast Asia deserve a taste of the challenger bank revolution happening in the U.S. and Europe, and it has raised $25 million in new funding to bring its app-and-debit-card service to more parts in the region.", Author = "Kylie Jenna", LikeCount = 6 });
            ListOfPost.Add(new Post { PostBody = "TE Connectivity is a technology company that designs and manufactures connectivity and sensor products for harsh environments in a variety of industries, such as automotive, industrial equipment, ", Author = "Johan Gasierel", LikeCount = 78 });

        }

        void SetupRecyclerView()
        {
            postRecyclerView.SetLayoutManager(new Android.Support.V7.Widget.LinearLayoutManager(postRecyclerView.Context));
            postAdapter = new PostAdapter(ListOfPost);
            postRecyclerView.SetAdapter(postAdapter);
            postAdapter.ItemLongClick += PostAdapter_ItemLongClick;
            postAdapter.LikeClick += PostAdapter_LikeClick;
        }

        private void PostAdapter_LikeClick(object sender, PostAdapterClickEventArgs e)
        {
            Post post = ListOfPost[e.Position];
            LikeEventListener likeEventListener = new LikeEventListener(post.ID);

            if (!post.Liked)
            {
                likeEventListener.LikePost();
            }
            else
            {
                likeEventListener.UnlikePost();
            }
        }

        private void PostAdapter_ItemLongClick(object sender, PostAdapterClickEventArgs e)
        {
            string postID = ListOfPost[e.Position].ID;
            string ownerID = ListOfPost[e.Position].OwnerId;

            if(AppDataHelper.GetFirebaseAuth().CurrentUser.Uid == ownerID)
            {
                Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                alert.SetTitle("Edit or Delete Post");
                alert.SetMessage("Are you sure");

                // Edit Post on Firestore
                alert.SetNegativeButton("Edit Post", (o, args) =>
                {
                    EditPostFragment editPostFragment = new EditPostFragment(ListOfPost[e.Position]);
                    var trans = SupportFragmentManager.BeginTransaction();
                    editPostFragment.Show(trans, "edit");

                });

                // Delete Post from Firestore and Storage
                alert.SetPositiveButton("Delete", (o, args) =>
                {
                    AppDataHelper.GetFirestore().Collection("posts").Document(postID).Delete();
                    StorageReference storageReference = FirebaseStorage.Instance.GetReference("postImages/" + postID);
                    storageReference.Delete();
                });

                alert.Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.feed_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            // Logout and detach Post Listener
            if(id == Resource.Id.action_logout)
            {
                postEventListener.RemoveListener();
                AppDataHelper.GetFirebaseAuth().SignOut();
                StartActivity(typeof(LoginActivity));
                Finish();
               
            }

            if(id == Resource.Id.action_refresh)
            {
                Toast.MakeText(this, "Refresh was clicked", ToastLength.Short).Show();
            }

            return base.OnOptionsItemSelected(item);
        }


    }
}