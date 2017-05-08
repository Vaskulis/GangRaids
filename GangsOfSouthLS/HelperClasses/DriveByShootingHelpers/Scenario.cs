using GangsOfSouthLS.HelperClasses.CommonUtilities;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    class Scenario
    {
        internal CrimeSceneScenarioTemplate Template;
        internal Vector3 Position { get; private set; }
        internal List<MyPed> WitnessList { get; private set; }
        internal List<MyPed> VictimList { get; private set; }
        internal List<MyPed> MedicList { get; private set; }
        internal Vehicle Ambulance { get; private set; }
        internal Dictionary<EGangName, List<string>> GangPedDict = new Dictionary<EGangName, List<string>>
        {
            { EGangName.Ballas, new List<string> { "g_m_y_ballaeast_01" , "g_m_y_ballaorig_01", "g_m_y_ballasout_01" }},
            { EGangName.Families, new List<string> { "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" }},
            { EGangName.Lost, new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03" }},
            { EGangName.Vagos, new List<string> { "g_m_y_mexgang_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03" }}
        };
        internal List<Tuple<string, string>> MedicAnimationList = new List<Tuple<string, string>>
        {
            Tuple.Create("amb@medic@standing@kneel@idle_a", "idle_a"),
            Tuple.Create("amb@medic@standing@kneel@idle_a", "idle_b"),
            Tuple.Create("amb@medic@standing@kneel@idle_a", "idle_c")
        };
        internal List<Tuple<string, string>> MaleWitnessIdleList = new List<Tuple<string, string>>
        {
            Tuple.Create("amb@world_human_stand_impatient@male@no_sign@idle_a", "idle_a"),
            Tuple.Create("amb@world_human_stand_impatient@male@no_sign@idle_a", "idle_b"),
            Tuple.Create("amb@world_human_stand_impatient@male@no_sign@idle_a", "idle_c"),
        };
        internal List<Tuple<string, string>> FemaleWitnessIdleList = new List<Tuple<string, string>>
        {
            Tuple.Create("amb@world_human_stand_impatient@female@no_sign@idle_a", "idle_a"),
            Tuple.Create("amb@world_human_stand_impatient@female@no_sign@idle_a", "idle_b"),
            Tuple.Create("amb@world_human_stand_impatient@female@no_sign@idle_a", "idle_c"),
        };
        internal EGangName VictimGang;
        internal EGangName SuspectGang;

        internal Scenario(CrimeSceneScenarioTemplate template)
        {
            Template = template;
            Position = Template.Position;
            var gangNameList = new List<EGangName>((EGangName[])Enum.GetValues(typeof(EGangName)));

            VictimGang = gangNameList.RandomElement();
            gangNameList.Remove(VictimGang);
            SuspectGang = gangNameList.RandomElement();
        }

        internal void Initialize()
        {
            Ambulance = new Vehicle("ambulance", Template.AmbulanceSpawn.Position, Template.AmbulanceSpawn.Heading);
            Ambulance.IsSirenOn = true;
            Ambulance.IsEngineOn = true;
            Ambulance.Doors[2].Open(true);
            Ambulance.Doors[3].Open(true);

            WitnessList = new List<MyPed> { };
            var witnum = 0;
            Template.WitnessSpawnList.Shuffle();
            foreach (var witPos in Template.WitnessSpawnList)
            {
                if (UsefulFunctions.Decide(100 - (witnum * 20)))
                {
                    var wit = new MyPed(witPos.Position, witPos.Heading);
                    wit.RandomizeVariation();
                    wit.BlockPermanentEvents = true;
                    wit.IsPersistent = true;
                    WitnessList.Add(wit);
                    witnum++;
                }
            }
            Game.LogTrivial("[GangsOfSouthLS] Spawned " + WitnessList.Count + " witnesses.");

            VictimList = new List<MyPed> { };
            var vicNum = 0;
            Template.VictimSpawnList.Shuffle();
            foreach (var vicPos in Template.VictimSpawnList)
            {
                if (UsefulFunctions.Decide(100 - (vicNum * 35)))
                {
                    MyPed vic;
                    if (UsefulFunctions.Decide(100 - (vicNum * 40)))
                    {
                        vic = new MyPed(GangPedDict[VictimGang].RandomElement(), vicPos.Position, vicPos.Heading);
                    }
                    else
                    {
                        vic = new MyPed(vicPos.Position, vicPos.Heading);
                    }
                    vic.RandomizeVariation();
                    VictimList.Add(vic);
                    vic.BlockPermanentEvents = true;
                    vic.IsPersistent = true;
                    vicNum++;
                    vic.Kill();
                }
            }
            Game.LogTrivial("[GangsOfSouthLS] Spawned " + VictimList.Count + " victims.");

            MedicList = new List<MyPed> { };
            for (int i = 0; i < 2; i++)
            {
                var medic = new MyPed("s_m_m_paramedic_01", Vector3.Zero, 0f);
                medic.IsPersistent = true;
                medic.RandomizeVariation();
                medic.BlockPermanentEvents = true;
                medic.IsInvincible = true;
                if (VictimList.Count == 1)
                {
                    medic.Position = VictimList[0].GetOffsetPositionRight(.5f - i);
                    medic.Face(VictimList[0]);
                }
                else
                {
                    medic.Position = VictimList[i].GetOffsetPositionRight(.5f);
                    medic.Face(VictimList[i]);
                }
                MedicList.Add(medic);
            }
        }

        internal enum EGangName
        {
            Ballas, Families, Lost, Vagos
        }
    }
}
