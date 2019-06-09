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

namespace KursyWalut.ServerData
{
    public class Currencies
    {
        //[JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        //[JsonProperty(PropertyName = "UserID")]
        public string UserID { get; set; }

        //[JsonProperty(PropertyName = "FirstCurrency")]
        public string FirstCurrency { get; set; }
        //[JsonProperty(PropertyName = "FirstCurrencyValue")]
        public string FirstCurrencyValue { get; set; }
        //[JsonProperty(PropertyName = "FirstCurrencyOver")]
        public bool FirstCurrencyOver { get; set; }

        //[JsonProperty(PropertyName = "SecondCurrency")]
        public string SecondCurrency { get; set; }
        //[JsonProperty(PropertyName = "SecondCurrencyValue")]
        public string SecondCurrencyValue { get; set; }
        //[JsonProperty(PropertyName = "SecondCurrencyOver")]
        public bool SecondCurrencyOver { get; set; }

       // [JsonProperty(PropertyName = "ThirdCurrency")]
        public string ThirdCurrency { get; set; }
        //[JsonProperty(PropertyName = "ThirdCurrencyValue")]
        public string ThirdCurrencyValue { get; set; }
        //[JsonProperty(PropertyName = "ThirdCurrencyOver")]
        public bool ThirdCurrencyOver { get; set; }

        //[JsonProperty(PropertyName = "FourthCurrency")]
        public string FourthCurrency { get; set; }
        //[JsonProperty(PropertyName = "FourthCurrencyValue")]
        public string FourthCurrencyValue { get; set; }
        //[JsonProperty(PropertyName = "FourthCurrencyOver")]
        public bool FourthCurrencyOver { get; set; }

        //[JsonProperty(PropertyName = "FifthCurrency")]
        public string FifthCurrency { get; set; }
        //[JsonProperty(PropertyName = "FifthCurrencyValue")]
        public string FifthCurrencyValue { get; set; }
        //[JsonProperty(PropertyName = "FifthCurrencyOver")]
        public bool FifthCurrencyOver { get; set; }
    }
}