using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempSensor.Forms.Models
{
    public class HistoryEntity : TableEntity
    {
        public DateTime CreatedAt { get; set; }

        public string Temp { get; set; }

        public string CreatedAtString { get; set; }

    }
}
