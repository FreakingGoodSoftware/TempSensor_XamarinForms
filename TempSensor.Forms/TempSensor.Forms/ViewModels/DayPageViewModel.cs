using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TempSensor.Forms.Pages;
using Xamarin.Forms;

namespace TempSensor.Forms.ViewModels
{
    public class DayPageViewModel : BaseViewModel
    {
        public INavigation Navigation { get; set; }
        TimeSpan selectedTime;
        string tempNow;
        HttpClient client;

        public DayPageViewModel(INavigation navigation)
        {
            Navigation = navigation;
            FilterCommand = new Command(execute: ShowFilter);
            SelectedTime = DateTime.Now.TimeOfDay;
            client = new HttpClient(new ModernHttpClient.NativeMessageHandler());
        }

        void ShowTemp()
        {
            // Call service
        }

        public void ShowFilter()
        {
            var filterPage = new FilterPage();
            filterPage.BindingContext = this;
            Navigation.PushModalAsync(filterPage);
        }
        public ICommand FilterCommand { private set; get; }

        public TimeSpan SelectedTime
        {
            get
            {
                return selectedTime;
            }

            set
            {
                SetProperty(ref selectedTime, value);
            }
        }

        public string TempNow
        {
            get
            {
                return tempNow;
            }

            set
            {
                SetProperty(ref tempNow, value);
            }
        }
    }
}
