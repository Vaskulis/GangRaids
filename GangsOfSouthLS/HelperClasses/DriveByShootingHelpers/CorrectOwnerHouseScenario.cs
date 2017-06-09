using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.Menus;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    class CorrectOwnerHouseScenario
    {
        internal string Address;
        internal Vehicle SuspectCar;
        internal List<MyPed> SuspectList;
        internal Vector3 Location;
        internal Blip Blip;
        internal ECOHSState State;

        private List<string> SuspectPistolList = new List<string> { "weapon_pistol", "weapon_snspistol", "weapon_combatpistol", "weapon_pistol50", "weapon_microsmg" };
        private bool isCarThere;
        private bool HasPlayerBeenAddedToPursuit;
        private OwnerHouseScenarioTemplate ScenarioTemplate;
        private LHandle pursuit;



        internal CorrectOwnerHouseScenario(OwnerHouseScenarioTemplate scenarioTemplate, SuspectCarTemplate carTemplate, CrimeSceneScenario CSScenario)
        {
            ScenarioTemplate = scenarioTemplate;
            Address = ScenarioTemplate.Address;
            Location = ScenarioTemplate.PossibleCarSpawnList[0].Position;
            carTemplate.Address = Address;

            isCarThere = false;
            HasPlayerBeenAddedToPursuit = false;

            var carSpawn = ScenarioTemplate.PossibleCarSpawnList.RandomElement();
            foreach (var veh in World.GetEntities(carSpawn.Position, 5f, GetEntitiesFlags.ConsiderAllVehicles))
            {
                veh.SafelyDelete();
            }
            if (UsefulFunctions.Decide(70))
            {
                SuspectCar = carTemplate.CreateCar(carSpawn);
                SuspectCar.IsPersistent = true;
                isCarThere = true;
            }
            foreach (var susSpawn in ScenarioTemplate.PossibleSuspectSpawnList)
            {
                foreach (var ped in World.GetEntities(susSpawn.Position, 30f, GetEntitiesFlags.ConsiderHumanPeds))
                {
                    ped.SafelyDelete();
                }
            }

            SuspectList = new List<MyPed> { };
            var susNum = 0;
            foreach (var susSpawn in ScenarioTemplate.PossibleSuspectSpawnList)
            {
                if (UsefulFunctions.Decide(100 - susNum * 40))
                {
                    var sus = new MyPed(CSScenario.GangPedDict[CSScenario.SuspectGang].RandomElement(), susSpawn.Position, susSpawn.Heading);
                    sus.RandomizeVariation();
                    sus.RelationshipGroup = "DRIVEBY_SUSPECT";
                    sus.BlockPermanentEvents = true;
                    sus.IsPersistent = true;
                    sus.Inventory.GiveNewWeapon(SuspectPistolList.RandomElement(), 999, false);
                    susNum++;
                    SuspectList.Add(sus);
                }
            }
            Game.LogTrivial("[GangsOfSouthLS] Initialized CorrectOwnerHouseScenario.");
            Game.LogTrivial("[GangsOfSouthLS] Spawned " + susNum + " suspects.");
            Game.LogTrivial("[GangsOfSouthLS] Is the car there: " + isCarThere);

            State = ECOHSState.Initialized;
        }


        internal void PlayAction()
        {
            if (State == ECOHSState.Initialized)
            {
                SetEnRoute();
            }

            if (State == ECOHSState.EnRoute)
            {
                if (Game.LocalPlayer.Character.DistanceTo(Location) < 40f)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Arrived at correct owner's house.");
                    Blip.SafelyDelete();
                    State = ECOHSState.Arrived;
                }
                foreach (var susSpawn in ScenarioTemplate.PossibleSuspectSpawnList)
                {
                    foreach (var ped in World.GetEntities(susSpawn.Position, 30f, GetEntitiesFlags.ConsiderHumanPeds))
                    {
                        if (!SuspectList.Contains(ped))
                        {
                            ped.SafelyDelete();
                        }
                    }
                }
            }

            if (State == ECOHSState.Arrived)
            {
                if (UsefulFunctions.Decide(20))
                {
                    foreach (var sus in SuspectList)
                    {
                        sus.Type = MyPed.EType.Suspect;
                    }
                    MakeSuspectsFightOrFlee();
                }
                else
                {
                    foreach (var sus in SuspectList)
                    {
                        sus.Type = MyPed.EType.Unknown;
                    }
                    MakeSuspectsTalk();
                }
            }

            if (State == ECOHSState.FightingOrFleeing)
            {
                GameFiber.Yield();
                var stillFightingOrFleeing = false;
                foreach (var suspect in SuspectList)
                {
                    if (suspect.SafelyIsDeadOrArrested())
                    {
                        suspect.Blip.SafelyDelete();
                    }
                    else
                    {
                        stillFightingOrFleeing = true;
                    }
                }                
                if (!stillFightingOrFleeing)
                {
                    State = ECOHSState.Ending;
                }
            }

            if (State == ECOHSState.Talking)
            {
                Game.DisplayHelp("Press ~b~Y ~w~while standing close to a ~o~suspect ~w~to identify the owner and arrest them.");
            }
        }


        private void SetEnRoute()
        {
            State = ECOHSState.EnRoute;
            Game.LogTrivial("[GangsOfSouthLS] En Route to correct owner.");
            Blip = new Blip(Location, 50f);
            Blip.Alpha = 0.5f;
            Blip.Color = Color.Yellow;
            Blip.EnableRoute(Color.Yellow);
        }


        private void MakeSuspectsFightOrFlee()
        {
            State = ECOHSState.FightingOrFleeing;

            pursuit = Functions.CreatePursuit();
            Game.SetRelationshipBetweenRelationshipGroups("DRIVEBY_SUSPECT", "PLAYER", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("DRIVEBY_SUSPECT", "COP", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("COP", "DRIVEBY_SUSPECT", Relationship.Hate);

            if (isCarThere && UsefulFunctions.Decide(70))
            {
                GameFiber.StartNew(delegate
                {
                    SuspectList[0].Type = MyPed.EType.Suspect;
                    SuspectList[0].AddBlip();
                    SuspectList[0].Tasks.EnterVehicle(SuspectCar, -1, 3f);
                    GameFiber.WaitUntil(() => SuspectList[0].IsInAnyVehicle(false), 6000);
                    if (SuspectList[0].IsInAnyVehicle(false))
                    {
                        AddToPursuitAndDeleteBlip(SuspectList[0]);
                    }
                    else
                    {
                        SuspectList[0].Tasks.FightAgainstClosestHatedTarget(50f);
                    }
                });
            }
            else
            {
                SuspectList[0].Tasks.FightAgainstClosestHatedTarget(50f);
            }

            foreach (var suspect in SuspectList.Where(n => n != SuspectList[0]))
            {
                suspect.Type = MyPed.EType.Suspect;
                if (UsefulFunctions.Decide(70))
                {
                    suspect.AddBlip();
                    suspect.Tasks.FightAgainstClosestHatedTarget(50f);
                }
                else
                {
                    AddToPursuitAndDeleteBlip(suspect);
                }
            }
        }


        private void MakeSuspectsTalk()
        {
            State = ECOHSState.Talking;

            var QuestionList = new List<string>
            {
                "LSPD. I am looking for " + SuspectList[0].Name + ".",
                "Good day! I am here for "  + SuspectList[0].Name + "."
            };

            var PrimarySuspectPlainReplyList = new List<string>
            {
                "Man, you must be mistaking me for somebody else.",
                "Yeah, that's me. So what?",
                "I don't know, I think I have heard that name before.",
                "What? Well look somewhere else!"
            };

            var PrimarySuspectReplyActionList = new List<string>
            {
                "Oh hell no!",
                "I don't think so."
            };

            var SecondarySuspectPlainReplyList = new List<string>
            {
                "I don't know, I think I have heard that name before.",
                "What do you want from him? I can give him an alibi for any time you want.",
                "What? Well look somewhere else!"
            };

            var SecondarySuspectReplyActionList = new List<string>
            {
                "Oh hell no!",
                "I don't think so.",
                "This wasn't supposed to happen!"
            };

            foreach (var sus in SuspectList)
            {
                sus.AddBlip();
                sus.StartNewConversation();
                sus.Conversation.AddAskForIDItem(70);
                if (sus != SuspectList[0])
                {
                    if (UsefulFunctions.Decide(20))
                    {
                        sus.Conversation.AddLine(QuestionList.RandomElement(), SecondarySuspectReplyActionList.RandomElement(), MakeSuspectsFightOrFlee);
                    }
                    else
                    {
                        sus.Conversation.AddLine(QuestionList.RandomElement(), SecondarySuspectPlainReplyList.RandomElement());
                    }
                }
                else
                {
                    {
                        if (UsefulFunctions.Decide(30))
                        {
                            sus.Conversation.AddLine(QuestionList.RandomElement(), PrimarySuspectReplyActionList.RandomElement(), MakeSuspectsFightOrFlee);
                        }
                        else
                        {
                            sus.Conversation.AddLine(QuestionList.RandomElement(), PrimarySuspectPlainReplyList.RandomElement());
                        }
                    }
                }
            }
        }


        private void AddToPursuitAndDeleteBlip(MyPed suspect)
        {
            suspect.Blip.SafelyDelete();
            Functions.AddPedToPursuit(pursuit, suspect);
            if (!HasPlayerBeenAddedToPursuit)
            {
                HasPlayerBeenAddedToPursuit = true;
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
            }
        }



        internal void End()
        {
            foreach (var sus in SuspectList)
            {
                sus.SafelyDelete();
            }
            Blip.SafelyDelete();
            SuspectCar.SafelyDelete();
        }


        internal enum ECOHSState
        {
            Initialized,
            EnRoute,
            Arrived,
            FightingOrFleeing,
            Talking,
            Ending
        }
    }
}
