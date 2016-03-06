using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempSensor.Forms.Pages;
using TempSensor.Forms.ViewModels;
using Xamarin.Forms;

namespace TempSensor.Forms
{
    public partial class DayPage : ContentPage
    {
        public DayPage()
        {
            InitializeComponent();

            BindingContext = new DayPageViewModel(Navigation);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as DayPageViewModel).
        }
    }
}
