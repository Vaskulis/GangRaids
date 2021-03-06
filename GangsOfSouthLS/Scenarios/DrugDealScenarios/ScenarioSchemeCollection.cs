﻿using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.DrugDealHelpers;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.Scenarios.DrugDealScenarios
{
    internal static class ScenarioSchemeCollection
    {
        internal static ScenarioScheme Scenario1 = new ScenarioScheme
        (
            "Scenario1",
            new List<CopCarWayPoint>
            {
                new CopCarWayPoint("W - Popular St", new Pos4(796.9736f, -1056.078f, 26.79304f, 6.348056f), new Pos4(829.3022f, -1044.76f, 26.9541f, 222.386f), "WEST"),
                new CopCarWayPoint("NE - Vespucci Blvd", new Pos4(885.6469f, -996.2393f, 32.01857f, 93.51751f), new Pos4(860.551f, -1024.535f, 29.65693f, 179.4727f), "NORTH"),
                new CopCarWayPoint("SW - Supply St", new Pos4(811.8109f, -1091.278f, 28.0963f, 269.4472f), new Pos4(860.8483f, -1068.41f, 28.05779f, 359.7094f), "SOUTH")
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
            new Vector3(859.6678f, -1057.579f, 28.01464f)
        );

        internal static ScenarioScheme Scenario2 = new ScenarioScheme
        (
            "Scenario2",
            new List<CopCarWayPoint>
            {
                        new CopCarWayPoint("NE - Carson Ave", new Pos4(306.75f, -1918.413f, 25.37817f, 142.7733f), new Pos4(262.7377f, -1964.95f, 22.31196f, 140.8337f), "NORTH"),
                        new CopCarWayPoint("SE - Jamestown St", new Pos4(296.4449f, -1995.007f, 20.25746f, 145.2198f), new Pos4(254.7816f, -1992.251f, 19.77044f, 55.70863f), "EAST"),
                        new CopCarWayPoint("SW - Dutch London St", new Pos4(209.1329f, -2030.292f, 17.8418f, 328.9295f), new Pos4(243.2737f, -1993.599f, 19.48059f, 324.3007f), "SOUTH")
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
                        new Pos4(243.7167f, -1971.37f, 21.48446f, 68.92776f),
                        new Pos4(248.1484f, -1968.349f, 21.52233f, 206.5595f),
                        new Pos4(251.9744f, -1976.815f, 21.21008f, 19.3455f)
            },
            new List<Pos4>
            {
                        new Pos4(250.8079f, -1969.246f, 21.95018f, 170.2963f),
                        new Pos4(248.0841f, -1972.67f, 21.90929f, 254.6881f),
                        new Pos4(244.987f, -1968.3f, 21.96168f, 217.9159f),
                        new Pos4(253.0257f, -1974.011f, 21.86953f, 68.35076f),
                        new Pos4(250.177f, -1972.815f, 21.87804f, 86.93569f),
            },
            new Vector3(251.9744f, -1976.815f, 21.21008f)
        );

        internal static ScenarioScheme Scenario3 = new ScenarioScheme
        (
            "Scenario3",
            new List<CopCarWayPoint>
            {
                                new CopCarWayPoint("SE - Davis Ave", new Pos4(199.9875f, -1559.346f, 28.87995f, 38.28777f), new Pos4(160.4086f, -1517.683f, 28.75009f, 61.17656f), "EAST"),
                                new CopCarWayPoint("NW - Strawberry Ave", new Pos4(115.5835f, -1476.35f, 28.81214f, 229.6633f), new Pos4(146.7929f, -1498.214f, 28.74943f, 210.4924f), "WEST"),
                                new CopCarWayPoint("SW - Macdonald St", new Pos4(119.1578f, -1532.491f, 28.79548f, 322.5087f), new Pos4(139.1225f, -1509.266f, 28.74949f, 296.9163f), "SOUTH")
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
                                new Pos4(143.3773f, -1513.609f, 28.64872f, 140.6087f),
                                new Pos4(148.0222f, -1517.89f, 28.70807f, 346.7974f),
                                new Pos4(148.6301f, -1506.425f, 28.76328f, 189.0623f)
            },
            new List<Pos4>
            {
                                new Pos4(149.5296f, -1515.066f, 29.14162f, 359.0868f),
                                new Pos4(146.3812f, -1512.353f, 29.14162f, 321.2526f),
                                new Pos4(152.9196f, -1514.322f, 29.14162f, 49.88635f),
                                new Pos4(145.6923f, -1506.042f, 29.14162f, 194.7787f),
                                new Pos4(146.7179f, -1509.394f, 29.14162f, 145.1558f),
            },
            new Vector3(148.6301f, -1506.425f, 28.76328f)
        );

        internal static ScenarioScheme Scenario4 = new ScenarioScheme
        (
            "Scenario4",
            new List<CopCarWayPoint>
            {
                                        new CopCarWayPoint("S - Davis Ave", new Pos4(-219.9544f, -1723.483f, 32.33878f, 49.08244f), new Pos4(-240.4327f, -1673.807f, 33.09414f, 358.6898f), "SOUTH"),
                                        new CopCarWayPoint("N - Carson Ave", new Pos4(-228.2108f, -1564.896f, 33.39318f, 143.45f), new Pos4(-240.6482f, -1624.891f, 33.16191f, 179.37f), "NORTH"),
                                        new CopCarWayPoint("E - Forum Dr", new Pos4(-188.4228f, -1611.44f, 33.42591f, 181.3878f), new Pos4(-231.0156f, -1634.881f, 33.17455f, 96.69217f), "EAST")
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
                                        new Pos4(-243.8134f, -1654.289f, 33.3615f, 179.6201f),
                                        new Pos4(-240.1096f, -1659.377f, 32.90593f, 357.9749f),
                                        new Pos4(-240.4324f, -1644.087f, 33.05951f, 179.053f)
            },
            new List<Pos4>
            {
                                        new Pos4(-238.7602f, -1657.735f, 33.73918f, 0.6454393f),
                                        new Pos4(-240.3228f, -1650.345f, 33.51057f, 357.401f),
                                        new Pos4(-237.304f, -1654.311f, 34.05693f, 47.01286f),
                                        new Pos4(-242.1824f, -1645.592f, 33.53774f, 192.4913f),
                                        new Pos4(-240.0979f, -1647.766f, 33.53027f, 182.7434f),
            },
            new Vector3(-240.4324f, -1644.087f, 33.05951f)
        );

        internal static ScenarioScheme Scenario5 = new ScenarioScheme
        (
            "Scenario5",
            new List<CopCarWayPoint>
            {
                        new CopCarWayPoint("E - Lowenstein Blvd", new Pos4(501.7538f, -1541.414f, 28.87946f, 136.1422f), new Pos4(463.6501f, -1525.564f, 28.87494f, 44.61883f), "EAST"),
                        new CopCarWayPoint("S - Innocence Blvd", new Pos4(418.6312f, -1563.713f, 28.88379f, 324.8923f), new Pos4(453.9618f, -1526.611f, 28.67854f, 319.8857f), "SOUTH"),
                        new CopCarWayPoint("N - Davis Ave", new Pos4(415.3104f, -1470.966f, 28.88953f, 298.9574f), new Pos4(440.3405f, -1504.599f, 28.89815f, 226.3646f), "NORTH")
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
                        new Pos4(456.6226f, -1508.514f, 28.79684f, 26.88555f),
                        new Pos4(451.4465f, -1509.889f, 28.79695f, 231.8612f),
                        new Pos4(459.4813f, -1516.511f, 28.8147f, 40.00843f)
            },
            new List<Pos4>
            {
                        new Pos4(454.3969f, -1510.776f, 29.2922f, 218.781f),
                        new Pos4(458.2578f, -1511.595f, 29.29019f, 193.3457f),
                        new Pos4(451.0422f, -1515.767f, 29.06611f, 292.471f),
                        new Pos4(463.1635f, -1515.018f, 29.2865f, 58.98998f),
                        new Pos4(459.371f, -1513.07f, 29.2896f, 32.47529f),
            },
            new Vector3(459.4813f, -1516.511f, 28.8147f)
        );

        internal static List<ScenarioScheme> ScenarioSchemeList = new List<ScenarioScheme>
        {
            Scenario1,
            Scenario2,
            Scenario3,
            Scenario4,
            Scenario5
        };
    }
}