using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;
using WindowsAzure.Messaging;
using System;
using Microsoft.WindowsAzure.MobileServices;
using KursyWalut.Database;
using Newtonsoft.Json;
using KursyWalut.ServerData;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

//zapytania o pozwolenia przy instalacji mozna to zrobic z manifestu
[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is needed only for Android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
namespace KursyWalut
{
    [BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE },
    Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
    Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
    Categories = new string[] { "@PACKAGE_NAME@" })]
    public class MyBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        public static string[] SENDER_IDS = new string[] { Constants.SenderID };

        public const string TAG = "MyBroadcastReceiver-GCM";
    }

    [Service]
    public class PushHandlerService : GcmServiceBase
    {
        private NotificationHub Hub { get; set; }
        public static string RegistrationID { get; private set; } 
        public string regiID;

        public PushHandlerService() : base(Constants.SenderID)
        {
            Log.Info(MyBroadcastReceiver.TAG, "PushHandlerService() constructor");
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            Log.Info(MyBroadcastReceiver.TAG, "GCM Message Received!");

            var msg = new StringBuilder();

            if (intent != null && intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                    msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
            }

            string messageText = intent.Extras.GetString("message");
            if (!string.IsNullOrEmpty(messageText))
            {
                createNotify("Kursy Walut!", messageText);
            }
            else
            {
                createNotify("Nieprawid³owe dane wiadomoœci", msg.ToString());
            }
        }

        protected override bool OnRecoverableError(Context context, string errorId)
        {
            Log.Warn(MyBroadcastReceiver.TAG, "Recoverable Error: " + errorId);

            return base.OnRecoverableError(context, errorId);
        }

        protected override void OnError(Context context, string errorId)
        {
            Log.Error(MyBroadcastReceiver.TAG, "GCM Error: " + errorId);
        }


        //createNotification("PushHandlerService-GCM Registered...",
        //                     RegistrationID/*"The device has been Registered!"*/);

        protected override void OnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Registered: " + registrationId);
            RegistrationID = registrationId;
            //regiID = registrationId;

            Hub = new NotificationHub(Constants.NotificationHubName, Constants.ListenConnectionString,
                                        context);

            CurrentPlatform.Init();
            RegiID._id = registrationId;
            ISharedPreferences pref = Application.Context.GetSharedPreferences("ListPref", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();
            edit.PutString("regiID", registrationId);
            edit.Apply();

            try
            {
                Hub.UnregisterAll(registrationId);
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }
            var tags = new List<string>() {}; 

            try
            {               
                var json = pref.GetString("list", string.Empty);

                if (json != string.Empty)
                {
                    var listCurr = JsonConvert.DeserializeObject<List<ListForSharedPref>>(json);
                    var listNames = new List<string>();
                    var listVal = new List<string>();
                    var listOver = new List<bool>();
                    var listValFresh = new List<string>();

                    foreach (var el in listCurr)
                    {
                        listValFresh.Add(el.Value);
                        listNames.Add(el.Name);
                        listOver.Add(el.OverOrUnder);
                    }

                    var tagsListSerialized = pref.GetString("tagsList", string.Empty);

                    if (tagsListSerialized != string.Empty)
                    {
                        listVal = new List<string>(JsonConvert.DeserializeObject<List<string>>(tagsListSerialized));
                    }
                    else
                    {
                        listVal = new List<string>(listValFresh);
                    }

                    
                    UpdateTags(registrationId, listNames, listVal, listOver);
                }
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Unregistered: " + registrationId);

            //createNotify("GCM Unregistered...", "The device has been unregistered!");
        }

        private void createNotify(string title, string desc)
        {
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            var uiIntent = new Intent(this, typeof(MainActivity));

            var notification = new Notification(Android.Resource.Drawable.SymActionEmail, title);

            notification.Flags = NotificationFlags.AutoCancel;
            notification.SetLatestEventInfo(this, title, desc, PendingIntent.GetActivity(this, 0, uiIntent, 0));

            notificationManager.Notify(1, notification);
            //dialogNotify(title, desc);
        }

        protected void dialogNotify(string title, string message)
        {
            MainActivity.mActivity.RunOnUiThread(() =>
            {
                AlertDialog.Builder dlg = new AlertDialog.Builder(MainActivity.mActivity);
                AlertDialog alert = dlg.Create();
                alert.SetTitle(title);
                alert.SetButton("Ok", delegate
                {
                    alert.Dismiss();
                });
                alert.SetMessage(message);
                alert.Show();
            });
        }

        private void UpdateTags(string registrationId, List<string> listCurrencies,List<string> listValues, List<bool> listOver)
        { 
            var tags = new List<string>();
            var _listTemp = new List<string>();

            Regex reg = new Regex(@"[^\u0020-\u007E]");

            for (int i = 0; i < listCurrencies.Count; i++)
            {
                string tail;

                var str = listCurrencies[i].Replace(" ", "")
                .Replace("\t", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("(", "")
                .Replace(")", "");
                str = reg.Replace(str, "");
                _listTemp.Add(str.ToLower());

                if (listOver[i])
                {
                    tail = "o";
                }
                else
                {
                    tail = "u";
                }

                var _temptag = _listTemp[i] + listValues[i] + tail;

                _temptag = _temptag.Replace(",", "COMA")
                .Replace(".", "COMA");

                tags.Add(_temptag);
            }

            try
            {
                var hubRegistration = Hub.Register(registrationId, tags.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }
        }
    }
}