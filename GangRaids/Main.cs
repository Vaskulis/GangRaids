using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Rage;
using GangRaids.Menus;

[assembly: Rage.Attributes.Plugin("Gang Raids", Description = "Vaskulis' Gang Raids", Author = "Vaskulis")]

namespace GangRaids
{
    public class Main : Plugin
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
                RegisterCallouts();
                DrugDealMenu.InitializeAndProcess();
            }
        }
        private static void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.DrugDeal));
        }
    }
}
