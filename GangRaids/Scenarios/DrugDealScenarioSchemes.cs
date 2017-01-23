using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GangRaids.HelperClasses;
using Rage;

namespace GangRaids.Scenarios
{
    class DrugDealScenarioSchemes
    {
        public static DrugDealScenarioScheme Scenario1 = new DrugDealScenarioScheme
        (
            "Scenario1",
            new List<CopCarWayPoint>
            {
                new CopCarWayPoint("W - Popular St", new Pos4(796.9736f, -1056.078f, 26.79304f, 6.348056f), new Pos4(829.3022f, -1044.76f, 26.9541f, 222.386f)),
                new CopCarWayPoint("NE - Vespucci Blvd", new Pos4(885.6469f, -996.2393f, 32.01857f, 93.51751f), new Pos4(860.551f, -1024.535f, 29.65693f, 179.4727f)),
                new CopCarWayPoint("SW - Supply St", new Pos4(811.8109f, -1091.278f, 28.0963f, 269.4472f), new Pos4(860.8483f, -1068.41f, 28.05779f, 359.7094f))
            },
            new List<CopCarBuild>
            {
                CopCarsBuilds.police,
                CopCarsBuilds.police2,
                CopCarsBuilds.police3,
                CopCarsBuilds.fbi2
            },
            new List<Pos4>
            {
                new Pos4(855.5496f, -1046.935f, 28.21676f, 10.68289f),
                new Pos4(849.7835f, -1050.015f, 27.59768f, 224.4843f),
                new Pos4(859.6678f, -1057.579f, 28.01464f, 19.73039f)
            },
            new List<Pos4>
            {
                new Pos4(851.9349f, -1052.366f, 28.16158f, 228.0347f),
                new Pos4(856.4052f, -1051.159f, 28.58391f, 199.6283f),
                new Pos4(850.6267f, -1044.873f, 28.30832f, 205.3668f),
                new Pos4(859.9125f, -1054.038f, 28.69007f, 34.79546f),
                new Pos4(857.0261f, -1052.339f, 28.55039f, 17.43996f),
            },
            new Vector3(859.6678f, -1057.579f, 28.01464f),
            9400
        );

        public static List<DrugDealScenarioScheme> ScenarioSchemeList = new List<DrugDealScenarioScheme>
        {
            Scenario1
        };

        public static DrugDealScenario ChooseScenario()
        {
            return new DrugDealScenario(ScenarioSchemeList.RandomElement());
        }
    }
}
