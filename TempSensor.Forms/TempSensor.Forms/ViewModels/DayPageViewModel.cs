using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TempSensor.Forms.Models;
using TempSensor.Forms.Pages;
using Xamarin.Forms;

namespace TempSensor.Forms.ViewModels
{
    public class DayPageViewModel : BaseViewModel
    {
        public INavigation Navigation { get; set; }
        TimeSpan selectedTime;
        string tempNow;
        string tempText;
        ObservableCollection<HistoryEntity> tempHistory = new ObservableCollection<HistoryEntity>();

        public DayPageViewModel(INavigation navigation)
        {
            Navigation = navigation;
            ShowAverage();
            FilterCommand = new Command(execute: ShowFilter);
            SelectedTime = DateTime.Now.TimeOfDay;
        }

        public async void ShowTimeTemp()
        {
            TempHistory.Clear();
            CloudTable table = App.tableClient.GetTableReference("TempHistory");

            var date = DateTime.Now;
            var dateOffset = new DateTimeOffset(date.Year, date.Month, date.Day, SelectedTime.Hours, SelectedTime.Minutes, SelectedTime.Seconds, TimeZoneInfo.Local.BaseUtcOffset.Subtract(TimeSpan.FromHours(1)));
            TableQuery<HistoryEntity> query = new TableQuery<HistoryEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "KeozPi"),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, dateOffset),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, dateOffset.AddMinutes(11))
                        )
                    )
                );
            TableQuerySegment<HistoryEntity> querySegment = null;

            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = await table.ExecuteQuerySegmentedAsync(query, querySegment != null ? querySegment.ContinuationToken : null);
                foreach (var item in querySegment)
                {
                    item.CreatedAtString = item.Timestamp.ToLocalTime().Subtract(TimeSpan.FromHours(1)).ToString("t");
                    TempHistory.Add(item);
                }
            }

            if (TempHistory.Count > 0)
            {
                double sum = 0;
                foreach (HistoryEntity entity in TempHistory)
                {
                    sum += double.Parse(entity.Temp);
                }
                sum = sum / TempHistory.Count;
                TempNow = String.Format("{0} °C", sum.ToString("##.#"));
            }
            else
            {
                TempNow = "Not enough data";
            }
            TempText = String.Format("Temperature at {0}", SelectedTime.ToString(@"hh\:mm"));
           
        }

        public async void ShowAverage()
        {
            TempHistory.Clear();

            CloudTable table = App.tableClient.GetTableReference("TempHistory");

            var date = DateTime.Now;
            var dateOffset = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeZoneInfo.Local.BaseUtcOffset.Subtract(TimeSpan.FromHours(1)));
            TableQuery<HistoryEntity> query = new TableQuery<HistoryEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "KeozPi"),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, dateOffset),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, dateOffset.AddHours(24))
                        )
                    )
                );
            TableQuerySegment<HistoryEntity> querySegment = null;

            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = await table.ExecuteQuerySegmentedAsync(query, querySegment != null ? querySegment.ContinuationToken : null);
                foreach (var item in querySegment)
                {
                    item.CreatedAtString = item.Timestamp.ToLocalTime().Subtract(TimeSpan.FromHours(1)).ToString("t");
                    TempHistory.Add(item);
                }
            }

            if (TempHistory.Count > 0)
            {
                double sum = 0;
                foreach (HistoryEntity entity in TempHistory)
                {
                    sum += double.Parse(entity.Temp);
                }
                sum = sum / TempHistory.Count;
                TempNow = String.Format("{0} °C", sum.ToString("##.#"));
            }
            else
            {
                TempNow = "Not enough data";
            }

            TempText = "Average today";
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

        public string TempText
        {
            get
            {
                return tempText;
            }

            set
            {
                SetProperty(ref tempText, value);
            }
        }

        public ObservableCollection<HistoryEntity> TempHistory
        {
            get
            {
                return tempHistory;
            }

            set
            {
                SetProperty(ref tempHistory, value);
            }
        }
    }
}
