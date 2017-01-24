﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using GangRaids.HelperClasses;
using GangRaids.Scenarios;

namespace GangRaids.Callouts
{
    [CalloutInfo("Drug Deal", CalloutProbability.VeryHigh)]
    class DrugDeal : Callout
    {
        public static DrugDealScenario Scenario;
        public static DrugDealScenarioScheme ScenarioScheme;
        public static Vector3 PlayerStartPosition;
        public static Vector3 PlayerEndPosition;
        private LHandle Pursuit;
        private bool DealersAndBuyersHateEachOther;
        private bool Buyer2IsFightingStraightAway;
        private bool HasAnyPedBeenAddedToPursuit = false;

        private Blip Dealer1Blip;
        private Blip Dealer2Blip;
        private Blip Dealer3Blip;
        private Blip Buyer1Blip;
        private Blip Buyer2Blip;
        private Dictionary<Ped, Blip> PedBlipDict;

        public static Blip PlayerStartPointBlip;
        public static Blip PlayerEndPointBlip;
        public static Blip SuspectsAreaBlip;
        public static EDrugDealState DrugDealState;
        public static bool IsCurrentlyRunning = false;

        private bool IsWaitTimeOver;


        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = DrugDealScenarioScheme.ChooseScenario(out ScenarioScheme);
            if (!scenarioFound)
            {
                Game.LogTrivial("Could not find scenario in range.");
                return false;
            }
            Scenario = new DrugDealScenario(ScenarioScheme);
            CalloutMessage = "Drug deal in progress";
            CalloutPosition = Scenario.Position;
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS WE_HAVE CRIME_DRUGDEAL IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", Scenario.Position);
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 70f);
            DealersAndBuyersHateEachOther = false;
            Buyer2IsFightingStraightAway = false;
            return base.OnBeforeCalloutDisplayed();
        }


        public override bool OnCalloutAccepted()
        {
            Scenario.Initialize();
            IsCurrentlyRunning = true;
            PedBlipDict = new Dictionary<Ped, Blip> { };
            DrugDealState = EDrugDealState.Accepted;
            SuspectsAreaBlip = new Blip(Scenario.Position, 50f);
            SuspectsAreaBlip.Alpha = 0.5f;
            SuspectsAreaBlip.Color = System.Drawing.Color.Yellow;
            SuspectsAreaBlip.IsRouteEnabled = true;
            return base.OnCalloutAccepted();
        }


        public override void Process()
        {
            base.Process();

            if (DrugDealState == EDrugDealState.Accepted)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 200f)
                {
                    SuspectsAreaBlip.DisableRoute();
                    DrugDealState = EDrugDealState.InPreparation;
                }
            }

            if (DrugDealState == EDrugDealState.InPreparation)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(PlayerStartPosition) > 7f)
                {
                    Game.DisplayHelp("Get into ~p~position ~w~or press ~b~L ~w~to open the menu. Be careful not to get to close to the ~y~suspects~w~!");
                }
                else
                {
                    Game.DisplayHelp("Press ~b~Y ~w~to engage suspects.");
                    if (Game.IsKeyDown(Keys.Y))
                    {
                        if (!(PlayerStartPointBlip == null) && PlayerStartPointBlip.Exists())
                        {
                            PlayerStartPointBlip.Delete();
                        }
                        DrugDealState = EDrugDealState.EngagingSuspects;
                    }
                }
                if (IsPlayerTooCloseToSuspects())
                {
                    Game.DisplayHelp("The ~r~suspects ~w~saw you, it's now or never!");
                    DrugDealState = EDrugDealState.Arrived;
                }
            }

            if (DrugDealState == EDrugDealState.EngagingSuspects)
            {
                PlayerEndPointBlip = new Blip(PlayerEndPosition);
                PlayerEndPointBlip.Color = System.Drawing.Color.Purple;
                                foreach (var veh in Scenario.CopCarDict.Keys)
                {
                    veh.Driver.Tasks.DriveToPosition(Scenario.CopCarDict[veh].endPoint.Position, 12f, VehicleDrivingFlags.Emergency, 1f);
                }
                IsWaitTimeOver = false;
                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(Scenario.WaitTime);
                    IsWaitTimeOver = true;
                    Game.LogTrivial("Wait time is over.");
                    return;
                });
                DrugDealState = EDrugDealState.Waiting;
            }

            if (DrugDealState == EDrugDealState.Waiting)
            {
                Game.DisplayHelp("Go get ~y~them~w~!");
                if (IsWaitTimeOver || Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 40f)
                {
                    foreach (var veh in Scenario.CopCarDict.Keys)
                    {
                        veh.IsSirenOn = true;
                    }
                    Game.LogTrivial("Engaging Suspects");
                    DrugDealState = EDrugDealState.Arrived;
                }
            }

            if (DrugDealState == EDrugDealState.Arrived)
            {
                if (!(SuspectsAreaBlip == null) && SuspectsAreaBlip.Exists())
                {
                    SuspectsAreaBlip.Delete();
                }
                if (!(PlayerEndPointBlip == null) && PlayerEndPointBlip.Exists())
                {
                    PlayerEndPointBlip.Delete();
                }
                if (!(PlayerStartPointBlip == null) && PlayerStartPointBlip.Exists())
                {
                    PlayerStartPointBlip.Delete();
                }

                Pursuit = Functions.CreatePursuit();
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "COP", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "COP", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "DRUGDEAL_BUYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "DRUGDEAL_DEALER", Relationship.Hate);
                if (UsefulExtensions.Decide(20))
                {
                    Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_DEALER", "DRUGDEAL_BUYER", Relationship.Hate);
                    Game.SetRelationshipBetweenRelationshipGroups("DRUGDEAL_BUYER", "DRUGDEAL_DEALER", Relationship.Hate);
                    DealersAndBuyersHateEachOther = true;
                    Game.LogTrivial("Made Dealers and Buyers hate each other.");
                }
                MakeDealer2DoStuff();
                MakeDealer1DoStuff();
                MakeBuyer2DoStuff();
                MakeBuyer1DoStuff();
                if (Scenario.Dealer3WasSpawned)
                {
                    MakeDealer3DoStuff();
                }
                if (!(Functions.GetPursuitPeds(Pursuit) == null) && !(Functions.GetPursuitPeds(Pursuit).Length == 0))
                {
                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                    foreach (var cop in Scenario.CopList1)
                    {
                        Functions.AddCopToPursuit(Pursuit, cop);
                    }
                    foreach (var cop in Scenario.CopList2)
                    {
                        Functions.AddCopToPursuit(Pursuit, cop);
                    }
                }
                else
                {
                    foreach (var cop in Scenario.CopList1)
                    {
                        cop.Tasks.FightAgainstClosestHatedTarget(150f);
                    }
                    foreach (var cop in Scenario.CopList2)
                    {
                        cop.Tasks.FightAgainstClosestHatedTarget(150f);
                    }
                }
                DrugDealState = EDrugDealState.CanBeEnded;
            }


            if (DrugDealState == EDrugDealState.CanBeEnded)
            {
                CleanUpBlips();
                if (HasAnyPedBeenAddedToPursuit && !Functions.IsPursuitStillRunning(Pursuit))
                {
                    Game.LogTrivial("Pursuit is not running any more, ending.");
                    End();
                }
                else if (HasAnyPedBeenAddedToPursuit && (Functions.GetActivePursuit() == null))
                {
                    Game.LogTrivial("Player left pursuit, ending.");
                    End();
                }
                else if (!HasAnyPedBeenAddedToPursuit && !IsAnySuspectStillAliveAndNotArrested())
                {
                    Game.LogTrivial("Every suspect is either dead or arrested, ending.");
                    End();
                }
            }
        }


        public override void End()
        {
            if (!(SuspectsAreaBlip == null) && SuspectsAreaBlip.Exists())
            {
                SuspectsAreaBlip.Delete();
            }
            if (!(PlayerStartPointBlip == null) && PlayerStartPointBlip.Exists())
            {
                PlayerStartPointBlip.Delete();
            }
            if (!(PlayerEndPointBlip == null) && PlayerEndPointBlip.Exists())
            {
                PlayerEndPointBlip.Delete();
            }

            IsCurrentlyRunning = false;

            if (!(PedBlipDict == null))
            {
                foreach (var blip in PedBlipDict.Values)
                {
                    if (blip.Exists())
                    {
                        blip.Delete();
                    }
                }
            }
            if (!(Scenario.DealerList == null))
            {
                foreach (var dealer in Scenario.DealerList)
                {
                    if (dealer.Exists())
                    {
                        dealer.Dismiss();
                    }
                }
            }
            if (!(Scenario.BuyerList == null))
            {
                foreach (var Buyer in Scenario.BuyerList)
                {
                    if (Buyer.Exists())
                    {
                        Buyer.Dismiss();
                    }
                }
            }
            if (!(Scenario.CopList1 == null))
            {
                foreach (var Cop in Scenario.CopList1)
                {
                    if (Cop.Exists())
                    {
                        Cop.Dismiss();
                    }
                }
            }
            if (!(Scenario.CopList2 == null))
            {
                foreach (var Cop in Scenario.CopList2)
                {
                    if (Cop.Exists())
                    {
                        Cop.Dismiss();
                    }
                }
            }
            if (!(Scenario.BadBoyCarList == null))
            {
                foreach (var veh in Scenario.BadBoyCarList)
                {
                    if (veh.Exists())
                    {
                        veh.Dismiss();
                    }
                }
            }
            if (!(Scenario.CopCarDict == null))
            {
                foreach (var veh in Scenario.CopCarDict.Keys)
                {
                    foreach (var cop in veh.Occupants)
                    {
                        if (cop.Exists())
                        {
                            cop.Dismiss();
                        }
                    }
                    if (veh.Exists())
                    {
                        veh.Dismiss();
                    }
                }
            }
            base.End();
        }


        public enum EDrugDealState
        {
            Accepted, InPreparation, EngagingSuspects, CanBeEnded, Arrived, Waiting
        }


        private void MakeDealer2DoStuff()
        {
            if (UsefulExtensions.Decide(30))
            {
                Game.LogTrivial("Decided to make Dealer2 try to enter van.");
                GameFiber.StartNew(delegate
                {
                    Scenario.Dealer2.Tasks.EnterVehicle(Scenario.DealerVan, -1, 3f);
                    var counter = 0;
                    while (!Scenario.Dealer2.IsInAnyVehicle(true) && counter <= 4000)
                    {
                        counter += 500;
                        GameFiber.Wait(500);
                        Game.LogTrivial(string.Format("Waited {0} ms for Dealer2 to get into van.", counter));
                    }
                    if (Scenario.Dealer2.IsInAnyVehicle(true))
                    {
                        Game.LogTrivial("Dealer2 made it into van, adding to pursuit.");
                        HasAnyPedBeenAddedToPursuit = true;
                        Functions.AddPedToPursuit(Pursuit, Scenario.Dealer2);
                    }
                    else
                    {
                        Game.LogTrivial("Dealer2 did NOT make it into van, making him fight.");
                        MakeFightAndCreateBlip(Scenario.Dealer2, Dealer2Blip);
                    }
                    return;
                });
            }
            else
            {
                if (UsefulExtensions.Decide(80))
                {
                    Game.LogTrivial("Decided to make Dealer2 fight straight away.");
                    MakeFightAndCreateBlip(Scenario.Dealer2, Dealer2Blip);
                }
                else
                {
                    Game.LogTrivial("Decided to make Dealer2 run away, adding to pursuit.");
                    HasAnyPedBeenAddedToPursuit = true;
                    Functions.AddPedToPursuit(Pursuit, Scenario.Dealer2);
                }
            }
        }


        private void MakeDealer1DoStuff()
        {
            if (Scenario.Dealer1.IsInAnyVehicle(false))
            {
                Game.LogTrivial("Dealer1 is in car.");
                if (UsefulExtensions.Decide(70))
                {
                    Game.LogTrivial("Decided to make Dealer1 flee in car, adding to pursuit");
                    HasAnyPedBeenAddedToPursuit = true;
                    Functions.AddPedToPursuit(Pursuit, Scenario.Dealer1);
                }
                else
                {
                    Game.LogTrivial("Decided to make Dealer1 exit car and fight");
                    MakeFightAndCreateBlip(Scenario.Dealer1, Dealer1Blip);
                }
            }
            else
            {
                Game.LogTrivial("Dealer1 is NOT in car.");
                if (UsefulExtensions.Decide(60))
                {
                    Game.LogTrivial("Decided to make Dealer1 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Dealer1.Tasks.EnterVehicle(Scenario.DealerCar, -1, 3f);
                        var counter = 0;
                        while(!Scenario.Dealer1.IsInAnyVehicle(true) && counter <= 4000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("Waited {0} ms for Dealer1 to get into car.", counter));
                        }
                        if (Scenario.Dealer1.IsInAnyVehicle(true))
                        {
                            Game.LogTrivial("Dealer1 made it into car, adding to pursuit.");
                            HasAnyPedBeenAddedToPursuit = true;
                            Functions.AddPedToPursuit(Pursuit, Scenario.Dealer1);
                        }
                        else
                        {
                            Game.LogTrivial("Dealer1 did NOT make it into car, making him fight.");
                            MakeFightAndCreateBlip(Scenario.Dealer1, Dealer1Blip);
                        }
                        return;
                    });
                }
                else
                {
                    Game.LogTrivial("Decided to make Dealer1 fight straight away.");
                    MakeFightAndCreateBlip(Scenario.Dealer1, Dealer1Blip);
                }
            }
        }


        private void MakeDealer3DoStuff()
        {
            Game.LogTrivial("Dealer3 always fights straight away.");
            MakeFightAndCreateBlip(Scenario.Dealer3, Dealer3Blip);
        }


        private void MakeBuyer1DoStuff()
        {
            if (Buyer2IsFightingStraightAway)
            {
                if (UsefulExtensions.Decide(70))
                {
                    Game.LogTrivial("Decided to make Buyer1 join Buyer2 in combat");
                    MakeFightAndCreateBlip(Scenario.Buyer1, Buyer1Blip);
                }
                else
                {
                    Game.LogTrivial("Decided to make Buyer1 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Buyer1.Tasks.EnterVehicle(Scenario.BuyerCar, -1, 3f);
                        var counter = 0;
                        while (!Scenario.Buyer1.IsInAnyVehicle(true) && counter <= 4000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("Waited {0} ms for Buyer1 to get into car.", counter));
                        }
                        if (Scenario.Buyer1.IsInAnyVehicle(true))
                        {
                            Game.LogTrivial("Buyer1 made it into car, adding to pursuit.");
                            HasAnyPedBeenAddedToPursuit = true;
                            Functions.AddPedToPursuit(Pursuit, Scenario.Buyer1);
                        }
                        else
                        {
                            Game.LogTrivial("Buyer1 did NOT make it into car, making him fight.");
                            MakeFightAndCreateBlip(Scenario.Buyer1, Buyer1Blip);
                        }
                        return;
                    });
                }
            }
            else
            {
                Game.LogTrivial("Making Buyer1 enter car and wait for Buyer2");
                GameFiber.StartNew(delegate 
                {
                    Scenario.Buyer1.Tasks.EnterVehicle(Scenario.BuyerCar, 0, 3f);
                    var counter = 0;
                    while(!Scenario.Buyer1.IsInAnyVehicle(false) && counter <= 4000)
                    {
                        counter += 500;
                        GameFiber.Wait(500);
                        Game.LogTrivial(string.Format("Waited {0} ms for Buyer1 to get into car.", counter));
                    }
                    if (Scenario.Buyer1.IsInAnyVehicle(false))
                    {
                        Game.LogTrivial("Buyer1 made it into car, waiting for Buyer2.");
                        while(!Scenario.Buyer2.IsInAnyVehicle(false) && counter <= 5000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("Buyer1 Waited {0} ms for Buyer2 to get in car.", counter));
                        }
                        if (Scenario.Buyer2.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("Both Buyers are in car, making Buyer1 shoot out of the vehicle.");
                            NativeFunction.Natives.SetPedCombatAttributes(Scenario.Buyer1, 1, true);
                            MakeFightAndCreateBlip(Scenario.Buyer1, Buyer1Blip);
                            GameFiber.StartNew(delegate 
                            {
                                while(Scenario.Buyer1.IsInAnyVehicle(false) && !Scenario.Buyer1.CurrentVehicle.IsSeatFree(-1) && !Scenario.Buyer1.CurrentVehicle.Driver.IsDead)
                                {
                                    GameFiber.Yield();
                                }
                                Game.LogTrivial("Buyer1's driver died.");
                                if (UsefulExtensions.Decide(50))
                                {
                                    Game.LogTrivial("Decided to make Buyer1 flee.");
                                    if (HasAnyPedBeenAddedToPursuit && Functions.IsPursuitStillRunning(Pursuit))
                                    {
                                        Game.LogTrivial("Pursuit is still active, adding Buyer1.");
                                        Functions.AddPedToPursuit(Pursuit, Scenario.Buyer1);
                                    }
                                    else
                                    {
                                        Game.LogTrivial("Pursuit is not active anymore, making Buyer1 fight instead");
                                    }
                                }
                                else
                                {
                                    Game.LogTrivial("Decided to make Buyer1 fight.");
                                }
                                return;
                            });
                        }
                        else
                        {
                            Game.LogTrivial("Buyer1 is alone in car.");
                            if (Functions.IsPursuitStillRunning(Pursuit))
                            {
                                Game.LogTrivial("Pursuit is active, making Buyer1 shuffle to drivers seat and adding him to pursuit.");
                                Scenario.Buyer1.Tasks.ShuffleToAdjacentSeat().WaitForCompletion();
                                Functions.AddPedToPursuit(Pursuit, Scenario.Buyer1);
                                HasAnyPedBeenAddedToPursuit = true;
                            }
                            else
                            {
                                Game.LogTrivial("Pursuit isn't active, making Buyer1 fight.");
                                MakeFightAndCreateBlip(Scenario.Buyer1, Buyer1Blip);
                            }
                        }
                    }
                    else
                    {
                        Game.LogTrivial("Buyer1 did NOT make it into car, making him fight.");
                        MakeFightAndCreateBlip(Scenario.Buyer1, Buyer1Blip);
                    }
                    return;
                });
            }
        }


        private void MakeBuyer2DoStuff()
        {
            if (DealersAndBuyersHateEachOther)
            {
                Game.LogTrivial("Dealers and Buyers hate each other, so made Buyer2 fight straight away.");
                MakeFightAndCreateBlip(Scenario.Buyer2, Buyer2Blip);
                Buyer2IsFightingStraightAway = true;
            }
            else
            {
                if (UsefulExtensions.Decide(70))
                {
                    Game.LogTrivial("Decided to make Buyer2 try to enter car.");
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Buyer2.Tasks.EnterVehicle(Scenario.BuyerCar, -1, 3f);
                        var counter = 0;
                        while (!Scenario.Buyer2.IsInAnyVehicle(true) && counter <= 4000)
                        {
                            counter += 500;
                            GameFiber.Wait(500);
                            Game.LogTrivial(string.Format("Waited {0} ms for Buyer2 to get into car.", counter));
                        }
                        if (Scenario.Buyer2.IsInAnyVehicle(true))
                        {
                            Game.LogTrivial("Buyer2 made it to car, adding to pursuit.");
                            Functions.AddPedToPursuit(Pursuit, Scenario.Buyer2);
                        }
                        else
                        {
                            Game.LogTrivial("Buyer2 did NOT make it into van, making him fight.");
                            MakeFightAndCreateBlip(Scenario.Buyer2, Buyer2Blip);
                        }
                        return;
                    });
                }
                else
                {
                    Game.LogTrivial("Decided not to make Buyer2 try to enter the car and fight straight away instead.");
                    MakeFightAndCreateBlip(Scenario.Buyer2, Buyer2Blip);
                    Buyer2IsFightingStraightAway = true;
                }
            }
        }


        private void CleanUpBlips()
        {

            foreach(var pedblip in PedBlipDict)
            {
                if (pedblip.Value.Exists() && (!pedblip.Key.Exists() || pedblip.Key.IsDead))
                {
                    Game.LogTrivial("Suspect was killed, deleting blip.");
                    pedblip.Value.Delete();
                }
            }
        }


        private void MakeFightAndCreateBlip(Ped ped, Blip blip)
        {
            ped.Tasks.FightAgainstClosestHatedTarget(100f);
            blip = new Blip(ped);
            blip.Color = System.Drawing.Color.Red;
            blip.Scale = 0.75f;
            PedBlipDict.Add(ped, blip);
        }


        private bool IsPlayerTooCloseToSuspects()
        {
            foreach (var waypoint in Scenario.CopCarWayPointList)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(waypoint.endPoint.Position) < 5f)
                {
                    return true;
                }
            }
            if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 30f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsAnySuspectStillAliveAndNotArrested()
        {
            var SuspectsAreStillAliveAndFree = false;
            foreach (var dealer in Scenario.DealerList)
            {
                if (dealer.Exists() && dealer.IsAlive && !Functions.IsPedArrested(dealer))
                {
                    SuspectsAreStillAliveAndFree = true;
                }
            }
            foreach (var buyer in Scenario.BuyerList)
            {
                if (buyer.Exists() && buyer.IsAlive && !Functions.IsPedArrested(buyer))
                {
                    SuspectsAreStillAliveAndFree = true;
                }
            }
            return SuspectsAreStillAliveAndFree;
        }
    }
}
