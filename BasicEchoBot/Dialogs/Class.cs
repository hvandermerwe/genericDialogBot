using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicEchoBot.Dialogs
{
    public class Class
    {

        public class Flow
        {
            public string flowID { get; set; }
            public List<Question> questions { get; set; }

            public Flow()
            {
                flowID = "UserDetails";
                questions = new List<Question>();

                Question question1 = new Question { Type = "Text", Value = "fname", Text = "Enter your name", Branch = new Branch{ FlowId = "GreetRian", Text="Rian"} };
                Question question2 = new Question { Type = "Text", Value = "lname", Text = "Enter your surname" };
                Question question3 = new Question { Type = "Text", Value = "email", Text = "Enter your email" };
                Question question4 = new Question { Type = "Date", Value = "dob", Text = "Enter your date of birth" };

                questions.Add(question1);
                questions.Add(question2);
                questions.Add(question3);
                questions.Add(question4);
            }
        }

        public class Question
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public string Text { get; set; }
            public Branch Branch { get; set; }
        }

        public class Branch
        {
            public string FlowId { get; set; }
            public string Text { get; set; }
        }

    }
}
