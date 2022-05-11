using System;
using System.Collections.Generic;
using WorkdayLoggerBot.Models;

namespace WorkdayLoggerBot.Services
{
    public class WorkdayHandlerService
    {
        public string result = "";
        public bool newData = false;
        public List<string> Questions = new List<string>();
        public int i = 0;
        public WorkdayModel workdayModel = null;

        public static string Order { get; set; }



        public WorkdayHandlerService()
        {
            AddQuestions();
        }

        public void AddQuestions()
        {
            Questions.Add("Projekt neve?");
            Questions.Add("Task címe?");
            Questions.Add("Komment?");
            Questions.Add("Melyik nap?");
            Questions.Add("Túlóra? igen/nem");
            Questions.Add("OnSite? igen/nem");
        }

        public void addDataToResult(string data)
        {
            result += data + " ";
        }


        public Boolean castStringToBool(String str)
        {
            if(str == "igen")
            {
                return true;
            }
            return false;
        }


        public void setWorkdayModelData()
        {
            workdayModel = null;
            string[] res = result.Split(" ");
            bool ot = castStringToBool(res[4]);
            bool onsite = castStringToBool(res[5]);
           // workdayModel = new WorkdayModel(res[0], res[1], res[2], res[3], ot, onsite);
        }

        public string writeQuestionToUser()
        {
            return Questions[i];
        }

        public void resetData()
        {
            result = "";
            newData = false;
            i = 1;
            workdayModel = null;
        }


    }
}
