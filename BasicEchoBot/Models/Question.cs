using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicEchoBot.Models
{
    public class Question
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public Branch Branch { get; set; }
    }
}
