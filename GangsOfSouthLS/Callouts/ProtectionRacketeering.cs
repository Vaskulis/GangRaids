using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers;
using GangsOfSouthLS.INIFile;

namespace GangsOfSouthLS.Callouts
{
    [CalloutInfo("Drug Deal", CalloutProbability.Always)]
    class ProtectionRacketeering : Callout
    {
        internal static ProtectionRacketeeringScenario Scenario;
        internal static ProtectionRacketeeringScenarioScheme ScenarioScheme;
        internal static ERacketState RacketState;

        private Blip ShopBlip;
        private int TimeBeforeCarSpawn;
        private bool firstloop = true;

        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = ProtectionRacketeeringScenarioScheme.ChooseScenario(out ScenarioScheme);
            if (!scenarioFound)
            {
                Game.LogTrivial("[GangsOfSouthLS] Could not find scenario in range.");
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
                    Game.LogTrivial("[GangsOfSouthLS] Player arrived on scene.");
                    RacketState = ERacketState.ArrivedOnScene;
                }

            }

            if (RacketState == ERacketState.ArrivedOnScene)
            {
                if (firstloop)
                {
                    firstloop = false;
                    TimeBeforeCarSpawn = (UsefulExtensions.rng.Next(30) + 10) * 1000;
                    Game.LogTrivial(string.Format("[GangsOfSouthLS] Waiting {0} s to spawn car.", (TimeBeforeCarSpawn / 1000)));
                    GameFiber.StartNew(delegate
                    {
                        GameFiber.Wait(TimeBeforeCarSpawn);
                        Scenario.SpawnCarAndBadGuys();
                        RacketState = ERacketState.OnSceneAndWaiting;
                        firstloop = true;
                        return;
                    });
                }
            }

            if (RacketState == ERacketState.OnSceneAndWaiting)
            {
                if (firstloop)
                {
                    firstloop = false;
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.ParkingPos4.Position, 15f, VehicleDrivingFlags.Normal, 15f).WaitForCompletion(120000);
                        Scenario.Driver.Tasks.ParkVehicle(Scenario.ParkingPos4.Position, Scenario.ParkingPos4.Heading).WaitForCompletion(20000);
                        Scenario.Racketeer.Tasks.FollowNavigationMeshToPosition(Scenario.RacketeerShopPos4.Position, Scenario.RacketeerShopPos4.Heading, 1f).WaitForCompletion(20000);
                        GameFiber.Wait(5000);
                        Scenario.Racketeer.Tasks.EnterVehicle(Scenario.GangsterCar, 0, 1f).WaitForCompletion(20000);
                        Scenario.Driver.Tasks.CruiseWithVehicle(15f, VehicleDrivingFlags.Normal);
                        firstloop = true;
                        End();
                        return;
                    });

                }
            }

            base.Process();
        }

        public override void End()
        {
            if(!(ShopBlip == null) && ShopBlip.IsValid())
            {
                ShopBlip.Delete();
            }
            if (!(Scenario == null))
            {
                if (!(Scenario.GangsterCar == null) && Scenario.GangsterCar.IsValid())
                {
                    Scenario.GangsterCar.Delete();
                }
                if (!(Scenario.Merchant == null) && Scenario.Merchant.IsValid())
                {
                    Scenario.Merchant.Delete();
                }
                if (!(Scenario.Racketeer == null) && Scenario.Racketeer.IsValid())
                {
                    Scenario.Racketeer.Delete();
                }
                if (!(Scenario.Driver == null) && Scenario.Driver.IsValid())
                {
                    Scenario.Driver.Delete();
                }
            }
            base.End();
        }

        internal enum ERacketState
        {
            Accepted, ArrivedOnScene, OnSceneAndWaiting
        }
      
    }
}
