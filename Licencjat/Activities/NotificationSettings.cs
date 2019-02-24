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
using Android.Views.InputMethods;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Licencjat.Database;
using Licencjat.ServerData;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using WindowsAzure.Messaging;
using Android.Util;
using Gcm;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Licencjat
{
    [Activity(Label = "Powiadomienia", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class NotificationSettings : Activity
    {
        private List<string> listNames;
        private List<string> listValues;
        private List<Switch> switches;
        private List<bool> listOver;
        private EditText tb1, tb2, tb3, tb4, tb5;
        private TextView tv1, tv2, tv3, tv4, tv5;
        private Switch sw1, sw2, sw3, sw4, sw5;
        private List<EditText> edit_Boxes;
        private List<TextView> text_Views;
        private Button btn_ok;
        private InputMethodManager imm;
        private LinearLayout mLay;
        private RelativeLayout rLayInsideScrollView;
        private Currencies serv_data;
        private static string Backend_Endpoint = @"https://licencjatbackend20160429045628.azurewebsites.net/";// @"http://192.168.0.103/LicencjatService";//
        private IMobileServiceSyncTable<Currencies> tab;
        //private string api_version = "?ZUMO-API-VERSION=2.0.0";
        private MobileServiceClient client;
        private const string localStore = "localDb.db";

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NotificationSettingsLayout);

            client = new MobileServiceClient(Backend_Endpoint);
            CurrentPlatform.Init();
            await InitLocalStoreAsync();
            tab = client.GetSyncTable<Currencies>();

            ISharedPreferences pref = Application.Context.GetSharedPreferences("ListPref", FileCreationMode.Private);
            var listJsonString = pref.GetString("list", string.Empty);

            listNames = new List<string>();
            listValues = new List<string>();
            listOver = new List<bool>();

            if (listJsonString != string.Empty)
            {
                var list = JsonConvert.DeserializeObject<List<ListForSharedPref>>(listJsonString);

                for (var i = 0; i < list.Count; i++)
                {
                    listNames.Add(list[i].Name);
                    listValues.Add(RemoveComas(list[i].Value));
                    listOver.Add(list[i].OverOrUnder);
                }
            }
            else
            {
                listNames = PrepareData.selectedCurrencies_Names;
                listValues = PrepareData.selectedCurrencies_Values;
                listOver = PrepareData.selectedCurrencies_Over;
                //domyslnie slidery na false
            }

            tb1 = FindViewById<EditText>(Resource.Id.textBox1);
            tb2 = FindViewById<EditText>(Resource.Id.textBox2);
            tb3 = FindViewById<EditText>(Resource.Id.textBox3);
            tb4 = FindViewById<EditText>(Resource.Id.textBox4);
            tb5 = FindViewById<EditText>(Resource.Id.textBox5);

            tv1 = FindViewById<TextView>(Resource.Id.curr1);
            tv2 = FindViewById<TextView>(Resource.Id.curr2);
            tv3 = FindViewById<TextView>(Resource.Id.curr3);
            tv4 = FindViewById<TextView>(Resource.Id.curr4);
            tv5 = FindViewById<TextView>(Resource.Id.curr5);

            sw1 = FindViewById<Switch>(Resource.Id.sw1);
            sw2 = FindViewById<Switch>(Resource.Id.sw2);
            sw3 = FindViewById<Switch>(Resource.Id.sw3);
            sw4 = FindViewById<Switch>(Resource.Id.sw4);
            sw5 = FindViewById<Switch>(Resource.Id.sw5);

            switches = new List<Switch> { sw1, sw2, sw3, sw4, sw5 };

            btn_ok = FindViewById<Button>(Resource.Id.btn_okNotSettings);

            mLay = FindViewById<LinearLayout>(Resource.Id.mLayout);
            rLayInsideScrollView = FindViewById<RelativeLayout>(Resource.Id.rLay_insideScrollview);

            mLay.Click += HideKb;
            rLayInsideScrollView.Click += HideKb;
            sw1.Click += HideKb;
            sw2.Click += HideKb;
            sw3.Click += HideKb;
            sw4.Click += HideKb;
            sw5.Click += HideKb;

            btn_ok.Click += Btn_ok_Click;

            edit_Boxes = new List<EditText>() { tb1, tb2, tb3, tb4, tb5 };
            text_Views = new List<TextView>() { tv1, tv2, tv3, tv4, tv5 };

            var listUserValuesJson = pref.GetString("userValues", string.Empty);
            var listUserValues = new List<string>();

            for (var i = 0; i < listNames.Count; i++)
                listUserValues.Add(string.Empty);

            if (listUserValuesJson != string.Empty)
                listUserValues = JsonConvert.DeserializeObject<List<string>>(listUserValuesJson);

            if (listNames.Count != 0 && listValues.Count != 0 && listOver.Count != 0)
            {
                for (var i = 0; i < listNames.Count; i++)
                {
                    text_Views[i].Text = listNames[i];
                    text_Views[i].Visibility = ViewStates.Visible;
                    edit_Boxes[i].Text = (listUserValues[i]!= string.Empty)? listUserValues[i] : listValues[i];
                    edit_Boxes[i].Visibility = ViewStates.Visible;
                    switches[i].Checked = listOver[i];
                    switches[i].Visibility = ViewStates.Visible;
                }
            }
            else
            {
                AlertDialog.Builder dlg = new AlertDialog.Builder(this);
                AlertDialog alert = dlg.Create();
                alert.SetTitle("Brak walut");
                var intent = new Intent(this, typeof(MainActivity));
                alert.SetButton("Ok", delegate
                {
                    alert.Dismiss();
                    this.StartActivity(intent);
                });
                alert.SetMessage("W menu walut, wybierz najpierw waluty, które chcesz œledziæ");
                alert.Show();
            }
        }

        private void HideKb(object sender, EventArgs e)
        {
            imm = (InputMethodManager)GetSystemService(InputMethodService);
            foreach (var eb in edit_Boxes)
            {
                imm.HideSoftInputFromWindow(eb.WindowToken, 0);
            }
        }

        private async void Btn_ok_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(MainActivity));
            serv_data = new Currencies();
            this.StartActivity(intent);

            var list = new List<ListForSharedPref>();
            ListForSharedPref o;

            for (var i = 0; i < listNames.Count; i++)
            {
                o = new ListForSharedPref()
                {
                    Name = listNames[i],
                    Value = listValues[i],
                    OverOrUnder = switches[i].Checked
                };
                list.Add(o);
                listOver[i] = switches[i].Checked;
            }

            if (listNames.Count >= 1)
            {
                serv_data.FirstCurrency = listNames[0];
                serv_data.FirstCurrencyValue = listValues[0];
                serv_data.FirstCurrencyOver = listOver[0]; 
            }
            if (listNames.Count >= 2)
            {
                serv_data.SecondCurrency = listNames[1];
                serv_data.SecondCurrencyValue = listValues[1];
                serv_data.SecondCurrencyOver = listOver[1];
            }
            if (listNames.Count >= 3)
            {
                serv_data.ThirdCurrency = listNames[2];
                serv_data.ThirdCurrencyValue = listValues[2];
                serv_data.ThirdCurrencyOver = listOver[2];
            }
            if (listNames.Count >= 4)
            {
                serv_data.FourthCurrency = listNames[3];
                serv_data.FourthCurrencyValue = listValues[3];
                serv_data.FourthCurrencyOver = listOver[3];
            }
            if (listNames.Count >= 5)
            {
                serv_data.FifthCurrency = listNames[4];
                serv_data.FifthCurrencyValue = listValues[4];
                serv_data.FifthCurrencyOver = listOver[4];
            }

            ChangeObjectIfValuesChanges(); //zmiana wartosci jesli sie textboxy zmieniaja

            ISharedPreferences pref = Application.Context.GetSharedPreferences("ListPref", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();
            //edit.Clear();

            var temp_ID = (RegiID._id == null) ? 
                pref.GetString("regiID", string.Empty) :
                RegiID._id;
            edit.PutString("regiID", temp_ID);
            serv_data.UserID = temp_ID;

            var tags = new List<string>
            {
                serv_data.FirstCurrencyValue,
                serv_data.SecondCurrencyValue,
                serv_data.ThirdCurrencyValue,
                serv_data.FourthCurrencyValue,
                serv_data.FifthCurrencyValue
            };

            string jsonListValues = JsonConvert.SerializeObject(listValues);
            edit.PutString("listValues", jsonListValues);
            string jsonTags = JsonConvert.SerializeObject(tags);
            edit.PutString("tagsList", jsonTags);
            string jsonListOver = JsonConvert.SerializeObject(listOver);
            edit.PutString("listOver", jsonListOver);
            var listUserValues = new List<string>() { tb1.Text, tb2.Text, tb3.Text, tb4.Text, tb5.Text };
            var jsonUserValues = JsonConvert.SerializeObject(listUserValues);
            edit.PutString("userValues", jsonUserValues);

            string jsonList = JsonConvert.SerializeObject(list);
            edit.PutString("list", jsonList);
            edit.Apply();

            try
            {
                await SyncAsync();
            }
            catch (Exception ex)
            {
                var exception = ex.Message;
            }
            
            Log.Info("MainActivity", "Registering...");
            GcmClient.Register(this, Constants.SenderID);

            //this.StartActivity(intent);
        }
       
        private async Task SyncAsync()
        {
            try
            {             
                var localTable = client.GetSyncTable<Currencies>();
                var remoteTable = client.GetTable<Currencies>();
                //var query = remoteTable.Select(i => i);
                var items = await remoteTable.ToListAsync();

                if (items.Count != 0)
                { 
                    var alreadyInDB = items.Where(x => x.UserID == serv_data.UserID).ToList();
                    if (alreadyInDB.Count != 0)
                    {
                        var tempObj = items.Single(x => x.UserID == serv_data.UserID);
                        serv_data.Id = tempObj.Id;
                        await localTable.UpdateAsync(serv_data);
                    }
                    else
                    {
                        await localTable.InsertAsync(serv_data);
                    }
                }
                else
                {
                    await localTable.InsertAsync(serv_data);
                }

                await client.SyncContext.PushAsync();
            }
            catch (MobileServicePushFailedException e)
            {
                var temp = e.PushResult;
            }
            catch (Exception e)
            {
                var t = e.Message;
            }
        }

        private async Task InitLocalStoreAsync()
        {
            // new code to initialize the SQLite store
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), localStore);

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            var store = new MobileServiceSQLiteStore(path);
            store.DefineTable<Currencies>();
            try
            {
                await client.SyncContext.InitializeAsync(store);
            }
            catch (Exception ex)
            {
                var t = ex.Message;
            }
        }

        private void ChangeObjectIfValuesChanges()
        {
            serv_data.FirstCurrencyValue = (edit_Boxes[0].Text.Replace(".", ",") == string.Empty) ? null : edit_Boxes[0].Text;
            serv_data.SecondCurrencyValue = (edit_Boxes[1].Text.Replace(".", ",")== string.Empty) ? null : edit_Boxes[1].Text;
            serv_data.ThirdCurrencyValue = (edit_Boxes[2].Text.Replace(".", ",") == string.Empty)? null: edit_Boxes[2].Text;
            serv_data.FourthCurrencyValue = (edit_Boxes[3].Text.Replace(".", ",") == string.Empty) ? null : edit_Boxes[3].Text;
            serv_data.FifthCurrencyValue = (edit_Boxes[4].Text.Replace(".", ",") == string.Empty) ? null : edit_Boxes[4].Text;
        }

        private string RemoveComas(string text)
        {
            return text = text.Replace(',', '.');
        }
    }
}