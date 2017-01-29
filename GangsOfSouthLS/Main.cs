using LSPD_First_Response.Mod.API;
using Rage;
using GangRaids.Menus;
using GangRaids.INIFile;

[assembly: Rage.Attributes.Plugin("Gang Raids", Description = "Vaskulis' Gang Raids", Author = "Vaskulis")]

namespace GangRaids
{
    internal class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("Initializing Gang Raids plugin.");
        }
        public override void Finally()
        {
            Game.LogTrivial("Gang Raids has been cleaned up.");
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
            Functions.RegisterCallout(typeof(Callouts.DrugDeal));
            //Functions.RegisterCallout(typeof(Callouts.ProtectionRacketeering));
        }
    }
}
