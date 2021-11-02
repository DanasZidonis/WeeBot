using Newtonsoft.Json;
using System.Collections.Generic;

namespace WeeBot.Models
{


    public partial class JsonInfo
    {
        [JsonProperty("Data")]
        public List<Data> Test { get; set; }

    }

    public class Data
    {
        public string Link { get; set; }
    }
}
