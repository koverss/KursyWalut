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

namespace Licencjat
{
    public class SelectedItems
    {
        public bool IsSelected { get; set; }
        public string CurrencyName { get; set; }
        public string ExchangeRate { get; set;  }
    }
}