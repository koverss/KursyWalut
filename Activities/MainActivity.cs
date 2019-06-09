using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
using System.Collections.Generic;
using Android.Util;
using Gcm.Client;
using Microsoft.WindowsAzure.MobileServices; // azure
using Android.Net;

namespace KursyWalut
{
    [Activity(Label = "Kursy Walut", 
        MainLauncher = true, Icon = "@drawable/icon",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation |
        Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        
        //mobile service wskazujący na moja apke na serv
        private Button btn_notifications;
        private Button btn_selectRates;

        private TextView firstCurrencyName;
        private TextView secondCurrencyName;
        private TextView thirdCurrencyName;
        private TextView fourthCurrencyName;
        private TextView fifthCurrencyName;

        private LinearLayout firstL;
        private LinearLayout secondL;
        private LinearLayout thirdL;
        private LinearLayout fourthL;
        private LinearLayout fifthL;

        private TextView first_DailyExRate;
        private TextView second_DailyExRate;
        private TextView third_DailyExRate;
        private TextView fourth_DailyExRate;
        private TextView fifth_DailyExRate;

        private TextView[] exRatesList;
        private LinearLayout[] layoutsWithExRates;
        private TextView[] tv_texts;

        private List<string> listOfNames;
        private List<string> listExchangeRates; //zeby sie w kazdym creacie nie tworzyły nowe zmienne

        private TextView updateDate;

        public static MainActivity mActivity;
        public static MobileServiceClient client = new MobileServiceClient("https://licencjat.azurewebsites.net");       

        //private static string Backend_Endpoint = "http://Licencjatbackend20160429045628.azurewebsites.net"; //endpoind serwera

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            CurrentPlatform.Init();

            SetContentView(Resource.Layout.Main);

            mActivity = this; //instancja MainActivity
            btn_notifications = FindViewById<Button>(Resource.Id.btn_Notifications);
            //btn_settings = FindViewById<Button>(Resource.Id.btn_Settings);
            btn_selectRates = FindViewById<Button>(Resource.Id.btn_SelectRates);

            btn_selectRates.Click += Btn_selectRates_Click;
            //btn_settings.Click += Btn_settings_Click;
            btn_notifications.Click += Btn_notifications_Click;  

            firstCurrencyName = FindViewById<TextView>(Resource.Id.firstCurrencyName);
            fourthCurrencyName = FindViewById<TextView>(Resource.Id.fourthCurrencyName);
            secondCurrencyName = FindViewById<TextView>(Resource.Id.secondCurrencyName);
            thirdCurrencyName = FindViewById<TextView>(Resource.Id.thirdCurrencyName);
            fifthCurrencyName = FindViewById<TextView>(Resource.Id.fifthCurrencyName);

            firstL = FindViewById<LinearLayout>(Resource.Id.lout_firstR);
            secondL = FindViewById<LinearLayout>(Resource.Id.lout_secondR);
            thirdL = FindViewById<LinearLayout>(Resource.Id.lout_thirdR);
            fourthL = FindViewById<LinearLayout>(Resource.Id.lout_fourthR);
            fifthL = FindViewById<LinearLayout>(Resource.Id.lout_fifthR);

            first_DailyExRate = FindViewById<TextView>(Resource.Id.first_DailyExRate);
            second_DailyExRate = FindViewById<TextView>(Resource.Id.second_DailyExRate);
            third_DailyExRate = FindViewById<TextView>(Resource.Id.third_DailyExRate);
            fourth_DailyExRate = FindViewById<TextView>(Resource.Id.fourth_DailyExRate);
            fifth_DailyExRate = FindViewById<TextView>(Resource.Id.fifth_DailyExRate);
            updateDate = FindViewById<TextView>(Resource.Id.dailyUpdate_DateAndTime);

            exRatesList = new[] { first_DailyExRate, second_DailyExRate, third_DailyExRate
                                     ,fourth_DailyExRate, fifth_DailyExRate};

            layoutsWithExRates = new[] { firstL, secondL, thirdL, fourthL, fifthL };

            tv_texts = new[] { firstCurrencyName, secondCurrencyName, thirdCurrencyName
                                  ,fourthCurrencyName, fifthCurrencyName};

            ISharedPreferences pref = Application.Context.GetSharedPreferences("ListPref", FileCreationMode.Private);
            var listJsonString = pref.GetString("list", string.Empty);
            string _date = pref.GetString("dateOfUpdate", string.Empty);

            if (!string.IsNullOrEmpty(_date))
            {
                DateTime dt = DateTime.Parse(_date);
                updateDate.Text = dt.ToShortDateString();
            }
            //else
            //{
            //    updateDate.Text = _date;
            //}
            
            listOfNames = new List<string>();
            listExchangeRates = new List<string>();
            if (listJsonString != string.Empty)
            {
                var list = JsonConvert.DeserializeObject<List<ListForSharedPref>>(listJsonString);

                for (var i = 0; i < list.Count; i++)
                {
                    listOfNames.Add(list[i].Name);
                    listExchangeRates.Add(list[i].Value);
                }
            }

            if (listOfNames.Count != 0)
            {
                for (var i = 0; i < listOfNames.Count; i++)
                {
                    tv_texts[i].Text = listOfNames[i];
                    exRatesList[i].Text = listExchangeRates[i];
                    layoutsWithExRates[i].Visibility = ViewStates.Visible;
                }
            }

            if((pref.GetString("regiID", string.Empty)==string.Empty))
                RegisterWithGCM();     
        }

        private void Btn_notifications_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(NotificationSettings));
            StartActivity(intent); ;
        }

        private void Btn_selectRates_Click(object sender, EventArgs e)
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            bool isOnline = (activeConnection != null) && activeConnection.IsConnected;
            if (isOnline)
            {
                var intent = new Intent(this, typeof(SelectedCurrencies));
                StartActivity(intent);
            }
            else
            {
                AlertDialog.Builder dlg = new AlertDialog.Builder(this);
                AlertDialog alert = dlg.Create();
                alert.SetTitle("Brak połączenia z internetem");
                alert.SetMessage("Wymagane połączenie z internetem");
                alert.Show();
            }
        }

        private void RegisterWithGCM()
        {
            // Check to ensure everything's set up right
            GcmClient.CheckDevice(this);
            GcmClient.CheckManifest(this);

            // Register for push notifications
            Log.Info("MainActivity", "Registering...");
            GcmClient.Register(this, Constants.SenderID);
        }
    }
}

