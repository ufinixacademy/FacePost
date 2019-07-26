using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using FacePost.EventListeners;
using FacePost.Fragments;
using FacePost.Helpers;
using Firebase.Firestore;
using Firebase.Storage;
using Java.Util;
using Plugin.Media;

namespace FacePost.Actvities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class CreatePostActivity : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar toolbar;
        ImageView postImage;
        Button submitButton;
        EditText postEditText;

        TaskCompletionListeners taskCompletionListeners = new TaskCompletionListeners();
        TaskCompletionListeners downloadUrlListener = new TaskCompletionListeners();

        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        byte[] fileBytes;
        ProgressDialogueFragment progressDialogue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.create_post);
            toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Create Post";

            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetDisplayHomeAsUpEnabled(true);
            actionBar.SetHomeAsUpIndicator(Resource.Drawable.outline_arrowback);
            postEditText = (EditText)FindViewById(Resource.Id.postEditText);

            postImage = (ImageView)FindViewById(Resource.Id.newPostImage);
            postImage.Click += PostImage_Click;

            submitButton = (Button)FindViewById(Resource.Id.submitButton);
            submitButton.Click += SubmitButton_Click;

            RequestPermissions(permissionGroup, 0);
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            HashMap postMap = new HashMap();
            postMap.Put("author", AppDataHelper.GetFullName());
            postMap.Put("owner_id", AppDataHelper.GetFirebaseAuth().CurrentUser.Uid);
            postMap.Put("post_date", DateTime.Now.ToString());
            postMap.Put("post_body", postEditText.Text);

            DocumentReference newPostRef = AppDataHelper.GetFirestore().Collection("posts").Document();
            string postKey = newPostRef.Id;

            postMap.Put("image_id", postKey);


            ShowProgressDialogue("Saving Information ...");

            // Save Post Image to Firebase Storaage
            StorageReference storageReference = null;
            if(fileBytes != null)
            {
                storageReference = FirebaseStorage.Instance.GetReference("postImages/" + postKey);
                storageReference.PutBytes(fileBytes)                 
                    .AddOnSuccessListener(taskCompletionListeners)
                    .AddOnFailureListener(taskCompletionListeners);
            }

            // Image Upload Success Callback
            taskCompletionListeners.Sucess += (obj, args) =>
            {
                if(storageReference != null)
                {
                    storageReference.DownloadUrl.AddOnSuccessListener(downloadUrlListener);
                }
            };

            // Image Download URL Callback
            downloadUrlListener.Sucess += (obj, args) =>
            {
                string downloadUrl = args.Result.ToString();
                postMap.Put("download_url", downloadUrl);

                // Save post to Firebase Firestore
                newPostRef.Set(postMap);
                CloseProgressDialogue();
                Finish();
            };


            // Image Upload Failure Callback
            taskCompletionListeners.Failure += (obj, args) =>
            {
                Toast.MakeText(this, "Upload was not completed", ToastLength.Short).Show();
            };
        }

        private void PostImage_Click(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder photoAlert = new Android.Support.V7.App.AlertDialog.Builder(this);
            photoAlert.SetMessage("Change Photo");

            photoAlert.SetNegativeButton("Take Photo", (thisalert, args) =>
            {
                // Capture Image
                TakePhoto();
            });

            photoAlert.SetPositiveButton("Upload Photo", (thisAlert, args) =>
            {
                // Choose Image
                SelectPhoto();
            });

            photoAlert.Show();
        }

        async void TakePhoto()
        {
           await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 20,
                Directory = "Sample",
                Name = GenerateRandomString(6) + "facepost.jpg"
            });

            if(file == null )
            {
                return;
            }

            //Converts file.path to byte array and set the resulting bitmap to imageview
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            fileBytes = imageArray;

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            postImage.SetImageBitmap(bitmap);

        }

        async void SelectPhoto()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Upload not supported", ToastLength.Short).Show();
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 30,
            });

            if(file == null)
            {
                return;
            }

            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            fileBytes = imageArray;

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            postImage.SetImageBitmap(bitmap);

        }

        void ShowProgressDialogue(string status)
        {
            progressDialogue = new ProgressDialogueFragment(status);
            var trans = SupportFragmentManager.BeginTransaction();
            progressDialogue.Cancelable = false;
            progressDialogue.Show(trans, "Progress");
        }

        void CloseProgressDialogue()
        {
            if (progressDialogue != null)
            {
                progressDialogue.Dismiss();
                progressDialogue = null;
            }
        }


        string GenerateRandomString(int lenght)
        {
            System.Random rand = new System.Random();
            char[] allowchars = "ABCDEFGHIJKLOMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            string sResult = "";
            for (int i = 0; i <= lenght; i++)
            {
                sResult += allowchars[rand.Next(0, allowchars.Length)];
            }

            return sResult;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}