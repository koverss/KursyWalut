
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;
using System;
using System.Linq;
using Android.Views;
using Newtonsoft.Json;

namespace Licencjat
{
    [Activity(Label = "Wybór walut", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class SelectedCurrencies : Activity
    {
        private List<SelectedItems> elements;
        private List<string> selectedCurrencies_Names;
        private List<string> namesList;
        private List<string> selectedCurrencies_Values;
        private ListView checkBoxListView;
        private ListView pickedCurrenciesListView;
        private Button btn_okSelRates;

        private string url_tableA = "http://www.nbp.pl/kursy/xml/LastA.xml";
        private string url_tableB = "http://www.nbp.pl/kursy/xml/LastB.xml";

        DateTime date;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SelectedCurrenciesLayout);

            checkBoxListView = FindViewById<ListView>(Resource.Id.checkBoxListView);
            pickedCurrenciesListView = FindViewById<ListView>(Resource.Id.pickedCurrenciesListView);
            btn_okSelRates = FindViewById<Button>(Resource.Id.btn_okSR);

            ISharedPreferences pref = Application.Context.GetSharedPreferences("ListPref", FileCreationMode.Private);
            var listJsonString = pref.GetString("list", string.Empty);
            var elementsJson = pref.GetString("elementsList", string.Empty);

            elements = (elementsJson == string.Empty)?PrepareData.elements:JsonConvert.DeserializeObject<List<SelectedItems>>(elementsJson); // przypisuje jeszcze raz dla adaptera przy zmianie orientacji

            selectedCurrencies_Names = new List<string>();
            selectedCurrencies_Values = new List<string>();

            if (listJsonString != string.Empty)
            {
                var list = JsonConvert.DeserializeObject<List<ListForSharedPref>>(listJsonString);

                for (var i = 0; i < list.Count; i++)
                {
                    selectedCurrencies_Names.Add(list[i].Name);
                    selectedCurrencies_Values.Add(list[i].Value);
                }
            }
            else
            {
                selectedCurrencies_Names = PrepareData.selectedCurrencies_Names;
                selectedCurrencies_Values = PrepareData.selectedCurrencies_Values;
            }

            if (elements.Count == 0)
            {
                try
                {
                    PrepareData.XmlCurrencyNamesToList(PrepareData.FormatXMLtoUTF8(url_tableA));
                    PrepareData.XmlCurrencyNamesToList(PrepareData.FormatXMLtoUTF8(url_tableB)); //tworze liste elementów 3 własciwosci
                }
                catch (Exception exc)
                {
                    AlertDialog.Builder mess = new AlertDialog.Builder(this);
                    mess.SetMessage(exc.Message);
                    mess.Show();
                }
            }


            namesList = PrepareData.XmlCurrencyNames(elements);
            ArrayAdapter<string> tablesAdapter = new ArrayAdapter<string>(this, 
                Android.Resource.Layout.SimpleListItemMultipleChoice,
                namesList);
            ArrayAdapter<string> pickedCurrAdapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleSelectableListItem,
                selectedCurrencies_Names);

            pickedCurrenciesListView.Adapter = pickedCurrAdapter;
            checkBoxListView.Adapter = tablesAdapter;
            checkBoxListView.ChoiceMode = ChoiceMode.Multiple;

            if (selectedCurrencies_Names.Count != 0)
            {
                pickedCurrenciesListView.Visibility = ViewStates.Visible;
            }


            foreach (var i in elements) //checkuje przy recreate wczesniej zaznaczone
            {
                var el = selectedCurrencies_Names.Any(a => a == i.CurrencyName);
                if (el)
                {
                    var selectedIndex = namesList.IndexOf(i.CurrencyName);
                    elements[selectedIndex].IsSelected = true;
                    checkBoxListView.SetItemChecked(selectedIndex, true);
                } 
            }

            date = PrepareData.GetDocumentDate(url_tableA); //data
            //pref.

            checkBoxListView.ItemClick += CheckBoxListView_ItemClick;
            pickedCurrenciesListView.ItemClick += PickedCurrenciesListView_ItemClick;

            btn_okSelRates.Click += Btn_okSelRates_Click;
        }

        private void PickedCurrenciesListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = elements.Single(x => x.CurrencyName == selectedCurrencies_Names[e.Position]);
            item.IsSelected = false; 

            selectedCurrencies_Names.Remove(selectedCurrencies_Names[e.Position]);
            selectedCurrencies_Values.Remove(selectedCurrencies_Values[e.Position]);

            pickedCurrenciesListView = FindViewById<ListView>(Resource.Id.pickedCurrenciesListView);
            ArrayAdapter<string> pickedCurrAdapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleSelectableListItem, 
                selectedCurrencies_Names);
            pickedCurrenciesListView.Adapter = pickedCurrAdapter;

            checkBoxListView = FindViewById<ListView>(Resource.Id.checkBoxListView);
            var chkListAfterUncheckingItem = PrepareData.XmlCurrencyNames(elements);
            var selectedIndex = chkListAfterUncheckingItem.FindIndex(a => a == item.CurrencyName);

            checkBoxListView.SetItemChecked(selectedIndex, false);

            if (selectedCurrencies_Names.Count == 0)
            {
                pickedCurrenciesListView.Visibility = ViewStates.Gone;
            }
        }

        private void CheckBoxListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            pickedCurrenciesListView = FindViewById<ListView>(Resource.Id.pickedCurrenciesListView);
            int selectedIndex;

            if (elements[e.Position].IsSelected == false && selectedCurrencies_Names.Count < 5)
            {
                selectedCurrencies_Names.Add(elements[e.Position].CurrencyName);
                selectedCurrencies_Values.Add(elements[e.Position].ExchangeRate);
                elements[e.Position].IsSelected = true;

                if (pickedCurrenciesListView.Visibility != ViewStates.Visible)
                    pickedCurrenciesListView.Visibility = ViewStates.Visible;

                ArrayAdapter<string> pickedCurrAdapter = new ArrayAdapter<string>(this,
                    Android.Resource.Layout.SimpleSelectableListItem,
                     selectedCurrencies_Names);
                pickedCurrenciesListView.Adapter = pickedCurrAdapter;
            }
            else if (elements[e.Position].IsSelected == false && selectedCurrencies_Names.Count >= 5)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Za dużo walut");
                alert.SetMessage("Wybrano za dużo walut (max 5 walut)");
                alert.Show();
                selectedIndex = e.Position;
                if(!elements[e.Position].IsSelected)
                    checkBoxListView.SetItemChecked(selectedIndex, false);
            }
            else if (elements[e.Position].IsSelected == true)
            {
                selectedCurrencies_Names.Remove(elements[e.Position].CurrencyName);
                selectedCurrencies_Values.Remove(elements[e.Position].ExchangeRate);
                elements[e.Position].IsSelected = false;

                if (selectedCurrencies_Names.Count == 0)
                    pickedCurrenciesListView.Visibility = ViewStates.Gone;

                checkBoxListView = FindViewById<ListView>(Resource.Id.checkBoxListView);
                selectedIndex = e.Position;
                checkBoxListView.SetItemChecked(selectedIndex, false);

                ArrayAdapter<string> pickedCurrAdapter = new ArrayAdapter<string>(this,
                    Android.Resource.Layout.SimpleSelectableListItem,
                    selectedCurrencies_Names);
                pickedCurrenciesListView.Adapter = pickedCurrAdapter;
            }   
        }

        private void Btn_okSelRates_Click(object sender, EventArgs e)
        {
            var intentMainActivity = new Intent(this, typeof(MainActivity));
            var listOfPairs = new List<ListForSharedPref>();
            ListForSharedPref o;
            for (var i = 0; i < selectedCurrencies_Names.Count; i++)
            {
                o = new ListForSharedPref() { Name = selectedCurrencies_Names[i], Value = selectedCurrencies_Values[i] };
                listOfPairs.Add(o);
            }

            string json = JsonConvert.SerializeObject(listOfPairs);
            string jsonElements = JsonConvert.SerializeObject(elements);

            ISharedPreferences pref = Application.Context.GetSharedPreferences("ListPref", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();
            edit.Clear();
            var _date = date.ToString();
            edit.PutString("dateOfUpdate",_date);
            edit.PutString("elementsList", jsonElements);
            edit.PutString("list", json); //przekazuje zaserializowany obiekt zeby elementy 2 list nie byly segregowane niezaleznie od siebie
            edit.Apply();

            this.StartActivity(intentMainActivity);
        }


    }
}