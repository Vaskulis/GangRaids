using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using System.ComponentModel;
using GangsOfSouthLS.APIWrappers;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting.Entities;
using System.Drawing;
using GangsOfSouthLS.Menus;

namespace GangsOfSouthLS.HelperClasses.CommonUtilities
{
    public class MyPed : Ped
    {
        private Persona persona;

        public Dictionary<string, int> CrimeSentenceDict { get; set; }
        public int PrisonSentence { get { return CrimeSentenceDict.Values.ToArray().Sum(); } }
        public string Name { get { return persona.FullName; } }
        public string Gender { get { return persona.Gender.ToString(); } }
        public int Age { get; private set; }
        public DateTime Birthday { get { return persona.BirthDay.Date; } }
        public Blip Blip { get; private set; }
        public EType Type { get; set; }
        public Conversation Conversation { get; private set; }


        public MyPed(Model model, Vector3 position, float heading, EType type = EType.Default) : base(model, position, heading)
        {
            CrimeSentenceDict = new Dictionary<string, int> { };
            persona = Functions.GetPersonaForPed(this);
            Type = type;
            CalculateAge();
        }


        public MyPed(Vector3 position, float heading, EType type = EType.Default) : base(position, heading)
        {
            CrimeSentenceDict = new Dictionary<string, int> { };
            persona = Functions.GetPersonaForPed(this);
            Type = type;
            CalculateAge();
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


        public void AddBlip()
        {
            Blip.SafelyDelete();
            Blip = new Blip(this);
            if (Type == EType.Suspect)
            {
                Blip.Color = Color.FromArgb(224, 50, 50);
            }
            else if (Type == EType.Witness)
            {
                Blip.Color = Color.Orange;
            }
            else if (Type == EType.Unknown)
            {
                Blip.Color = Color.Yellow;
            }
            else if (Type == EType.Default)
            {
                Blip.Color = Color.White;
            }
            Blip.Scale = 0.75f;
            Blip.Order = 0;
        }


        public void StartNewConversation()
        {
            if (Conversation != null)
            {
                Conversation.Terminate();
            }
            Conversation = new Conversation(this);
        }


        public enum EType
        {
            Default,
            Suspect,
            Witness,
            Unknown
        }


        public override void Delete()
        {
            Blip.SafelyDelete();
            base.Delete();
        }


        public void Dismiss(bool deleteBlip=true)
        {
            if (deleteBlip)
            {
                Blip.SafelyDelete();
            }
            base.Dismiss();
        }


        private void CalculateAge()
        {
            var today = World.DateTime;
            Age = today.Year - persona.BirthDay.Year;
            if (persona.BirthDay.AddYears(Age) > today)
            {
                Age--;
            }
        }

    }
}
