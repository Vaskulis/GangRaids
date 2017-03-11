using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using System.ComponentModel;

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
            var listlen = CrimeSentenceDict.Count;
            if (listlen == 0)
            {
                return "None";
            }
            else if (listlen == 1)
            {
                return CrimeSentenceDict.First().Key;
            }
            else
            {
                var returnString = "";
                for (int c = 0; c == listlen - 2; c++)
                {
                    returnString += (CrimeSentenceDict.Keys.ToArray()[c] + ", ");
                }
                returnString += ("and " + CrimeSentenceDict.Keys.ToArray()[listlen - 1]);
                return returnString;
            }
        }
    }
}
