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
using System.Windows.Forms;

namespace GangsOfSouthLS.Callouts
{
    [CalloutInfo("Drug Deal", CalloutProbability.Always)]
    class ProtectionRacketeering : Callout
    {
        internal static ProtectionRacketeeringScenario Scenario;
        internal static ProtectionRacketeeringScenarioScheme ScenarioScheme;
        internal static ERacketState RacketState;

        private LHandle Pursuit;
        private Blip ShopBlip;
        private Blip CarBlip;
        private Blip PassengerBlip;
        private Blip DriverBlip;
        private Blip MerchantBlip;
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
                if (firstloop)
                {
                    firstloop = false;
                    GameFiber.StartNew(delegate
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Starting check if doors are open.");
                        while (RacketState == ERacketState.Accepted || RacketState == ERacketState.ArrivedOnScene || RacketState == ERacketState.OnSceneAndWaiting)
                        {
                            foreach (var door in Scenario.DoorModelNames)
                            {
                                MyNatives.OpenDoor(door, Scenario.DoorLocation);
                            }
                            GameFiber.Wait(3000); //Make sure doors are open every 3 seconds
                            GameFiber.Yield();
                        }
                        Game.LogTrivial("[GangsOfSouthLS] Ending check if doors are open.");
                        return;
                    });
                }
                if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 100f)
                {
                    RacketState = ERacketState.ArrivedOnScene;
                    firstloop = true;
                }
            }

            if (RacketState == ERacketState.ArrivedOnScene)
            {
                Game.DisplayHelp("Wait for the ~y~suspects to arrive.");
                if (firstloop)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player arrived on scene.");
                    firstloop = false;
                    TimeBeforeCarSpawn = (UsefulFunctions.rng.Next(20)) * 1000;
                    Game.LogTrivial(string.Format("[GangsOfSouthLS] Waiting {0} s to spawn car.", (TimeBeforeCarSpawn / 1000)));

                    ShopBlip.SafelyDelete();
                    MerchantBlip = new Blip(Scenario.Merchant);
                    MerchantBlip.Scale = 0.75f;
                    MerchantBlip.Color = System.Drawing.Color.Orange;

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
                    Game.LogTrivial("[GangsOfSouthLS] Player is on the scene and waiting.");
                    firstloop = false;
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.ParkingPos4.Position, 12f, VehicleDrivingFlags.Normal, 30f).WaitForCompletion(120000);
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.ParkingPos4.Position, 8f, VehicleDrivingFlags.Normal, 15f).WaitForCompletion(120000);
                        Scenario.Driver.Tasks.ParkVehicle(Scenario.ParkingPos4.Position, Scenario.ParkingPos4.Heading).WaitForCompletion(20000);
                        InitializeBlips();

                        Scenario.Passenger.Tasks.FollowNavigationMeshToPosition(Scenario.RacketeerShopPos4.Position, Scenario.RacketeerShopPos4.Heading, 1f).WaitForCompletion(20000);
                        GameFiber.Wait(5000);
                        Scenario.Passenger.Tasks.EnterVehicle(Scenario.GangsterCar, 0, 1f).WaitForCompletion(20000);
                        ShopBlip.SafelyDelete();
                        MerchantBlip.SafelyDelete();

                        Scenario.Driver.Tasks.CruiseWithVehicle(12f, VehicleDrivingFlags.Normal);
                        RacketState = ERacketState.WaitingForPullover;
                        Game.LogTrivial("[GangsOfSouthLS] Waiting for Player to pull over the suspects.");
                        firstloop = true;
                        return;
                    });
                }
            }

            if (RacketState == ERacketState.WaitingForPullover)
            {
                if (Functions.IsPlayerPerformingPullover() && (Functions.GetPulloverSuspect(Functions.GetCurrentPullover())) == Scenario.Driver)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player started Pullover.");
                    CarBlip.SafelyDelete();
                    RacketState = ERacketState.InPullover;
                }
            }

            if (RacketState == ERacketState.InPullover)
            {
                if (!Functions.IsPlayerPerformingPullover() || !(Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == Scenario.Driver))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player aborted Pullover, waiting again.");
                    CarBlip = new Blip(Scenario.GangsterCar);
                    CarBlip.Color = System.Drawing.Color.Yellow;
                    CarBlip.Order = 2;
                    RacketState = ERacketState.WaitingForPullover;
                }
                if (!(Functions.GetActivePursuit() == null))
                {
                    Functions.ForceEndCurrentPullover();
                    Pursuit = Functions.GetActivePursuit();
                    Game.LogTrivial("[GangsOfSouthLS] LSPDFR started pursuit automatically, giving up control of Driver and making Passenger fight.");
                    DriverBlip.SafelyDelete();
                    Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(100f);
                    RacketState = ERacketState.InAutomaticPursuit;
                }
                if ((Functions.GetActivePursuit() == null) && !Game.LocalPlayer.Character.IsInAnyVehicle(false) && (Game.LocalPlayer.Character.DistanceTo(Scenario.Driver) < 3f))
                {
                    Functions.ForceEndCurrentPullover();
                    Game.LogTrivial("[GangsOfSouthLS] Making gangsters fight player.");
                    RacketState = ERacketState.Fighting;
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
                        Scenario.Driver.Tasks.FightAgainstClosestHatedTarget(100f);
                    });
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Passenger.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
                        Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(100f);
                    });
                }
            }

            if (RacketState == ERacketState.Fighting)
            {
                CleanUp();
                if (!(Functions.GetActivePursuit() == null))
                {
                    Functions.ForceEndCurrentPullover();
                    Pursuit = Functions.GetActivePursuit();
                    Game.LogTrivial("[GangsOfSouthLS] LSPDFR started pursuit automatically, giving up control of Driver and making Passenger fight.");
                    DriverBlip.SafelyDelete();
                    RacketState = ERacketState.InAutomaticPursuit;
                }
                if ((!Scenario.Passenger.IsValid() || Scenario.Passenger.IsDead) && (!Scenario.Driver.IsValid() || Scenario.Driver.IsDead))
                {
                    End();
                }
            }

            if (RacketState == ERacketState.InAutomaticPursuit)
            {
                CleanUp();
                if(!Functions.IsPursuitStillRunning(Pursuit) && (!Scenario.Passenger.IsValid() || Scenario.Passenger.IsDead || Functions.IsPedArrested(Scenario.Passenger)))
                {
                    End();
                }
            }

            base.Process();
        }

        public override void End()
        {
            RacketState = ERacketState.Ended;
            ShopBlip.SafelyDelete();
            DriverBlip.SafelyDelete();
            PassengerBlip.SafelyDelete();
            CarBlip.SafelyDelete();
            MerchantBlip.SafelyDelete();
            if (!(Scenario == null))
            {
                Scenario.GangsterCar.SafelyDelete();
                Scenario.Merchant.SafelyDelete();
                Scenario.Passenger.SafelyDelete();
                Scenario.Driver.SafelyDelete();
            }
            base.End();
        }

        internal enum ERacketState
        {
            Accepted, ArrivedOnScene, OnSceneAndWaiting, WaitingForPullover, InPullover, InAutomaticPursuit, Fighting, Ended
        }

        internal void InitializeBlips()
        {
            CarBlip = new Blip(Scenario.GangsterCar);
            CarBlip.Color = System.Drawing.Color.Yellow;
            CarBlip.Order = 2;
            PassengerBlip = new Blip(Scenario.Passenger);
            PassengerBlip.Color = System.Drawing.Color.FromArgb(224, 50, 50);
            PassengerBlip.Scale = 0.75f;
            PassengerBlip.Order = 1;
            DriverBlip = new Blip(Scenario.Driver);
            PassengerBlip.Color = System.Drawing.Color.FromArgb(224, 50, 50);
            PassengerBlip.Scale = 0.75f;
            PassengerBlip.Order = 1;
        }

        internal void CleanUp()
        {
            if (Scenario.Driver.IsValid() && Scenario.Driver.IsDead)
            {
                DriverBlip.SafelyDelete();
                Scenario.Driver.SafelyDismiss();
            }
            if (Scenario.Passenger.IsValid() && Scenario.Passenger.IsDead)
            {
                PassengerBlip.SafelyDelete();
                Scenario.Passenger.SafelyDismiss();
            }
        }
      
    }
}
