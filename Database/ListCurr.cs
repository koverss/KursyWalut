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
using Newtonsoft.Json;

namespace KursyWalut.Database
{
    public static class ListCurr
    {
        private static ISharedPreferences pref = Application.Context.GetSharedPreferences("ListPref", FileCreationMode.Private);
        private static int counter = 0;
        public static List<SingleItem> listSingleItems = new List<SingleItem>();

        public static List<SingleItem> CreateList()
        {
            var listJsonString = pref.GetString("list", string.Empty);
            var list = JsonConvert.DeserializeObject<List<ListForSharedPref>>(listJsonString);
            counter = list.Count()/3;

            for (int i = 0; i < counter; i++)
            {
                var o = new SingleItem { CurrName = list[i].Name, Value = list[i].Value, Over = list[i].OverOrUnder };
                listSingleItems.Add(o);
            }
            return listSingleItems;
        }
    }
}