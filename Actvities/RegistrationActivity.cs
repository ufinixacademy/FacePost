using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using FacePost.EventListeners;
using FacePost.Fragments;
using FacePost.Helpers;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Java.Lang;
using Java.Util;

namespace FacePost.Actvities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class RegistrationActivity : AppCompatActivity
    {
        Button registerButton;
        TextInputLayout fullnameText, emailText, passwordText, confirmPasswordText;
        FirebaseFirestore database;
        FirebaseAuth mAuth;
        string fullname, email, password, confirm;
        TaskCompletionListeners taskCompletionListeners = new TaskCompletionListeners();
        ProgressDialogueFragment progressDialogue;
        TextView clickHereToLogin;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);
            fullnameText = (TextInputLayout)FindViewById(Resource.Id.fullNameRegText);
            emailText = (TextInputLayout)FindViewById(Resource.Id.emailRegText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordRegText);
            confirmPasswordText = (TextInputLayout)FindViewById(Resource.Id.confirmPasswordRegText);
            clickHereToLogin = (TextView)FindViewById(Resource.Id.clickToLogin);
            clickHereToLogin.Click += ClickHereToLogin_Click;

            registerButton = (Button) FindViewById(Resource.Id.registerButton);
            registerButton.Click += RegisterButton_Click;
            database = AppDataHelper.GetFirestore();
            mAuth = AppDataHelper.GetFirebaseAuth();
        }

        private void ClickHereToLogin_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
           
            fullname = fullnameText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;
            confirm = confirmPasswordText.EditText.Text;

            if(fullname.Length < 4)
            {
                Toast.MakeText(this, "Please enter a valid name", ToastLength.Short).Show();
                return;
            }
            else if (!email.Contains("@"))
            {
                Toast.MakeText(this, "Please enter a valid email address", ToastLength.Short).Show();
                return;
            }
            else if(password.Length < 8)
            {
                Toast.MakeText(this, "Please enter a password upto 8 characters", ToastLength.Short).Show();
                return;
            }
            else if (password != confirm)
            {
                Toast.MakeText(this, "Password does not match, please make correction", ToastLength.Short).Show();
                return;
            }

            // Perform Registration
            ShowProgressDialogue("Registering you..");
            mAuth.CreateUserWithEmailAndPassword(email, password).AddOnSuccessListener(this, taskCompletionListeners)
                .AddOnFailureListener(this, taskCompletionListeners);

            // Registration Success Callback
            taskCompletionListeners.Sucess += (success, args) =>
            {
                HashMap userMap = new HashMap();
                userMap.Put("email", email);
                userMap.Put("fullname", fullname);

                DocumentReference userReference = database.Collection("users").Document(mAuth.CurrentUser.Uid);
                userReference.Set(userMap);
                CloseProgressDialogue();
                StartActivity(typeof(MainActivity));
                Finish();
            };

            // Registration Failure Callback
            taskCompletionListeners.Failure += (failure, args) =>
            {
                CloseProgressDialogue();
                Toast.MakeText(this, "Registartion Failed : " + args.Cause, ToastLength.Short).Show();
            };
           

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



    }
}