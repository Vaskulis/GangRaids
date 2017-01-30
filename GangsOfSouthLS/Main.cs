using LSPD_First_Response.Mod.API;
using Rage;
using GangsOfSouthLS.Menus;
using GangsOfSouthLS.INIFile;

[assembly: Rage.Attributes.Plugin("GangsOfSouthLS", Description = "Vaskulis' GangsOfSouthLS", Author = "Vaskulis")]

namespace GangsOfSouthLS
{
    internal class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("Initializing GangsOfSouthLS plugin.");
        }
        public override void Finally()
        {
            Game.LogTrivial("GangsOfSouthLS has been cleaned up.");
        }
        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                if (INIReader.LoadINIFile())
                {
                    
                    RegisterCallouts();
                    DrugDealMenu.InitializeAndProcess();
                }
            }
        }
        private static void RegisterCallouts()
        {
            //Functions.RegisterCallout(typeof(Callouts.DrugDeal));
            Functions.RegisterCallout(typeof(Callouts.ProtectionRacketeering));
        }
    }
}
