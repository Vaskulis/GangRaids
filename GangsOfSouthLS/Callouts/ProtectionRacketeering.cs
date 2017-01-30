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
                Game.DisplayHelp("Wait for the ~y~suspects ~w~to arrive.");
                if (firstloop)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player arrived on scene.");
                    firstloop = false;
                    TimeBeforeCarSpawn = (UsefulFunctions.rng.Next(10)) * 1000;
                    Game.LogTrivial(string.Format("[GangsOfSouthLS] Waiting {0} s to spawn car.", (TimeBeforeCarSpawn / 1000)));
                    MakeMerchantBlip();
                    ShopBlip.SafelyDelete();

                    GameFiber.StartNew(delegate
                    {
                        GameFiber.Wait(TimeBeforeCarSpawn);
                        Scenario.SpawnCarAndBadGuys();
                        //RacketState = ERacketState.OnSceneAndWaiting;
                        MakeCarBlip();
                        RacketState = ERacketState.WaitingForPullover;
                        firstloop = true;
                        return;
                    });
                }
            }

            if (RacketState == ERacketState.OnSceneAndWaiting)
            {
                CleanUp();
                Game.DisplayHelp("Wait for the ~y~suspects ~w~to arrive.");
                if (firstloop)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Car spawned, player is on the scene and waiting.");
                    firstloop = false;
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.ParkingPos4.Position, 12f, VehicleDrivingFlags.Normal, 30f).WaitForCompletion(120000);
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.ParkingPos4.Position, 8f, VehicleDrivingFlags.Normal, 15f).WaitForCompletion(120000);
                        Scenario.Driver.Tasks.ParkVehicle(Scenario.ParkingPos4.Position, Scenario.ParkingPos4.Heading).WaitForCompletion(20000);
                        MakeCarBlip();
                        MakePassengerBlip();

                        Scenario.Passenger.Tasks.FollowNavigationMeshToPosition(Scenario.RacketeerShopPos4.Position, Scenario.RacketeerShopPos4.Heading, 1f).WaitForCompletion(20000);
                        GameFiber.Wait(5000);
                        Scenario.Passenger.Tasks.EnterVehicle(Scenario.GangsterCar, 0, 1f).WaitForCompletion(20000);
                        PassengerBlip.Delete();
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
                CleanUp();
                Game.DisplayHelp("Pull over the ~y~suspect's car~w~.");
                if (Functions.IsPlayerPerformingPullover() && (Functions.GetPulloverSuspect(Functions.GetCurrentPullover())) == Scenario.Driver)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player started Pullover.");
                    CarBlip.SafelyDelete();
                    RacketState = ERacketState.InPullover;
                }
                if (Functions.IsPedGettingArrested(Scenario.Driver))
                {
                    MakeStuffHappenWhenArrestingTheDriver();
                }
            }

            if (RacketState == ERacketState.InPullover)
            {
                CleanUp();
                Game.DisplayHelp("Arrest the ~r~suspects~w~!");
                if (!Functions.IsPlayerPerformingPullover() || !(Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == Scenario.Driver))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Player aborted Pullover, waiting again.");
                    MakeCarBlip();
                    RacketState = ERacketState.WaitingForPullover;
                }
                if (!(Functions.GetActivePursuit() == null))
                {
                    MakeStuffHappenWhenAutomaticPursuitIsCreated();
                }
                if ((Functions.GetActivePursuit() == null) && !Game.LocalPlayer.Character.IsInAnyVehicle(false) && (Game.LocalPlayer.Character.DistanceTo(Scenario.Driver) < 6f))
                {
                    if (UsefulFunctions.Decide(35)) //Make them fight
                    {
                        MakeStuffHappenWhenStartingAFight();
                    }
                    else  //Make them drive off
                    {
                        MakeStuffHappenWhenCreatingSelfmadePursuit();
                    }
                    if (Functions.IsPedGettingArrested(Scenario.Driver))
                    {
                        MakeStuffHappenWhenArrestingTheDriver();
                    }
                }
            }

            if (RacketState == ERacketState.DriverSafelyArrested)
            {
                CleanUp();
                Game.DisplayHelp("Arrest the ~r~suspects!");
                if (UsefulFunctions.Decide(20))
                {
                    MakeStuffHappenWhenCreatingSelfmadePursuit();
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Passenger is not fleeing.");
                    RacketState = ERacketState.CanBeEnded;
                }
            }

            if (RacketState == ERacketState.CanBeEnded)
            {
                CleanUp();
                if ((!Scenario.Passenger.Exists() || Scenario.Passenger.IsDead || Functions.IsPedArrested(Scenario.Passenger) && (!Scenario.Driver.Exists() || Scenario.Driver.IsDead || Functions.IsPedArrested(Scenario.Passenger))))
                {
                    End();
                    return;
                }
            }

            if (RacketState == ERacketState.Fighting)
            {
                CleanUp();
                if (!(Functions.GetActivePursuit() == null))
                {
                    MakeStuffHappenWhenAutomaticPursuitIsCreated();
                }
            }

            if (RacketState == ERacketState.InPursuit)
            {
                CleanUp();
                if(!Functions.IsPursuitStillRunning(Pursuit) && (!Scenario.Passenger.Exists() || Scenario.Passenger.IsDead || Functions.IsPedArrested(Scenario.Passenger)))
                {
                    End();
                    return;
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
            Accepted, ArrivedOnScene, OnSceneAndWaiting, WaitingForPullover, InPullover, InPursuit, Fighting, DriverSafelyArrested, CanBeEnded, Ended
        }

        internal void MakeCarBlip()
        {
            CarBlip = new Blip(Scenario.GangsterCar);
            CarBlip.Color = System.Drawing.Color.Yellow;
            CarBlip.Order = 999;
        }

        internal void MakePassengerBlip()
        {
            PassengerBlip = new Blip(Scenario.Passenger);
            PassengerBlip.Color = System.Drawing.Color.FromArgb(224, 50, 50);
            PassengerBlip.Scale = 0.75f;
            PassengerBlip.Order = 1;
        }

        internal void MakeDriverBlip()
        {
            DriverBlip = new Blip(Scenario.Driver);
            DriverBlip.Color = System.Drawing.Color.FromArgb(224, 50, 50);
            DriverBlip.Scale = 0.75f;
            DriverBlip.Order = 1;
        }

        internal void MakeMerchantBlip()
        {
            MerchantBlip = new Blip(Scenario.Merchant);
            MerchantBlip.Scale = 0.75f;
            MerchantBlip.Color = System.Drawing.Color.Orange;
        }

        internal void CleanUp()
        {
            if (Scenario.Driver.Exists() && Scenario.Driver.IsDead)
            {
                DriverBlip.SafelyDelete();
                Scenario.Driver.SafelyDismiss();
            }
            if (Scenario.Passenger.Exists() && Scenario.Passenger.IsDead)
            {
                PassengerBlip.SafelyDelete();
                Scenario.Passenger.SafelyDismiss();
            }
            EndIfPlayerDies();
            TestEndCondition();
        }

        internal void TestEndCondition()
        {
            if ((!Scenario.Passenger.Exists() || Scenario.Passenger.IsDead || Functions.IsPedArrested(Scenario.Passenger) && (!Scenario.Driver.Exists() || Scenario.Driver.IsDead || Functions.IsPedArrested(Scenario.Passenger))))
            {
                End();
            }
        }

        internal void EndIfPlayerDies()
        {
            if (Game.LocalPlayer.Character.IsDead)
            {
                End();
            }
        }

        internal void MakeStuffHappenWhenAutomaticPursuitIsCreated()
        {
            Functions.ForceEndCurrentPullover();
            Pursuit = Functions.GetActivePursuit();
            Game.LogTrivial("[GangsOfSouthLS] LSPDFR started pursuit automatically, giving up control of Driver and making Passenger fight.");
            MakePassengerBlip();
            Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(100f);
            RacketState = ERacketState.InPursuit;
        }

        internal void MakeStuffHappenWhenCreatingSelfmadePursuit()
        {
            CleanUp();
            Functions.ForceEndCurrentPullover();
            Pursuit = Functions.CreatePursuit();
            if (!Scenario.Passenger.IsDead && Scenario.Passenger.IsInAnyVehicle(false) && (!Scenario.Passenger.CurrentVehicle.HasDriver || Functions.IsPedGettingArrested(Scenario.Driver) || Functions.IsPedArrested(Scenario.Driver) || Scenario.Driver.IsDead))
            {
                Game.LogTrivial("[GangsOfSouthLS] Starting pursuit, car has no driver, adding passenger to pursuit.");
                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(3000);
                    Scenario.Passenger.Tasks.ShuffleToAdjacentSeat().WaitForCompletion(5000);
                    if(!(Functions.GetActivePursuit() == null))
                    {
                        Pursuit = Functions.GetActivePursuit();
                    }
                    Functions.AddPedToPursuit(Pursuit, Scenario.Passenger);
                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                });
            }
            else if (!Scenario.Driver.IsDead)
            {
                Game.LogTrivial("[GangsOfSouthLS] Starting pursuit, adding driver and making passenger fight.");
                MakePassengerBlip();
                if (!Scenario.Passenger.IsDead)
                {
                    Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(100f);
                }
                if (!(Functions.GetActivePursuit() == null))
                {
                    Pursuit = Functions.GetActivePursuit();
                }
                Functions.AddPedToPursuit(Pursuit, Scenario.Driver);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
            }
            RacketState = ERacketState.InPursuit;
        }

        internal void MakeStuffHappenWhenStartingAFight()
        {
            Functions.ForceEndCurrentPullover();
            Game.LogTrivial("[GangsOfSouthLS] Making gangsters fight player.");
            RacketState = ERacketState.Fighting;
            MakePassengerBlip();
            MakeDriverBlip();
            GameFiber.StartNew(delegate
            {
                if (Scenario.Driver.IsInAnyVehicle(false))
                {
                    Scenario.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
                }
                Scenario.Driver.Tasks.FightAgainstClosestHatedTarget(100f);
            });
            GameFiber.StartNew(delegate
            {
                if (Scenario.Passenger.IsInAnyVehicle(false))
                {
                    Scenario.Passenger.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
                }
                Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(100f);
            });
        }      

        internal void MakeStuffHappenWhenArrestingTheDriver()
        {
            Game.LogTrivial("[GangsOfSouthLS] Driver is getting arrested.");
            Functions.ForceEndCurrentPullover();
            if (UsefulFunctions.Decide(30))
            {
                MakeStuffHappenWhenStartingAFight();
            }
            else
            {
                Game.LogTrivial("[GangsOfSouthLS] Driver was arrested safely.");
                RacketState = ERacketState.DriverSafelyArrested;
            }
        }
    }
}
