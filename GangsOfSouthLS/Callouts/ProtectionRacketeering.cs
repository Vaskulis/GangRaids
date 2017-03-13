using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers;
using GangsOfSouthLS.INIFile;
using GangsOfSouthLS.Scenarios.ProtectionRacketeeringScenarios;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GangsOfSouthLS.APIWrappers;

namespace GangsOfSouthLS.Callouts
{
    [CalloutInfo("Protection Racketeering", CalloutProbability.High)]
    internal class ProtectionRacketeering : Callout
    {
        internal static Scenario Scenario;
        internal static ScenarioScheme ScenarioScheme;
        internal static ERacketState RacketState;

        private LHandle Pursuit;
        private Blip ShopBlip;
        private Blip CarBlip;
        private Blip PassengerBlip;
        private Blip DriverBlip;
        private Blip MerchantBlip;
        private int TimeBeforeCarSpawn;
        private bool firstloop = true;
        private bool endingregularly = false;
        private bool pursuitcreated = false;
        private bool givenbat = false;
        private bool endDoorCheck = false;
        private bool endVisibleCheck = false;
        private bool endOnSceneCheck = false;
        private bool endDeletingStockMerchant = false;
        private bool gangstersSeePlayer = false;
        private bool glueCar = false;
        private bool merchantdead = false;
        private Conversation RacketConversation;
        private bool isLSPDFRPlusRunning;
        private bool isComputerPlusRunning;
        private Guid callID;
        private List<MyPed> Suspectlist = new List<MyPed> { };

        public override bool OnBeforeCalloutDisplayed()
        {
            var scenarioFound = ScenarioScheme.ChooseScenario(out ScenarioScheme);
            if (!scenarioFound)
            {
                Game.LogTrivial("[GangsOfSouthLS] Could not find scenario in range.");
                return false;
            }
            Scenario = new Scenario(ScenarioScheme);
            Suspectlist = new List<MyPed> { };
            RacketConversation = new Conversation(ConversationPartCollection.ConverstaionPartsCollections);
            CalloutMessage = "Protection Racketeering ~w~at ~y~" + Scenario.Name;
            CalloutPosition = Scenario.Position;
            if (!(Scenario.ShopNameString == "NONE"))
            {
                Functions.PlayScannerAudio(string.Format("DISP_ATTENTION_UNIT_01 {0} ASSISTANCE_REQUIRED FOR CRIME_GANGACTIVITYINCIDENT AT {1} SUSPECTS_ARE_MEMBERS_OF {2} UNITS_RESPOND_CODE_02_02", INIReader.UnitName, Scenario.ShopNameString, Scenario.GangNameString));
            }
            else
            {
                Functions.PlayScannerAudioUsingPosition(string.Format("DISP_ATTENTION_UNIT_01 {0} ASSISTANCE_REQUIRED FOR CRIME_GANGACTIVITYINCIDENT IN_OR_ON_POSITION SUSPECTS_ARE_MEMBERS_OF {1} UNITS_RESPOND_CODE_02_02", INIReader.UnitName, Scenario.GangNameString), CalloutPosition);
            }
            Functions.PlayScannerAudio("SUSPECTS_ARE_MEMBERS_OF " + Scenario.GangNameString);
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 80f);
            isComputerPlusRunning = DependencyPluginCheck.IsLSPDFRPluginRunning("ComputerPlus", new Version("1.3.0.0"));
            isLSPDFRPlusRunning = DependencyPluginCheck.IsLSPDFRPluginRunning("LSPDFR+", new Version("1.4.1.0"));
            if (isComputerPlusRunning)
            {
                callID = ComputerPlusWrapperClass.CreateCallout("Protection Racketeering", "PROTECTION RACKETEERING", Scenario.Position, ComputerPlus.EResponseType.Code_2, "The owner of the shop reported a regular collection of protection money by one of the local gangs. To collect evidence, the owner agreed to talk to the racketeers while wearing a hidden microphone.");
            }
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            RacketState = ERacketState.Accepted;
            Scenario.Initialize();
            ShopBlip = new Blip(Scenario.Position, 80f);
            ShopBlip.Color = System.Drawing.Color.Yellow;
            ShopBlip.Alpha = 0.5f;
            ShopBlip.IsRouteEnabled = true;
            StartDoorCheck();
            if (isComputerPlusRunning)
            {
                ComputerPlusWrapperClass.SetCalloutStatusToUnitResponding(callID);
                ComputerPlusWrapperClass.AddPedToCallout(callID, Scenario.Merchant);
            }
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (isComputerPlusRunning)
            {
                ComputerPlusWrapperClass.AssignCallToAIUnit(callID);
            }
            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (RacketState == ERacketState.Accepted)
            {
                DoIfStateAccepted();
            }

            if (RacketState == ERacketState.ArrivedOnScene)
            {
                DoIfStateArrivedOnScene();
            }

            if (RacketState == ERacketState.GangstersDrivingToShop)
            {
                DoIfStateGangstersDrivingToShop();
            }

            if (RacketState == ERacketState.RacketeerEnteringShop)
            {
                DoIfStateRacketeerEnteringShop();
            }

            if (RacketState == ERacketState.DrivingPastTheShop)
            {
                DoIfStateDrivingPastTheShop();
            }

            if (RacketState == ERacketState.InConversationStateStart)
            {
                DoIfStateInConversationStateStart();
            }

            if (RacketState == ERacketState.InConversationStateRefusingToGiveMoney)
            {
                DoIfStateInConversationStateRefusingToGiveMoney();
            }

            if (RacketState == ERacketState.InConversationStateIntentionallyTellingAboutPolice)
            {
                DoIfStateInConversationStateIntentionallyTellingAboutPolice();
            }

            if (RacketState == ERacketState.InConversationStateGivingMoneyAfterIntimidation)
            {
                DoIfStateInConversationStateGivingMoneyAfterIntimidation();
            }

            if (RacketState == ERacketState.InConversationStateGivingMoneyStraightAway)
            {
                DoIfStateInConversationStateGivingMoneyStraightAway();
            }

            if (RacketState == ERacketState.InConversationStateUnintentionallyTellingAboutPolice)
            {
                DoIfStateInConversationStateUnintentionallyTellingAboutPolice();
            }

            if (RacketState == ERacketState.RacketeerLeavingShopNormally)
            {
                DoIfStateRacketeerLeavingShopNormally();
            }

            if (RacketState == ERacketState.AttackingAfterConversation)
            {
                DoIfStateAttackingAfterConversation();
            }

            if (RacketState == ERacketState.FleeingAfterConversation)
            {
                DoIfStateFleeingAfterConversation();
            }

            if (RacketState == ERacketState.WaitingForPullover)
            {
                DoIfStateWaitingForPullover();
            }

            if (RacketState == ERacketState.InPullover)
            {
                DoIfStateInPullover();
            }

            if (RacketState == ERacketState.ArrestingBoth)
            {
                DoIfStateArrestingBoth();
            }

            if (RacketState == ERacketState.DriverSafelyArrested)
            {
                DoIfStateDriverSafelyArrested();
            }

            if (RacketState == ERacketState.WaitingToLeaveScene)
            {
                DoIfStateWaitingToLeaveScene();
            }

            if (RacketState == ERacketState.CanBeEnded)
            {
                DoIfStateCanBeEnded();
            }
        }

        public override void End()
        {
            RacketState = ERacketState.Ending;
            CleanUp();
            if (isComputerPlusRunning)
            {
                if (Scenario.Driver.Exists())
                {
                    ComputerPlusWrapperClass.AddPedToCallout(callID, Scenario.Driver);
                }
                if (Scenario.Passenger.Exists())
                {
                    ComputerPlusWrapperClass.AddPedToCallout(callID, Scenario.Passenger);
                }
            }
            if (!endingregularly)
            {
                Game.LogTrivial("[GangsOfSouthLS] NOT ending callout regularly.");
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.CancelCallout(callID);
                }
            }
            else
            {
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.ConcludeCallout(callID);
                }
            }
            EndDoorCheck();
            EndVisibleCheck();
            EndOnSceneCheck();
            EndDeletingStockMerchant();
            ShopBlip.SafelyDelete();
            DriverBlip.SafelyDelete();
            PassengerBlip.SafelyDelete();
            CarBlip.SafelyDelete();
            MerchantBlip.SafelyDelete();
            glueCar = false;
            if (!(Scenario == null))
            {
                Scenario.GangsterCar.SafelyDismiss();
                Scenario.Merchant.SafelyDelete();
            }
            if (!(Scenario == null) && !endingregularly)
            {
                Scenario.Passenger.SafelyDismiss();
                Scenario.Driver.SafelyDismiss();
                Scenario.Merchant.SafelyDismiss();
            }
            base.End();
        }

        internal enum ERacketState
        {
            Accepted,
            ArrivedOnScene,
            GangstersDrivingToShop,
            RacketeerEnteringShop,
            InConversationStateStart,
            InConversationStateGivingMoneyStraightAway,
            InConversationStateRefusingToGiveMoney,
            InConversationStateGivingMoneyAfterIntimidation,
            InConversationStateUnintentionallyTellingAboutPolice,
            InConversationStateIntentionallyTellingAboutPolice,
            RacketeerLeavingShopNormally,
            AttackingAfterConversation,
            FleeingAfterConversation,
            WaitingForPullover,
            InPullover,
            WaitingToLeaveScene,
            CanBeEnded,
            DriverSafelyArrested,
            ArrestingBoth,
            Ending,
            DrivingPastTheShop
        }

        //FUNCTIONS THAT GET CALLED DURING SPECIFIC CALLOUT STATE:
        //BEGIN
        internal void DoIfStateAccepted()
        {
            if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 100f)
            {
                RacketState = ERacketState.ArrivedOnScene;
                firstloop = true;
            }
        }

        internal void DoIfStateArrivedOnScene()
        {
            foreach (var entity in World.GetEntities(Scenario.ParkingPos4.Position, 12f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludeOccupiedVehicles | GetEntitiesFlags.ExcludePlayerVehicle))
            {
                if (!(entity == Scenario.GangsterCar))
                {
                    entity.SafelyDelete();
                }
            }
            if (firstloop)
            {
                StartDeletingStockMerchant();
                StartOnSceneCheck();
                GameFiber.StartNew(delegate
                {
                    if (RacketState == ERacketState.ArrivedOnScene || RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.DisplayHelp("The ~o~owner ~w~of the shop reported a regular collection of protection money by one of the local gangs.");
                        GameFiber.Wait(8000);
                    }
                    if (RacketState == ERacketState.ArrivedOnScene || RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.DisplayHelp("To collect evidence, the ~o~owner ~w~agreed to talk to the ~r~racketeers ~w~while wearing a hidden microphone.");
                        GameFiber.Wait(8000);
                    }
                    if (RacketState == ERacketState.ArrivedOnScene || RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.DisplayHelp("Your job is to record the evidence and arrest the ~r~racketeers ~w~as soon as you think it's safe.");
                        GameFiber.Wait(8000);
                    }
                    if (RacketState == ERacketState.ArrivedOnScene || RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.DisplayHelp("But be ready to engage the ~r~suspects ~w~ at any time. The ~o~shopkeeper ~w~was promised protection by the LSPD.");
                        GameFiber.Wait(8000);
                    }
                    if (RacketState == ERacketState.ArrivedOnScene || RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.DisplayHelp("Stay hidden, the ~r~racketeers ~w~should arrive any minute now!", 10000);
                    }
                });
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.SetCalloutStatusToAtScene(callID);
                    ComputerPlusWrapperClass.AddUpdateToCallout(callID, "Officer arrived at the scene.");
                }
                Game.LogTrivial("[GangsOfSouthLS] Player arrived on scene.");
                firstloop = false;
                TimeBeforeCarSpawn = 5000 + ((UsefulFunctions.rng.Next(10)) * 1000);
                Game.LogTrivial(string.Format("[GangsOfSouthLS] Waiting {0} s to spawn car.", (TimeBeforeCarSpawn / 1000)));
                MakeMerchantBlip();
                ShopBlip.SafelyDelete();

                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(TimeBeforeCarSpawn);
                    Scenario.SpawnCarAndBadGuys();
                    Scenario.Passenger.AddCrimeToList("Extortion", 0);
                    Scenario.Driver.AddCrimeToList("Extortion", 0);
                    StartVisibleCheck();
                    Suspectlist = new List<MyPed> { Scenario.Passenger, Scenario.Driver };
                    RacketState = ERacketState.GangstersDrivingToShop;
                    firstloop = true;
                    return;
                });
            }
        }

        internal void DoIfStateGangstersDrivingToShop()
        {
            CleanUp();
            foreach (var entity in World.GetEntities(Scenario.ParkingPos4.Position, 12f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludeOccupiedVehicles | GetEntitiesFlags.ExcludePlayerVehicle))
            {
                if (!(entity == Scenario.GangsterCar))
                {
                    entity.SafelyDelete();
                }
            }
            if (firstloop)
            {
                Game.LogTrivial("[GangsOfSouthLS] Car spawned, player is on the scene and waiting.");
                firstloop = false;
                GameFiber.StartNew(delegate
                {
                    if (RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        var count = 0;
                        var carismoving = false;
                        while ((RacketState == ERacketState.GangstersDrivingToShop) && !carismoving && (count < 10))
                        {
                            Scenario.Driver.Tasks.DriveToPosition(Scenario.CarWaypointPos4.Position, 10f, VehicleDrivingFlags.Normal | VehicleDrivingFlags.AllowMedianCrossing, 20f);
                            GameFiber.Wait(2500);
                            if (Scenario.GangsterCar.DistanceTo(Scenario.CarSpawnPos4.Position) > 1f)
                            {
                                Game.LogTrivial(string.Format("[GangsOfSouthLS] Car started moving after {0} try/tries.", count + 1));
                                carismoving = true;
                            }
                            else
                            {
                                Scenario.Driver.Tasks.Clear();
                                count += 1;
                            }
                        }
                        if (!carismoving)
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Car didn't start moving for some reason, aborting callout.");
                            Game.DisplayNotification("~r~GangsOfSouthLS: The car didn't start moving for some reason, aborting the callout. Please report this error!");
                            End();
                            return;
                        }
                    }
                    if (RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Driving to waypoint, 8, 3");
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.CarWaypointPos4.Position, 8f, VehicleDrivingFlags.Normal | VehicleDrivingFlags.AllowMedianCrossing, 3f).WaitForCompletion(40000);
                    }
                    if (RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Driving to parking, 5, 10");
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.ParkingPos4.Position, 5f, VehicleDrivingFlags.Normal | VehicleDrivingFlags.AllowMedianCrossing, 10f).WaitForCompletion(30000);
                    }
                    if (RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Parking.");
                        Scenario.Driver.Tasks.ParkVehicle(Scenario.ParkingPos4.Position, Scenario.ParkingPos4.Heading).WaitForCompletion(30000);
                        var absheadingdiff = 0f;
                        GameFiber.StartNew(delegate
                        {
                            while (RacketState == ERacketState.GangstersDrivingToShop)
                            {
                                GameFiber.Yield();
                                if ((Scenario.GangsterCar.DistanceTo(Scenario.ParkingPos4.Position) < 2f))
                                {
                                    absheadingdiff = Math.Abs(Scenario.ParkingPos4.Heading - Scenario.GangsterCar.Heading);
                                    if ((absheadingdiff < 300) || (Math.Abs(360 - absheadingdiff) < 30))
                                    {
                                        break;
                                    }
                                }
                            }
                            Scenario.Driver.Tasks.Clear();
                        });
                    }
                    if (RacketState == ERacketState.GangstersDrivingToShop)
                    {
                        if (Scenario.GangsterCar.Position.DistanceTo(Scenario.ParkingPos4.Position) < 5f)
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Car parked and Racketeer is entering shop.");
                            RacketState = ERacketState.RacketeerEnteringShop;
                        }
                        else
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Car didn't make it for some reason, aborting callout.");
                            Game.DisplayNotification("~r~GangsOfSouthLS: The car didn't make it to the shop for some reason, aborting the callout. If you are not sure what caused this, please report this error!");
                            End();
                        }
                    }
                    firstloop = true;
                    EndOnSceneCheck();
                    return;
                });
            }
            CheckAndReactIfVisibleBeforeArriving();
        }

        internal void DoIfStateRacketeerEnteringShop()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                EndOnSceneCheck();
                RacketConversation.PlayConversationPartOfState(ConversationState.InformingOfArrival);
                GameFiber.StartNew(delegate
                {
                    if (!(RacketState == ERacketState.Ending))
                    {
                        Scenario.Passenger.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(10000);
                    }
                    if (UsefulFunctions.Decide(60))
                    {
                        Scenario.Passenger.Inventory.Weapons.Clear();
                        Scenario.Passenger.Inventory.GiveNewWeapon("weapon_bat", 1, true);
                        givenbat = true;
                        Game.LogTrivial("[GangsOfSouthLS] Racketeer was given a bat and had his gun removed.");
                    }
                    if (!(RacketState == ERacketState.Ending))
                    {
                        Scenario.Passenger.Tasks.FollowNavigationMeshToPosition(Scenario.RacketeerShopPos4.Position, Scenario.RacketeerShopPos4.Heading, 1.3f).WaitForCompletion(40000);
                    }
                    if (RacketState == ERacketState.RacketeerEnteringShop)
                    {
                        if (Scenario.Passenger.Position.DistanceTo(Scenario.RacketeerShopPos4.Position) < 3)
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Racketeer is in Shop, starting Conversation.");
                            RacketState = ERacketState.InConversationStateStart;
                            firstloop = true;
                        }
                        else
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Racketeer didn't make it to his position shop for some reason, aborting callout");
                            Game.DisplayNotification("~r~GangsOfSouthLS: The gangster didn't make it to his position in the shop for some reason, aborting the callout. Please report this error!");
                            End();
                        }
                    }
                    return;
                });
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.AddUpdateToCallout(callID, "Suspects arrived at the scene. Suspect vehicle has been identified.");
                    ComputerPlusWrapperClass.AddVehicleToCallout(callID, Scenario.GangsterCar);
                }
            }
            CheckAndReactIfVisibleInShop();
        }

        internal void DoIfStateInConversationStateStart()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                RacketConversation.PlayConversationPartOfState(ConversationState.Start);
                Scenario.Passenger.AddCrimeToList("Extortion", 84);
                Scenario.Driver.AddCrimeToList("Extortion", 84);
            }
            if (RacketConversation.IsFinished)
            {
                if (UsefulFunctions.Decide(40))
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.Start finished normally, going on to ConversationState.GivingMoneyStraightAway.");
                    RacketState = ERacketState.InConversationStateGivingMoneyStraightAway;
                    firstloop = true;
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.Start finished normally, going on to ConversationState.RefusingToGiveMoney.");
                    RacketState = ERacketState.InConversationStateRefusingToGiveMoney;
                    firstloop = true;
                }
            }
            CheckAndReactIfVisibleInShop();
        }

        internal void DoIfStateInConversationStateGivingMoneyStraightAway()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                RacketConversation.PlayConversationPartOfState(ConversationState.GivingMoneyStraightAway);
            }
            if (RacketConversation.IsFinished)
            {
                if (UsefulFunctions.Decide(65))
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.GivingMoneyStraightAway finished normally, making the Racketeer reenter the car.");
                    RacketState = ERacketState.RacketeerLeavingShopNormally;
                    firstloop = true;
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.GivingMoneyStraightAway finished normally, going on to ConversationState.UnintentionallyTellingAboutPolice.");
                    RacketState = ERacketState.InConversationStateUnintentionallyTellingAboutPolice;
                    firstloop = true;
                }
            }
            CheckAndReactIfVisibleInShop();
        }

        internal void DoIfStateInConversationStateRefusingToGiveMoney()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                RacketConversation.PlayConversationPartOfState(ConversationState.RefusingToGiveMoney);
                Scenario.Passenger.AddCrimeToList("Extortion", 36);
            }
            if (RacketConversation.IsFinished)
            {
                if (UsefulFunctions.Decide(50))
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.RefusingToGiveMoney finished normally, going on to ConversationState.IntentionallyTellingAboutPolice.");
                    RacketState = ERacketState.InConversationStateIntentionallyTellingAboutPolice;
                    firstloop = true;
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.RefusingToGiveMoney finished normally, going on to ConversationState.InConversationStateGivingMoneyAfterIntimidation.");
                    RacketState = ERacketState.InConversationStateGivingMoneyAfterIntimidation;
                    firstloop = true;
                }
            }
            CheckAndReactIfVisibleInShop();
        }

        internal void DoIfStateInConversationStateGivingMoneyAfterIntimidation()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                RacketConversation.PlayConversationPartOfState(ConversationState.GivingMoneyAfterIntimidation);
            }
            if (RacketConversation.IsFinished)
            {
                if (UsefulFunctions.Decide(65))
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.GivingMoneyAfterIntimidation finished normally, making the Racketeer reenter the car.");
                    RacketState = ERacketState.RacketeerLeavingShopNormally;
                    firstloop = true;
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.GivingMoneyAfterIntimidation finished normally, going on to ConversationState.UnintentionallyTellingAboutPolice.");
                    RacketState = ERacketState.InConversationStateUnintentionallyTellingAboutPolice;
                    firstloop = true;
                }
            }
            CheckAndReactIfVisibleInShop();
        }

        internal void DoIfStateInConversationStateUnintentionallyTellingAboutPolice()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                RacketConversation.PlayConversationPartOfState(ConversationState.UnintentionallyTellingAboutPolice);
            }
            if (RacketConversation.IsFinished)
            {
                if (UsefulFunctions.Decide(50))
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.UnintentionallyTellingAboutPolice finished normally, making the gangsters fight.");
                    RacketState = ERacketState.AttackingAfterConversation;
                    firstloop = true;
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.UnintentionallyTellingAboutPolice finished normally, making the gangsters flee.");
                    RacketState = ERacketState.FleeingAfterConversation;
                    firstloop = true;
                }
            }
            CheckAndReactIfVisibleInShop();
        }

        internal void DoIfStateInConversationStateIntentionallyTellingAboutPolice()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                RacketConversation.PlayConversationPartOfState(ConversationState.IntentionallyTellingAboutPolice);
            }
            if (RacketConversation.IsFinished)
            {
                if (UsefulFunctions.Decide(60))
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.IntentionallyTellingAboutPolice finished normally, making the gangsters fight.");
                    RacketState = ERacketState.AttackingAfterConversation;
                    firstloop = true;
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] ConversationState.IntentionallyTellingAboutPolice finished normally, making the gangsters flee.");
                    RacketState = ERacketState.FleeingAfterConversation;
                    firstloop = true;
                }
            }
            CheckAndReactIfVisibleInShop();
        }

        internal void DoIfStateRacketeerLeavingShopNormally()
        {
            CleanUp();
            EndVisibleCheck();
            if (firstloop)
            {
                firstloop = false;
                GameFiber.StartNew(delegate
                {
                    Scenario.Passenger.Tasks.EnterVehicle(Scenario.GangsterCar, 0, 1.3f).WaitForCompletion(40000);
                    if (RacketState == ERacketState.RacketeerLeavingShopNormally)
                    {
                        if (Scenario.Passenger.IsInVehicle(Scenario.GangsterCar, false))
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Racketeer is back in car, waiting for pull over.");
                            if (givenbat)
                            {
                                Game.LogTrivial("[GangsOfSouthLS] Giving the racketeer back a gun.");
                                Scenario.Passenger.Inventory.GiveNewWeapon(Scenario.GunList.RandomElement(), 999, false);
                            }
                            Scenario.Driver.Tasks.CruiseWithVehicle(12f, VehicleDrivingFlags.Normal);
                            MakeCarBlip();
                            RacketConversation.PlayConversationPartOfState(ConversationState.InformingOfDeparture);
                            RacketState = ERacketState.WaitingForPullover;
                        }
                        else
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Racketeer didn't make it back to the car for some reason, aborting callout");
                            Game.DisplayNotification("~r~GangsOfSouthLS: The gangster didn't make it back to the car for some reason, aborting the callout. Please report this error!");
                            End();
                        }
                    }
                    firstloop = true;
                });
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.AddUpdateToCallout(callID, "Collected enough evidence to make an arrest.");
                }
            }
        }

        internal void DoIfStateAttackingAfterConversation()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                EndVisibleCheck();
                MakeDriverBlip();
                MakePassengerBlip();
                Game.SetRelationshipBetweenRelationshipGroups("RACKET_GANGSTER", "RACKET_MERCHANT", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("RACKET_MERCHANT", "RACKET_GANGSTER", Relationship.Hate);
                GameFiber.StartNew(delegate
                {
                    Scenario.Passenger.Tasks.FightAgainst(Scenario.Merchant);
                    GameFiber.StartNew(delegate
                    {
                        GameFiber.Wait(10000);
                        if (Scenario.Merchant.IsDead)
                        {
                            Scenario.Passenger.AddCrimeToList("Second Degree Murder", 144);
                        }
                        else
                        {
                            Scenario.Passenger.AddCrimeToList("Aggravated Assault", 36);
                        }
                    });
                    while (!RacketConversation.IsFinished)
                    {
                        GameFiber.Yield();
                    }
                    RacketConversation.PlayConversationPartOfState(ConversationState.Attacking);
                    MyNatives.SetPedCombatAttributes(Scenario.Driver, MyNatives.CombatAttributesFlag.CanLeaveVehicle, false);
                    MyNatives.SetPedCombatAttributes(Scenario.Driver, MyNatives.CombatAttributesFlag.CanDoDrivebys, true);
                    Scenario.Driver.Tasks.FightAgainstClosestHatedTarget(100f);
                    glueCar = true;
                    GameFiber.StartNew(delegate
                    {
                        Vector3 pos;
                        while (glueCar)
                        {
                            pos = Scenario.GangsterCar.Position;
                            GameFiber.Yield();
                            Scenario.GangsterCar.Position = pos;
                        }
                    });
                    Scenario.Driver.Tasks.FightAgainstClosestHatedTarget(100f);
                    Scenario.Driver.AddCrimeToList("Aggravated Assault", 48);
                    while (!RacketConversation.IsFinished)
                    {
                        GameFiber.Yield();
                    }
                    if (givenbat)
                    {
                        Game.DisplaySubtitle("<i>~o~*thudding noises and screaming*</i>");
                    }
                    else
                    {
                        Game.DisplaySubtitle("<i>~o~*gunshots and screaming*</i>");
                    }
                    GameFiber.Wait(5000);
                    if (givenbat)
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Giving the racketeer back a gun.");
                        Scenario.Passenger.Inventory.GiveNewWeapon(Scenario.GunList.RandomElement(), 999, false);
                    }
                    if (UsefulFunctions.Decide(50))
                    {
                        glueCar = false;
                        Game.LogTrivial("[GangsOfSouthLS] Starting a fight.");
                        if (!(RacketState == ERacketState.Ending))
                        {
                            MakeStuffHappenWhenStartingAFight();
                            MyNatives.SetPedCombatAttributes(Scenario.Driver, MyNatives.CombatAttributesFlag.CanLeaveVehicle, true);
                        }
                    }
                    else if (UsefulFunctions.Decide(50))
                    {
                        glueCar = false;
                        Game.LogTrivial("[GangsOfSouthLS] Starting the pursuit without letting the Passenger try to enter the car.");
                        if (!(RacketState == ERacketState.Ending))
                        {
                            MakeStuffHappenWhenCreatingSelfmadePursuit();
                            MyNatives.SetPedCombatAttributes(Scenario.Driver, MyNatives.CombatAttributesFlag.CanLeaveVehicle, true);
                        }
                    }
                    else
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Making the racketeer try to enter the car before starting the pursuit");
                        Scenario.Passenger.Tasks.EnterVehicle(Scenario.GangsterCar, 0, 3f);
                        for (int i = 0; i <= 25; i++)
                        {
                            Game.LogTrivial(string.Format("[GangsOfSouthLS] Waited {0} ms for Passenger to enter the car", i * 500));
                            GameFiber.Wait(500);
                            if (Scenario.Passenger.Exists() && (Scenario.Passenger.IsInAnyVehicle(false) || Scenario.Passenger.IsDead))
                            {
                                break;
                            }
                        }
                        if (!(RacketState == ERacketState.Ending))
                        {
                            Scenario.Driver.Tasks.Clear();
                            glueCar = false;
                            MyNatives.SetPedCombatAttributes(Scenario.Driver, MyNatives.CombatAttributesFlag.CanLeaveVehicle, true);
                            MakeStuffHappenWhenCreatingSelfmadePursuit();
                        }
                    }
                });
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.AddUpdateToCallout(callID, "The racketeers are attacking the shopkeeper after finding out about the officer listening.");
                }
            }
        }

        internal void DoIfStateFleeingAfterConversation()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                EndVisibleCheck();
                RacketConversation.PlayConversationPartOfState(ConversationState.Fleeing);
                MakePassengerBlip();
                MakeDriverBlip();
                if (givenbat)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Giving the racketeer back a gun.");
                    Scenario.Passenger.Inventory.GiveNewWeapon(Scenario.GunList.RandomElement(), 999, false);
                }
                GameFiber.StartNew(delegate
                {
                    Game.LogTrivial("[GangsOfSouthLS] Making the racketeer try to enter the car before starting the pursuit");
                    MyNatives.SetPedCombatAttributes(Scenario.Driver, MyNatives.CombatAttributesFlag.CanLeaveVehicle, false);
                    if (!(RacketState == ERacketState.Ending))
                    {
                        Scenario.Driver.Tasks.FightAgainstClosestHatedTarget(100f);
                        Scenario.Driver.AddCrimeToList("Aggravated Assault", 48);
                        glueCar = true;
                        GameFiber.StartNew(delegate
                        {
                            Vector3 pos;
                            while (glueCar)
                            {
                                pos = Scenario.GangsterCar.Position;
                                GameFiber.Yield();
                                Scenario.GangsterCar.Position = pos;
                            }
                        });
                    }
                    Scenario.Passenger.Tasks.EnterVehicle(Scenario.GangsterCar, 0, 3f);
                    for (int i = 0; i <= 25; i++)
                    {
                        Game.LogTrivial(string.Format("[GangsOfSouthLS] Waited {0} ms for Passenger to enter the car", i * 500));
                        GameFiber.Wait(500);
                        if (Scenario.Passenger.Exists() && (Scenario.Passenger.IsInAnyVehicle(false) || Scenario.Passenger.IsDead))
                        {
                            break;
                        }
                    }
                    if (!(RacketState == ERacketState.Ending))
                    {
                        glueCar = false;
                        Scenario.Driver.Tasks.Clear();
                        MyNatives.SetPedCombatAttributes(Scenario.Driver, MyNatives.CombatAttributesFlag.CanLeaveVehicle, true);
                        MakeStuffHappenWhenCreatingSelfmadePursuit();
                    }
                });
                if (isComputerPlusRunning)
                {
                    ComputerPlusWrapperClass.AddUpdateToCallout(callID, "The racketeers are fleeing the scene after finding out about the officer listening.");
                }
            }
        }

        internal void DoIfStateWaitingForPullover()
        {
            CleanUp();
            Game.DisplayHelp("Pull over the ~y~suspect's car~w~.");
            if (Functions.IsPlayerPerformingPullover() && (Functions.GetPulloverSuspect(Functions.GetCurrentPullover())) == Scenario.Driver)
            {
                Game.LogTrivial("[GangsOfSouthLS] Player started Pullover.");
                CarBlip.SafelyDelete();
                firstloop = true;
                RacketState = ERacketState.InPullover;
            }
            if (Functions.IsPedGettingArrested(Scenario.Driver))
            {
                MakeStuffHappenWhenArrestingDuringPullover();
            }
        }

        internal void DoIfStateInPullover()
        {
            CleanUp();
            CarBlip.SafelyDelete();
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
            {
                Game.DisplaySubtitle("Arrest the ~r~suspects~w~!");
            }
            else
            {
                Game.DisplayHelp("Arrest the ~r~suspects~w~!");
            }
            if (!Functions.IsPlayerPerformingPullover() || !(Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == Scenario.Driver))
            {
                Game.LogTrivial("[GangsOfSouthLS] Player aborted Pullover, waiting again.");
                RacketState = ERacketState.WaitingForPullover;
            }
            if (!(Functions.GetActivePursuit() == null))
            {
                MakeStuffHappenWhenAutomaticPursuitIsCreated();
            }
            if ((Functions.GetActivePursuit() == null) && !Game.LocalPlayer.Character.IsInAnyVehicle(false) && (Game.LocalPlayer.Character.DistanceTo(Scenario.Driver) < 6f))
            {
                if (UsefulFunctions.Decide(35) && firstloop) //Make them fight
                {
                    firstloop = false;
                    MakeStuffHappenWhenStartingAFight();
                }
                else if (UsefulFunctions.Decide(30) && firstloop)  //Make them drive off
                {
                    firstloop = false;
                    MakeStuffHappenWhenCreatingSelfmadePursuit();
                }
                else
                {
                    firstloop = false;
                    Game.LogTrivial("[GangsOfSouthLS] Allowing the Player to arrest both.");
                    RacketState = ERacketState.ArrestingBoth;
                }
            }
            if (Functions.IsPedGettingArrested(Scenario.Driver) || Functions.IsPedGettingArrested(Scenario.Passenger))
            {
                MakeStuffHappenWhenArrestingDuringPullover();
            }
        }

        internal void DoIfStateWaitingToLeaveScene()
        {
            CleanUp();
            if (HasPlayerLeftScene())
            {
                Game.LogTrivial("[GangsOfSouthLS] Player left scene, allowing callout to be ended.");
                RacketState = ERacketState.CanBeEnded;
            }
            else
            {
                if (Scenario.Merchant.Exists() && Scenario.Merchant.IsAlive)
                {
                    if (Game.LocalPlayer.Character.DistanceTo(Scenario.Merchant) < 4f)
                    {
                        Game.DisplayHelp("Press ~b~Y ~w~to talk to the ~o~shopkeeper~w~.");
                        if (RacketConversation.IsFinished)
                        {
                            Game.DisplayHelp("Press ~b~Y ~w~to talk to the ~o~shopkeeper~w~.");
                            if (Game.IsKeyDown(Keys.Y))
                            {
                                if (pursuitcreated && !(Pursuit == null) && Functions.IsPursuitStillRunning(Pursuit))
                                {
                                    RacketConversation.PlayConversationPartOfState(ConversationState.PursuitStillRunning);
                                }
                                else
                                {
                                    RacketConversation.PlayConversationPartOfState(ConversationState.Survived);
                                }
                            }
                        }
                    }
                    else if (!pursuitcreated || !Functions.IsPursuitStillRunning(Pursuit))
                    {
                        Game.DisplayHelp("Go talk to the ~o~shopkeeper ~w~or leave the scene.");
                    }
                }
            }
        }

        internal void DoIfStateDriverSafelyArrested()
        {
            CleanUp();
            Game.DisplayHelp("Arrest the ~r~suspects!");
            if (UsefulFunctions.Decide(35) && firstloop)
            {
                firstloop = false;
                MakeStuffHappenWhenCreatingSelfmadePursuit();
            }
            else
            {
                Game.LogTrivial("[GangsOfSouthLS] Passenger is not fleeing.");
                RacketState = ERacketState.WaitingToLeaveScene;
            }
        }

        internal void DoIfStateArrestingBoth()
        {
            CleanUp();
            if (!(Functions.GetActivePursuit() == null))
            {
                MakeStuffHappenWhenAutomaticPursuitIsCreated();
            }
        }

        internal void DoIfStateDrivingPastTheShop()
        {
            CleanUp();
            if (firstloop)
            {
                firstloop = false;
                if (RacketState == ERacketState.DrivingPastTheShop)
                {
                    GameFiber.StartNew(delegate
                    {
                        Scenario.Driver.Tasks.Clear();
                        Game.LogTrivial("[GangsOfSouthLS] Making the driver drive to the shop.");
                        Scenario.Driver.Tasks.DriveToPosition(Scenario.ParkingPos4.Position, 12f, VehicleDrivingFlags.Normal, 25f).WaitForCompletion(60000);
                        if (!(RacketState == ERacketState.Ending))
                        {
                            Game.LogTrivial("[GangsOfSouthLS] Making the driver cruise around and playing ConversationState.InformingOfDrivingPast.");
                            RacketConversation.PlayConversationPartOfState(ConversationState.InformingOfDrivingPast);
                            Scenario.Driver.Tasks.CruiseWithVehicle(12f);
                        }
                        while (!(RacketState == ERacketState.Ending) && !RacketConversation.IsFinished)
                        {
                            GameFiber.Yield();
                        }
                        Game.DisplayHelp("The gangsters saw you and decided not to pay a visit to the shopkeeper.", 5000);
                        GameFiber.Wait(5000);
                        Game.DisplayHelp("You could still pull them over and arrest them, but without any proof of their racket, they'll be free again soon.", 5000);
                        GameFiber.Wait(5000);
                        Game.DisplayHelp("They are going to remember what the shopkeeper tried to do...", 5000);
                    });
                }
            }
            if (!(RacketState == ERacketState.Ending) && AreTheGangstersGone())
            {
                Game.LogTrivial("[GangsOfSouthLS] Player left the vicinity of the gangsters, ending callout.");
                Scenario.Driver.SafelyDismiss();
                Scenario.Passenger.SafelyDismiss();
                endingregularly = true;
                End();
            }
        }

        internal void DoIfStateCanBeEnded()
        {
            CleanUp();
            TestEndCondition();
            if (!HasPlayerLeftScene())
            {
                Game.LogTrivial("[GangsOfSouthLS] Player hasn't left scene, NOT allowing callout to be ended.");
                RacketState = ERacketState.WaitingToLeaveScene;
            }
        }

        //FUNCTIONS THAT GET CALLED DURING SPECIFIC CALLOUT STATE:
        //END

        //HELPER FUNCTIONS
        //BEGIN
        internal void MakeCarBlip()
        {
            if (CarBlip == null || !CarBlip.Exists())
            {
                CarBlip = new Blip(Scenario.GangsterCar);
                CarBlip.Color = System.Drawing.Color.Yellow;
                CarBlip.Order = 999;
            }
        }

        internal void MakePassengerBlip()
        {
            if (Scenario.Passenger.Exists() && (PassengerBlip == null || !PassengerBlip.Exists()))
            {
                PassengerBlip = new Blip(Scenario.Passenger);
                PassengerBlip.Color = System.Drawing.Color.FromArgb(224, 50, 50);
                PassengerBlip.Scale = 0.75f;
                PassengerBlip.Order = 1;
            }
        }

        internal void MakeDriverBlip()
        {
            if (Scenario.Driver.Exists() && (DriverBlip == null || !DriverBlip.Exists()))
            {
                DriverBlip = new Blip(Scenario.Driver);
                DriverBlip.Color = System.Drawing.Color.FromArgb(224, 50, 50);
                DriverBlip.Scale = 0.75f;
                DriverBlip.Order = 1;
            }
        }

        internal void MakeMerchantBlip()
        {
            if (Scenario.Merchant.Exists() && (MerchantBlip == null || !MerchantBlip.Exists()))
            {
                MerchantBlip = new Blip(Scenario.Merchant);
                MerchantBlip.Color = System.Drawing.Color.Orange;
                MerchantBlip.Scale = 0.75f;
                MerchantBlip.Order = 1;
            }
        }

        internal void CleanUp()
        {
            var newList = new List<MyPed> { };
            foreach (var suspect in Suspectlist)
            {
                if (!suspect.Exists())
                {
                    continue;
                }
                else if (suspect.IsDead)
                {
                    if (isComputerPlusRunning)
                    {
                        ComputerPlusWrapperClass.AddPedToCallout(callID, suspect);
                        ComputerPlusWrapperClass.AddUpdateToCallout(callID, string.Format("Suspect {0} was killed.", Functions.GetPersonaForPed(suspect).FullName));
                    }
                    suspect.SafelyDismiss();
                }
                else if (Functions.IsPedArrested(suspect))
                {
                    if (isComputerPlusRunning)
                    {
                        ComputerPlusWrapperClass.AddPedToCallout(callID, suspect);
                        ComputerPlusWrapperClass.AddUpdateToCallout(callID, string.Format("Suspect {0} was arrested.", Functions.GetPersonaForPed(suspect).FullName));
                    }
                    suspect.CreateCourtCase(isLSPDFRPlusRunning);
                }
                else
                {
                    newList.Add(suspect);
                }
            }
            Suspectlist = newList;

            if (isComputerPlusRunning && !merchantdead && Scenario.Merchant.Exists() && Scenario.Merchant.IsDead)
            {
                ComputerPlusWrapperClass.AddUpdateToCallout(callID, string.Format("The shopkeeper, {0}, was killed.", Functions.GetPersonaForPed(Scenario.Merchant).FullName));
                merchantdead = true;
            }

            if (Scenario.Driver.Exists() && Scenario.Driver.IsDead)
            {
                DriverBlip.SafelyDelete();
            }
            if (Scenario.Passenger.Exists() && Scenario.Passenger.IsDead)
            {
                PassengerBlip.SafelyDelete();
            }
            if (Scenario.Merchant.Exists() && Scenario.Merchant.IsDead)
            {
                MerchantBlip.SafelyDelete();
            }
            EndIfPlayerDies();
        }

        internal void TestEndCondition()
        {
            if (!(RacketState == ERacketState.Ending))
            {
                if (pursuitcreated)
                {
                    if (!Functions.IsPursuitStillRunning(Pursuit) && ((!Scenario.Passenger.Exists() || Scenario.Passenger.IsDead || Functions.IsPedArrested(Scenario.Passenger)) && ((!Scenario.Driver.Exists() || Scenario.Driver.IsDead || Functions.IsPedArrested(Scenario.Driver)))))
                    {
                        CleanUp();
                        Game.LogTrivial("[GangsOfSouthLS] Pursuit ended, Ending callout regularly.");
                        endingregularly = true;
                        End();
                    }
                }
                else
                {
                    if ((!Scenario.Passenger.Exists() || Scenario.Passenger.IsDead || Functions.IsPedArrested(Scenario.Passenger)) && ((!Scenario.Driver.Exists() || Scenario.Driver.IsDead || Functions.IsPedArrested(Scenario.Driver))))
                    {
                        CleanUp();
                        Game.LogTrivial("[GangsOfSouthLS] Everyone dead or arrested, Ending callout regularly.");
                        endingregularly = true;
                        End();
                    }
                }
            }
        }

        internal void EndIfPlayerDies()
        {
            if (!(RacketState == ERacketState.Ending) && Game.LocalPlayer.Character.IsDead)
            {
                Game.LogTrivial("[GangsOfSouthLS] Player died, NOT ending regularly.");
                End();
            }
        }

        internal void MakeStuffHappenWhenAutomaticPursuitIsCreated()
        {
            Functions.ForceEndCurrentPullover();
            Pursuit = Functions.GetActivePursuit();
            pursuitcreated = true;
            Game.LogTrivial("[GangsOfSouthLS] LSPDFR started pursuit automatically, giving up control of Driver and making Passenger fight.");
            MakePassengerBlip();
            Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(100f);
            Scenario.Driver.AddCrimeToList("Resisting Arrest", 12);
            Scenario.Passenger.AddCrimeToList("Resisting Arrest", 12);
            Scenario.Passenger.AddCrimeToList("Aggravated Assault", 48);
            RacketState = ERacketState.WaitingToLeaveScene;
        }

        internal void MakeStuffHappenWhenCreatingSelfmadePursuit()
        {
            CleanUp();
            Functions.ForceEndCurrentPullover();
            Game.SetRelationshipBetweenRelationshipGroups("COP", "RACKET_GANGSTER", Relationship.Hate);
            Pursuit = Functions.GetActivePursuit();
            if (Pursuit == null)
            {
                Pursuit = Functions.CreatePursuit();
            }
            Game.LogTrivial("[GangsOfSouthLS] Starting Pursuit.");
            if (Scenario.Driver.Exists() && Scenario.Passenger.Exists())
            {
                if (Scenario.Passenger.IsInVehicle(Scenario.GangsterCar, false) && !Scenario.Driver.SafelyIsDeadOrArrested() && Scenario.Driver.IsInVehicle(Scenario.GangsterCar, false) && (Scenario.Driver.SeatIndex == -1))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Both are in the car and driver is not dead.");
                    Game.LogTrivial("[GangsOfSouthLS] Driver not dead or arrested, adding to pursuit.");
                    DropWeaponAndAddtoPursuit(Pursuit, Scenario.Driver);
                    if (!Scenario.Passenger.SafelyIsDeadOrArrested())
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Making Passenger fight.");
                        MakePassengerBlip();
                        Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(80f);
                        Scenario.Passenger.AddCrimeToList("Aggravated Assault", 48);
                        Scenario.Driver.AddCrimeToList("Resisting Arrest", 12);
                        Scenario.Passenger.AddCrimeToList("Resisting Arrest", 12);
                    }
                }
                else if (Scenario.Passenger.IsInVehicle(Scenario.GangsterCar, false))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Driver is not in car or dead, passenger is in car.");
                    if (!Scenario.Passenger.SafelyIsDeadOrArrested())
                    {
                        if (Scenario.Passenger.SeatIndex != -1)
                        {
                            Scenario.Passenger.Tasks.ShuffleToAdjacentSeat().WaitForCompletion();
                            GameFiber.Wait(200);
                        }
                        Game.LogTrivial("[GangsOfSouthLS] Passenger not dead or arrested, adding to pursuit.");
                        DropWeaponAndAddtoPursuit(Pursuit, Scenario.Passenger);
                        Scenario.Passenger.AddCrimeToList("Resisting Arrest", 12);
                    }
                    if (!Scenario.Driver.SafelyIsDeadOrArrested())
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Driver not dead or arrested, adding to pursuit.");
                        DropWeaponAndAddtoPursuit(Pursuit, Scenario.Driver);
                        Scenario.Driver.AddCrimeToList("Resisting Arrest", 12);
                    }
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Starting pursuit without any special conditions.");
                    if (!Scenario.Driver.SafelyIsDeadOrArrested())
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Driver not dead or arrested, adding to pursuit.");
                        DropWeaponAndAddtoPursuit(Pursuit, Scenario.Driver);
                        Scenario.Driver.AddCrimeToList("Resisting Arrest", 12);
                    }
                    if (!Scenario.Passenger.SafelyIsDeadOrArrested())
                    {
                        Game.LogTrivial("[GangsOfSouthLS] Passenger not dead or arrested, adding to pursuit.");
                        DropWeaponAndAddtoPursuit(Pursuit, Scenario.Passenger);
                        Scenario.Passenger.AddCrimeToList("Resisting Arrest", 12);
                    }
                }
            }
            Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
            firstloop = true;
            pursuitcreated = true;
            RacketState = ERacketState.WaitingToLeaveScene;
        }

        internal void MakeStuffHappenWhenStartingAFight()
        {
            Functions.ForceEndCurrentPullover();
            Game.LogTrivial("[GangsOfSouthLS] Making gangsters fight player.");
            MakePassengerBlip();
            MakeDriverBlip();
            Game.SetRelationshipBetweenRelationshipGroups("COP", "RACKET_GANGSTER", Relationship.Hate);
            Scenario.Passenger.AddCrimeToList("Resisting Arrest", 12);
            Scenario.Driver.AddCrimeToList("Resisting Arrest", 12);
            if (!Scenario.Driver.SafelyIsDeadOrArrested())
            {
                GameFiber.StartNew(delegate
                {
                    Scenario.Driver.Tasks.Clear();
                    if (Scenario.Driver.IsInAnyVehicle(false))
                    {
                        Scenario.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
                    }
                    Scenario.Driver.Tasks.FightAgainstClosestHatedTarget(100f);
                    Scenario.Driver.AddCrimeToList("Aggravated Assault", 48);
                    while (!(RacketState == ERacketState.Ending))
                    {
                        if (Scenario.Driver.SafelyIsDeadOrArrested(true))
                        {
                            while (!(RacketState == ERacketState.Ending))
                            {
                                if (!Scenario.Driver.SafelyIsDeadOrArrested(true))
                                {
                                    if (Pursuit == null)
                                    {
                                        Pursuit = Functions.GetActivePursuit();
                                        if (Pursuit == null)
                                        {
                                            Pursuit = Functions.CreatePursuit();
                                            Game.LogTrivial("[GangsOfSouthLS] Starting Pursuit.");
                                        }
                                    }
                                    pursuitcreated = true;
                                    DropWeaponAndAddtoPursuit(Pursuit, Scenario.Driver);
                                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                                    Game.LogTrivial("[GangsOfSouthLS] Adding Driver to pursuit.");
                                    break;
                                }
                                GameFiber.Yield();
                            }
                            break;
                        }
                        GameFiber.Yield();
                    }
                });
                RacketState = ERacketState.WaitingToLeaveScene;
            }
            if (!Scenario.Passenger.SafelyIsDeadOrArrested())
            {
                GameFiber.StartNew(delegate
                {
                    Scenario.Passenger.Tasks.Clear();
                    if (Scenario.Passenger.IsInAnyVehicle(false))
                    {
                        Scenario.Passenger.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
                    }
                    Scenario.Passenger.Tasks.FightAgainstClosestHatedTarget(100f);
                    Scenario.Passenger.AddCrimeToList("Aggravated Assault", 48);
                    while (!(RacketState == ERacketState.Ending))
                    {
                        if (Scenario.Passenger.SafelyIsDeadOrArrested(true))
                        {
                            while (!(RacketState == ERacketState.Ending))
                            {
                                if (!Scenario.Passenger.SafelyIsDeadOrArrested(true))
                                {
                                    if (Pursuit == null)
                                    {
                                        Pursuit = Functions.GetActivePursuit();
                                        if (Pursuit == null)
                                        {
                                            Pursuit = Functions.CreatePursuit();
                                            Game.LogTrivial("[GangsOfSouthLS] Starting Pursuit.");
                                        }
                                    }
                                    pursuitcreated = true;
                                    DropWeaponAndAddtoPursuit(Pursuit, Scenario.Passenger);
                                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                                    Game.LogTrivial("[GangsOfSouthLS] Adding Passenger to pursuit.");
                                    break;
                                }
                                GameFiber.Yield();
                            }
                            break;
                        }
                        GameFiber.Yield();
                    }
                });
            }
            firstloop = true;
        }

        internal void MakeStuffHappenWhenArrestingDuringPullover()
        {
            Game.LogTrivial("[GangsOfSouthLS] A suspect is getting arrested.");
            Functions.ForceEndCurrentPullover();
            if (UsefulFunctions.Decide(35))
            {
                MakeStuffHappenWhenStartingAFight();
            }
            else
            {
                Game.LogTrivial("[GangsOfSouthLS] The suspect was arrested safely.");
                RacketState = ERacketState.DriverSafelyArrested;
            }
        }

        internal void CheckAndReactIfVisibleInShop()
        {
            if (gangstersSeePlayer)
            {
                Game.LogTrivial("[GangsOfSouthLS] The gangsters see the player AFTER arriving at the shop.");
                RacketConversation.PlayConversationPartOfState(ConversationState.Surprised, true);
                if (UsefulFunctions.Decide(60))
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make attack after conversation.");
                    RacketState = ERacketState.AttackingAfterConversation;
                    firstloop = true;
                }
                else
                {
                    Game.LogTrivial("[GangsOfSouthLS] Decided to make flee after conversation.");
                    RacketState = ERacketState.FleeingAfterConversation;
                    firstloop = true;
                }
            }
        }

        internal void CheckAndReactIfVisibleBeforeArriving()
        {
            if (gangstersSeePlayer)
            {
                Game.LogTrivial("[GangsOfSouthLS] The gangsters see the player BEFORE arriving at the shop.");
                if (!(RacketState == ERacketState.Ending))
                {
                    Scenario.Driver.Tasks.Clear();
                    if (UsefulFunctions.Decide(20))
                    {
                        Game.LogTrivial("[GangsOfSouthLS] The gangsters are panicking, starting pursuit.");
                        Game.DisplaySubtitle("They saw you and are panicking. ~r~Get them now~w~!");
                        MakeStuffHappenWhenCreatingSelfmadePursuit();
                    }
                    else
                    {
                        Game.LogTrivial("[GangsOfSouthLS] The gangsters do the smart thing and just drive on.");
                        RacketState = ERacketState.DrivingPastTheShop;
                    }
                }
            }
        }

        internal void StartDoorCheck()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("[GangsOfSouthLS] Starting check if doors are open.");
                while (!endDoorCheck)
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

        internal void EndDoorCheck()
        {
            endDoorCheck = true;
        }

        internal void StartVisibleCheck()
        {
            Game.LogTrivial("[GangsOfSouthLS] Starting check if gangsters see player.");
            var gangsterList = new List<Ped> { Scenario.Driver, Scenario.Passenger };
            //check every tick if player is shooting or one of the gangsters is dead
            GameFiber.StartNew(delegate
            {
                while (Functions.IsCalloutRunning() && !endVisibleCheck)
                {
                    if (!(RacketState == ERacketState.Ending))
                    {
                        foreach (var gangster in gangsterList)
                        {
                            if (gangster.IsDead)
                            {
                                Game.LogTrivial("[GangsOfSouthLS] One of the gangsters died.");
                                gangstersSeePlayer = true;
                                endVisibleCheck = true;
                                break;
                            }
                            if (Game.LocalPlayer.Character.DistanceTo(gangster) < 80)
                            {
                                if (Game.LocalPlayer.Character.IsShooting)
                                {
                                    Game.LogTrivial("[GangsOfSouthLS] Player is shooting.");
                                    gangstersSeePlayer = true;
                                    endVisibleCheck = true;
                                    break;
                                }
                            }
                        }
                    }
                    GameFiber.Yield();
                }
            });
            //check every 200ms if player is visible:
            GameFiber.StartNew(delegate
            {
                while (Functions.IsCalloutRunning() && !endVisibleCheck)
                {
                    if (!(RacketState == ERacketState.Ending))
                    {
                        foreach (var gangster in gangsterList)
                        {
                            if (Game.LocalPlayer.Character.DistanceTo(gangster) < 80)
                            {
                                if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                                {
                                    if (Game.LocalPlayer.Character.CurrentVehicle.IsSirenOn && (Game.LocalPlayer.Character.DistanceTo(gangster) < 40))
                                    {
                                        if (!Game.LocalPlayer.Character.CurrentVehicle.IsSirenSilent)
                                        {
                                            Game.LogTrivial("[GangsOfSouthLS] Gangsters heard player's siren.");
                                            gangstersSeePlayer = true;
                                            endVisibleCheck = true;
                                            break;
                                        }
                                        else if (gangster.HasClearLosToEntity(Game.LocalPlayer.Character.CurrentVehicle))
                                        {
                                            Game.LogTrivial("[GangsOfSouthLS] Gangsters saw player's emergency lights.");
                                            gangstersSeePlayer = true;
                                            endVisibleCheck = true;
                                            break;
                                        }
                                    }
                                    if (!Game.LocalPlayer.Character.CurrentVehicle.IsPoliceVehicle || INIReader.UnmarkedCarList.Contains(Game.LocalPlayer.Character.CurrentVehicle.Model.Name.ToLower()))
                                    {
                                        if (!(Game.LocalPlayer.Character.DistanceTo(gangster) > 20))
                                        {
                                            if (Game.LocalPlayer.Character.DistanceTo(gangster) < 10f || gangster.HasClearLosToEntity(Game.LocalPlayer.Character.CurrentVehicle))
                                            {
                                                Game.LogTrivial("[GangsOfSouthLS] Gangsters see player in an unmarked car.");
                                                gangstersSeePlayer = true;
                                                endVisibleCheck = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Game.LocalPlayer.Character.DistanceTo(gangster) < 15f || gangster.HasClearLosToEntity(Game.LocalPlayer.Character.CurrentVehicle))
                                        {
                                            Game.LogTrivial("[GangsOfSouthLS] Gangsters see player in a non-unmarked car.");
                                            gangstersSeePlayer = true;
                                            endVisibleCheck = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!(Game.LocalPlayer.Character.DistanceTo(gangster) > 20))
                                    {
                                        if (Game.LocalPlayer.Character.DistanceTo(gangster) < 5f || gangster.HasClearLosToEntity(Game.LocalPlayer.Character))
                                        {
                                            Game.LogTrivial("[GangsOfSouthLS] Gangsters see player on foot.");
                                            gangstersSeePlayer = true;
                                            endVisibleCheck = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (Functions.IsPedGettingArrested(gangster) || Functions.IsPedArrested(gangster))
                            {
                                Game.LogTrivial("[GangsOfSouthLS] Player is arresting gangster.");
                                gangstersSeePlayer = true;
                                endVisibleCheck = true;
                                break;
                            }
                        }
                    }
                    GameFiber.Wait(200); //Check every 200 ms
                }
                Game.LogTrivial("[GangsOfSouthLS] Ending check if Gangsters see player.");
                return;
            });
        }

        internal void EndVisibleCheck()
        {
            endVisibleCheck = true;
        }

        internal void StartOnSceneCheck()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("[GangsOfSouthLS] Starting check if player is on scene.");
                while (Functions.IsCalloutRunning() && !endOnSceneCheck)
                {
                    if (Game.LocalPlayer.Character.DistanceTo(Scenario.Position) > 101f)
                    {
                        Game.DisplaySubtitle("~r~Get back to the shop!", 200);
                    }
                    GameFiber.Yield();
                }
                Game.LogTrivial("[GangsOfSouthLS] Ending check if player is on scene.");
            });
        }

        internal void EndOnSceneCheck()
        {
            endOnSceneCheck = true;
        }

        internal void StartDeletingStockMerchant()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("[GangsOfSouthLS] Starting looking for and deleting stock merchant.");
                while (Functions.IsCalloutRunning() && !endDeletingStockMerchant)
                {
                    foreach (var ped in World.GetEntities(Scenario.MerchantSpawnPos4.Position, 30f, GetEntitiesFlags.ConsiderAllPeds))
                    {
                        foreach (var model in Scenario.MerchantStringList)
                        {
                            if ((ped.Model.Name.ToLower() == model.ToLower()) && Scenario.Merchant.Exists() && !(ped == Scenario.Merchant))
                            {
                                ped.SafelyDelete();
                            }
                        }
                    }
                    GameFiber.Yield();
                }
                Game.LogTrivial("[GangsOfSouthLS] Ending looking for and deleting stock merchant.");
            });
        }

        internal void EndDeletingStockMerchant()
        {
            endDeletingStockMerchant = true;
        }

        internal void DropWeaponAndAddtoPursuit(LHandle pursuit, Ped ped)
        {
            if (!ped.SafelyIsDeadOrArrested())
            {
                if (ped.IsOnFoot && !(ped.Inventory.EquippedWeaponObject == null))
                {
                    ped.Inventory.EquippedWeapon.Drop();
                }
                Functions.AddPedToPursuit(pursuit, ped);
            }
        }

        internal bool HasPlayerLeftScene()
        {
            if (Game.LocalPlayer.Character.Position.DistanceTo(Scenario.Position) < 70f)
            {
                return false;
            }
            return true;
        }

        internal bool AreTheGangstersGone()
        {
            if (!Scenario.GangsterCar.Exists() || !Scenario.Driver.Exists() || !Scenario.Passenger.Exists())
            {
                return true;
            }
            else if (Game.LocalPlayer.Character.DistanceTo(Scenario.GangsterCar) < 70f)
            {
                return false;
            }
            return true;
        }

        //HELPER FUNCTIONS
        //END
    }
}