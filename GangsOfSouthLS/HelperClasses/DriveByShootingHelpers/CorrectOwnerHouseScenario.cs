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
    class CorrectOwnerHouseScenario : HouseScenario
    {
        internal Vehicle SuspectCar;
        internal MyPed Owner;

        private List<string> SuspectPistolList = new List<string> { "weapon_pistol", "weapon_snspistol", "weapon_combatpistol", "weapon_pistol50", "weapon_microsmg" };
        private bool isCarThere;
        private bool HasPlayerBeenAddedToPursuit;
        private LHandle pursuit;


        internal CorrectOwnerHouseScenario(OwnerHouseScenarioTemplate scenarioTemplate, SuspectCarTemplate carTemplate, CrimeSceneScenario CSScenario) : base(scenarioTemplate, CSScenario)
        {
            carTemplate.Address = Address;

            isCarThere = false;
            HasPlayerBeenAddedToPursuit = false;

            var carSpawn = ScenarioTemplate.PossibleCarSpawnList.RandomElement();
            foreach (var veh in World.GetEntities(carSpawn.Position, 5f, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle))
            {
                veh.SafelyDelete();
            }
            if (UsefulFunctions.Decide(70))
            {
                SuspectCar = carTemplate.CreateCar(carSpawn);
                SuspectCar.IsPersistent = true;
                isCarThere = true;
            }

            foreach (var sus in SuspectList)
            {
                sus.Inventory.GiveNewWeapon(SuspectPistolList.RandomElement(), 999, false);
            }

            Owner = SuspectList[0];

            Game.LogTrivial("[GangsOfSouthLS] Initialized CorrectOwnerHouseScenario.");
            Game.LogTrivial("[GangsOfSouthLS] Spawned " + SuspectList.Count + " suspects.");
            Game.LogTrivial("[GangsOfSouthLS] Is the car there: " + isCarThere);

        }


        internal override void PlayAction()
        {
            if (State == EHouseState.Initialized)
            {
                SetEnRoute();
            }

            if (State == EHouseState.EnRoute)
            {
                if (Game.LocalPlayer.Character.DistanceTo(Location) < 40f)
                {
                    Game.LogTrivial("[GangsOfSouthLS] Arrived at correct owner's house.");
                    Blip.SafelyDelete();
                    State = EHouseState.Arrived;
                }
                foreach (var susSpawn in ScenarioTemplate.PossibleSuspectSpawnList)
                {
                    foreach (var ped in World.GetEntities(susSpawn.Position, 30f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed))
                    {
                        if (!SuspectList.Contains(ped))
                        {
                            ped.SafelyDelete();
                        }
                    }
                }
            }

            if (State == EHouseState.Arrived)
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

            if (State == EHouseState.FightingOrFleeing)
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
                    State = EHouseState.Ending;
                }
            }

            if (State == EHouseState.Talking)
            {
                GameFiber.Yield();
                Game.DisplayHelp("Press ~b~Y ~w~while standing close to a ~o~suspect ~w~to identify the owner and arrest them.");
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
                    State = EHouseState.Ending;
                }

            }
        }


        internal override void SetEnRoute()
        {
            base.SetEnRoute();
            Game.LogTrivial("[GangsOfSouthLS] En Route to correct owner.");
        }


        internal override void End()
        {
            base.End();
            SuspectCar.SafelyDelete();
        }


        private void MakeSuspectsFightOrFlee()
        {
            State = EHouseState.FightingOrFleeing;

            pursuit = Functions.CreatePursuit();
            Game.SetRelationshipBetweenRelationshipGroups("DRIVEBY_SUSPECT", "PLAYER", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("DRIVEBY_SUSPECT", "COP", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("COP", "DRIVEBY_SUSPECT", Relationship.Hate);

            if (isCarThere && UsefulFunctions.Decide(70))
            {
                GameFiber.StartNew(delegate
                {
                    Owner.Type = MyPed.EType.Suspect;
                    Owner.AddBlip();
                    Owner.Tasks.EnterVehicle(SuspectCar, -1, 3f);
                    GameFiber.WaitUntil(() => Owner.IsInAnyVehicle(false), 6000);
                    if (Owner.IsInAnyVehicle(false))
                    {
                        AddToPursuitAndDeleteBlip(Owner);
                    }
                    else
                    {
                        Owner.Tasks.FightAgainstClosestHatedTarget(50f);
                    }
                });
            }
            else
            {
                Owner.AddBlip();
                Owner.Tasks.FightAgainstClosestHatedTarget(50f);
            }

            foreach (var suspect in SuspectList.Where(n => n != Owner))
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
            State = EHouseState.Talking;

            var QuestionList = new List<string>
                {
                    "LSPD. I am looking for " + Owner.Name + ".",
                    "Good day! I am here for "  + Owner.Name + "."
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
                if (sus != Owner)
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
                        if (UsefulFunctions.Decide(20))
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
    }
}
