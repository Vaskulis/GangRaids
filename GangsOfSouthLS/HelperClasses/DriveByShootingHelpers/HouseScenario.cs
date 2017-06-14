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
    abstract class HouseScenario
    {
        internal string Address;
        internal List<MyPed> SuspectList;
        internal Vector3 Location;
        internal Blip Blip;
        internal EHouseState State;
        internal OwnerHouseScenarioTemplate ScenarioTemplate;


        internal HouseScenario(OwnerHouseScenarioTemplate scenarioTemplate, CrimeSceneScenario CSScenario)
        {
            ScenarioTemplate = scenarioTemplate;
            Address = ScenarioTemplate.Address;
            Location = ScenarioTemplate.PossibleCarSpawnList[0].Position;

            foreach (var susSpawn in ScenarioTemplate.PossibleSuspectSpawnList)
            {
                foreach (var ped in World.GetEntities(susSpawn.Position, 30f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed))
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
                    susNum++;
                    SuspectList.Add(sus);
                }
            }

            State = EHouseState.Initialized;
        }


        internal abstract void PlayAction();


        internal virtual void SetEnRoute()
        {
            State = EHouseState.EnRoute;
            Game.LogTrivial("[GangsOfSouthLS] En Route to " + Address + ".");
            Blip = new Blip(Location, 50f);
            Blip.Alpha = 0.5f;
            Blip.Color = Color.Yellow;
            Blip.EnableRoute(Color.Yellow);
        }


        internal virtual void End()
        {
            foreach (var sus in SuspectList)
            {
                sus.SafelyDelete();
            }
            Blip.SafelyDelete();
        }


        internal enum EHouseState
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
