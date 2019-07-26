using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FacePost.Helpers;
using Firebase.Firestore;
using Java.Lang;

namespace FacePost.EventListeners
{
    public class FullnameListener : Java.Lang.Object, IOnSuccessListener
    {

        public void FetchUser()
        {
            AppDataHelper.GetFirestore().Collection("users").Document(AppDataHelper.GetFirebaseAuth().CurrentUser.Uid).Get()
                .AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            DocumentSnapshot snapshot = (DocumentSnapshot)result;
            if (snapshot.Exists())
            {
                string fullname = snapshot.Get("fullname").ToString();
                AppDataHelper.SaveFullName(fullname);
            }
        }
    }
}