using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace TempSensor.Forms
{
    public class App : Application
    {
        public static string sasToken = "sv=2015-04-05&tn=TempHistory&sig=LwRAxyIxXV7Fj%2FH4yHfO27yP7fHmjvqxHAOkbSbw%2Fyw%3D&spr=https%2Chttp&se=2016-03-27T19%3A48%3A46Z&sp=r";
        public static CloudTableClient tableClient;
        public App()
        {
            var storageCredentials = new StorageCredentials(sasToken);
            tableClient = new CloudTableClient(new Uri("https://analyticstempstorage.table.core.windows.net"), storageCredentials);

            MainPage = new NavigationPage( new Home());

        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
