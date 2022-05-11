using System;

namespace WorkdayLoggerBot.Models
{
    public class WorkdayModel
    {

        public string Project { get; set; }
        public string Task { get; set; }
        public string Comment { get; set; }
        public int Hours { get; set; }
        public DateTime Day { get; set; }
        public Boolean Overtime { get; set; }
        public Boolean OnSite { get; set; }
        public Boolean isDaySet { get; set; } = false;


        public WorkdayModel(string project, string task, string comment, DateTime d, Boolean overtime, Boolean onsite)
        {
            Project = project;
            Task = task;
            Comment = comment;
            Day = d;
            Overtime = overtime;
            OnSite = onsite;
        }

        public WorkdayModel() {
            Hours = 0;
        }

        public string toString()
        {
            string onsite = OnSite ? "igen" : "nem";
            string ot = Overtime ? "igen" : "nem";
            return "Projekt: " + Project +
                "  \nTask: " + Task +
                "  \nKomment: " + Comment +
                "  \nNap: " + Day +
                "  \nÓra: " + Hours + 
                "  \nTúlóra: " + ot +
                "  \nOnSite: " + onsite;
        }

    }
}
