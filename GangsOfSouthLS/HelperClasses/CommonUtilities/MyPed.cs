using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using System.ComponentModel;
using GangsOfSouthLS.APIWrappers;
using LSPD_First_Response.Mod.API;

namespace GangsOfSouthLS.HelperClasses.CommonUtilities
{
    public class MyPed : Ped
    {
        public Dictionary<string, int> CrimeSentenceDict { get; set; }
        public int PrisonSentence { get { return CrimeSentenceDict.Values.ToArray().Sum(); } }

        public MyPed(Model model, Vector3 position, float heading) : base(model, position, heading)
        {
            CrimeSentenceDict = new Dictionary<string, int> { };
        }

        public void AddCrimeToList(string Crime, int MonthsPrison)
        {
            if (!CrimeSentenceDict.Keys.Contains(Crime))
            {
                CrimeSentenceDict.Add(Crime, MonthsPrison);
            }
            else
            {
                CrimeSentenceDict[Crime] += MonthsPrison;
            }
        }

        public string GetCrimesString()
        {
            var CrimesList = CrimeSentenceDict.Keys.ToList();
            var listlen = CrimesList.Count();
            if (listlen == 0)
            {
                return "None";
            }
            else if (listlen == 1)
            {
                return CrimesList[0];
            }
            else
            {
                var returnString = "";
                for (int c = 0; c <= listlen - 2; c++)
                {
                    returnString += (CrimesList[c] + ", ");
                }
                returnString += ("and " + CrimesList[listlen - 1]);
                return returnString;
            }
        }
         public void CreateCourtCase(bool IsLSPDFRPlusRunning)
        {
            if (!IsLSPDFRPlusRunning || GetCrimesString() == "None")
            {
                return;
            }
            if (PrisonSentence == 0)
            {
                LSPDFRPlusWrapperClass.CreateNewCourtCase(Functions.GetPersonaForPed(this), GetCrimesString(), 0, 0);
            }
            else
            {
                foreach (var crime in CrimeSentenceDict)
                {
                    Game.LogTrivial("Crime: " + crime.Key + "Sentence: " + crime.Value);
                }
                var RandomPrisonSentence = ( (UsefulFunctions.rng.Next(90, 110) / 100.0) * PrisonSentence ) ;
                LSPDFRPlusWrapperClass.CreateNewCourtCase(Functions.GetPersonaForPed(this), GetCrimesString(), 100, (int)RandomPrisonSentence);
            }
        }
    }
}
