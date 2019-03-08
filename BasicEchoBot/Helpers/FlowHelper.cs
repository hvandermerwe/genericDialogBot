using BasicEchoBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicEchoBot.Helpers
{
    public static class FlowHelper
    {
        public static List<Flow> InitFlow(string json)
        {
            JObject jObject = JObject.Parse(json);
            return null;
        }
    }
}
