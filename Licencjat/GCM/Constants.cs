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
    public class Constants
    {
        public const string SenderID = "1016940572338"; // Google API Project Number
        public const string ListenConnectionString = "Endpoint=sb://licencjat.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=0svbcHZt6l5YcBaTz/mgO+G7dO8fWMqugdY3CIM9cvM=";
        public const string NotificationHubName = "LicHub";
    }
}