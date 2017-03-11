using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPDFR_.API;
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage;

namespace GangsOfSouthLS.APIWrappers
{
    class LSPDFRPlusWrapperClass
    {
        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, List<string> Answers)
        {
            Functions.AddQuestionToTrafficStop(Suspect, Question, Answers);
        }

        public static void AddQuestionToTrafficStop(Ped Suspect, List<string> Questions, List<string> Answers)
        {
            Functions.AddQuestionToTrafficStop(Suspect, Questions, Answers);
        }

        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, string Answer)
        {
            Functions.AddQuestionToTrafficStop(Suspect, Question, Answer);
        }

        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, List<string> Answers, Action<Ped, string> Callback)
        {
            Functions.AddQuestionToTrafficStop(Suspect, Question, Answers, Callback);
        }

        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, Func<Ped, string> CallbackAnswer)
        {
            Functions.AddQuestionToTrafficStop(Suspect, Question, CallbackAnswer);
        }

        public static void CreateNewCourtCase(Persona DefendantPersona, string Crime, int GuiltyChance, int MonthsInPrison)
        {
            var years = MonthsInPrison / 12;
            var months = MonthsInPrison % 12;
            var monthstring = "";
            var yearstring = "";
            var sentencestring = "";
            if (months == 1)
            {
                monthstring = "1 month";
            }
            else
            {
                monthstring = months + " months";
            }
            if (years == 1)
            {
                yearstring = "1 year";
            }
            else
            {
                yearstring = years + " years";
            }
            if (monthstring != "0 months" && yearstring != "0 years")
            {
                sentencestring = "Sentenced to " + yearstring + " and " + monthstring + " in prison.";
            }
            else if (yearstring == "0 years")
            {
                sentencestring = sentencestring = "Sentenced to " + monthstring + " in prison.";
            }
            else
            {
                sentencestring = sentencestring = "Sentenced to " + yearstring + " in prison.";
            }
            Functions.CreateNewCourtCase(DefendantPersona, Crime, GuiltyChance, sentencestring);
        }


    }
}
