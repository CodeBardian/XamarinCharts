using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace ArduinoMonitor
{
    public class Record
    {
        private int id;
        private double temp;
        private double humidity;
        private string time;
        private string date;

        public double Temp { get => temp; set => temp = value; }
        public double Humidity { get => humidity; set => humidity = value; }
        public string Time { get => time; set => time = value; }
        public string Date { get => date; set => date = value; }

    }

    public class Records
    {
        [JsonProperty("records")]
        public List<Record> RecordList { get; set; }
    }

}
