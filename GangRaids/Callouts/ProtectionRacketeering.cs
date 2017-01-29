using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using GangRaids.HelperClasses.ProtectionRacketeeringHelpers;
using GangRaids.INIFile;

namespace GangRaids.Callouts
{
    [CalloutInfo("Drug Deal", CalloutProbability.High)]
    class ProtectionRacketeering : Callout
    {
        internal static ProtectionRacketeeringScenario Scenario;
        internal static ProtectionRacketeeringScenarioScheme ScenarioScheme;
        internal static ERacketState RacketState;

        private Blip ShopBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = ProtectionRacketeeringScenarioScheme.ChooseScenario(out ScenarioScheme);
            if (!scenarioFound)
            {
                Game.LogTrivial("[GANG RAIDS] Could not find scenario in range.");
                return false;
            }
            Scenario = new ProtectionRacketeeringScenario(ScenarioScheme);
            CalloutMessage = "Protection Racketeering";
            CalloutPosition = Scenario.Position;
            Functions.PlayScannerAudioUsingPosition(string.Format("DISP_ATTENTION_UNIT_01 {0} ASSISTANCE_REQUIRED FOR CRIME_GANGACTIVITYINCIDENT IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", INIReader.UnitName), CalloutPosition);
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 50f);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            RacketState = ERacketState.Accepted;
            Scenario.Initialize();
            ShopBlip = new Blip(Scenario.Position, 40f);
            ShopBlip.Color = System.Drawing.Color.Yellow;
            ShopBlip.Alpha = 0.5f;
            ShopBlip.IsRouteEnabled = true;
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if (RacketState == ERacketState.Accepted)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 100f)
                {
                    Game.LogTrivial("[GANG RAIDS] Player arrived on scene.");
                    RacketState = ERacketState.OnScene;
                }

            }

            if (RacketState == ERacketState.OnScene)
            {

            }

            base.Process();
        }

        public override void End()
        {

            base.End();
        }

        internal enum ERacketState
        {
            Accepted, OnScene
        }
      
    }
}
