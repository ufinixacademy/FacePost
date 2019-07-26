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
    public class LikeEventListener : Java.Lang.Object, IOnSuccessListener
    {
        string postID;
        bool Like;

        public LikeEventListener (string _postId)
        {
            postID = _postId;
        }

        public void LikePost()
        {
            Like = true;
            AppDataHelper.GetFirestore().Collection("posts").Document(postID).Get()
                .AddOnSuccessListener(this);
            
        }

        public void UnlikePost()
        {
            Like = false;
            AppDataHelper.GetFirestore().Collection("posts").Document(postID).Get()
                .AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            DocumentSnapshot snapshot = (DocumentSnapshot)result;

            if (!snapshot.Exists())
            {
                return;
            }

            DocumentReference likesReference = AppDataHelper.GetFirestore().Collection("posts").Document(postID);

            if (Like)
            {
                likesReference.Update("likes." + AppDataHelper.GetFirebaseAuth().CurrentUser.Uid, true);
            }
            else
            {
                if(snapshot.Get("likes") == null)
                {
                    return;
                }

                var data = snapshot.Get("likes") != null ? snapshot.Get("likes") : null;
                if(data != null)
                {
                    var dictionaryFromHashMap = new Android.Runtime.JavaDictionary<string, string>(data.Handle, JniHandleOwnership.DoNotRegister);

                    string uid = AppDataHelper.GetFirebaseAuth().CurrentUser.Uid;

                    if (dictionaryFromHashMap.Contains(uid))
                    {
                        dictionaryFromHashMap.Remove(uid);
                        likesReference.Update("likes", dictionaryFromHashMap);
                    }
                }

            }



        }
    }
}