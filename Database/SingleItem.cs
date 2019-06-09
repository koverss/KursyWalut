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

namespace KursyWalut.Database
{
    public class SingleItem
    {
        //dla preferences
        public string CurrName { get; set; }
        public string Value { get; set; }
        public bool Over { get; set; }
    }
}