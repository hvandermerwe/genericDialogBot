using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicEchoBot.Models
{
    public class Flow
    {
        public string flowID { get; set; }
        public List<Question> questions { get; set; }
    }
}
