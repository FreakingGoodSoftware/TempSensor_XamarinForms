using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempSensor.Forms.ViewModels;
using Xamarin.Forms;

namespace TempSensor.Forms.Pages
{
    public partial class FilterPage : ContentPage
    {
        public FilterPage()
        {
            InitializeComponent();
        }

        public async void OnTimeClick(object s, EventArgs e)
        {
            (BindingContext as DayPageViewModel).ShowTimeTemp();
            await Navigation.PopModalAsync();
        }
        public async void OnAverageClick(object s, EventArgs e)
        {
            (BindingContext as DayPageViewModel).ShowAverage();
            await Navigation.PopModalAsync();
        }
    }
}
