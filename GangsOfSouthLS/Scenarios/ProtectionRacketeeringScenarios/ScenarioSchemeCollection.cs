using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.Scenarios.ProtectionRacketeeringScenarios
{
    internal static class ScenarioSchemeCollection
    {
        private static ScenarioScheme Scenario1 = new ScenarioScheme
        (
            "Strawberry 247 Supermarket",
            new Pos4(25.02721f, -1342.898f, 29.49704f, 221.721f),
            new Pos4(27.09624f, -1345.195f, 29.49704f, 40.14472f),
            new List<string> { "s_f_m_sweatshop_01", "mp_m_shopkeep_01" },
            new Vector3(29.0f, -1349.0f, 30.0f),
            new List<string> { "v_ilev_247door_r", "v_ilev_247door" },
            new List<CarWaypointCollection>
            {
                new CarWaypointCollection(new Pos4(-43.08104f, -1251.596f, 35.45556f, 272.0149f), new Pos4(42.9661f, -1359.115f, 28.87617f, 89.5684f), new Pos4(26.61937f, -1356.843f, 28.84396f, 90.90969f)),
                new CarWaypointCollection(new Pos4(103.4353f, -1166.005f, 30.11271f, 90.66514f), new Pos4(42.9661f, -1359.115f, 28.87617f, 89.5684f), new Pos4(26.61937f, -1356.843f, 28.84396f, 90.90969f))
            }
        );

        private static ScenarioScheme Scenario2 = new ScenarioScheme
        (
            "Davis Herr Kutz Barber",
            new Pos4(137.8611f, -1706.771f, 29.29162f, 132.6466f),
            new Pos4(136.5526f, -1707.918f, 29.29162f, 311.1761f),
            new List<string> { "s_f_m_fembarber" },
            new Vector3(133.2188f, -1711.565f, 29.29166f),
            new List<string> { "V_ILEV_BS_DOOR" },
            new List<CarWaypointCollection>
            {
                new CarWaypointCollection(new Pos4(238.6752f, -1709.307f, 28.93003f, 140.1303f), new Pos4(146.8948f, -1735.57f, 28.73005f, 50.13921f), new Pos4(129.3942f, -1716.341f, 29.00904f, 50.45683f)),
                new CarWaypointCollection(new Pos4(122.0899f, -1838.084f, 25.91018f, 319.421f), new Pos4(146.8948f, -1735.57f, 28.73005f, 50.13921f), new Pos4(129.3942f, -1716.341f, 29.00904f, 50.45683f)),
                new CarWaypointCollection(new Pos4(251.5134f, -1821.831f, 26.6742f, 52.72974f), new Pos4(146.8948f, -1735.57f, 28.73005f, 50.13921f), new Pos4(129.3942f, -1716.341f, 29.00904f, 50.45683f))
            },
            "HERRKUTZBARBERSHOP"
        );

        private static ScenarioScheme Scenario3 = new ScenarioScheme
        (
            "Strawberry Discount Store",
            new Pos4(75.82664f, -1390.549f, 29.37615f, 258.0114f),
            new Pos4(78.53907f, -1391.068f, 29.37614f, 81.13006f),
            new List<string> { "s_f_y_shop_low", "s_f_y_shop_mid" },
            new Vector3(82.40263f, -1391.558f, 29.39009f),
            new List<string> { "V_ILEV_CS_DOOR01", "V_ILEV_CS_DOOR01_R" },
            new List<CarWaypointCollection>
            {
                new CarWaypointCollection(new Pos4(96.63424f, -1527.235f, 28.75929f, 50.31038f), new Pos4(106.3955f, -1413.516f, 28.87475f, 47.09685f), new Pos4(92.64221f, -1396.5f, 28.8469f, 23.8403f)),
                new CarWaypointCollection(new Pos4(-72.16184f, -1377.302f, 28.74361f, 266.3362f), new Pos4(85.66916f, -1378.602f, 28.83008f, 185.6659f), new Pos4(85.84813f, -1392.223f, 28.85566f, 180.1211f))
            },
            "SOUTHLSATMDISCOUNTSTORE"
        );

        private static ScenarioScheme Scenario4 = new ScenarioScheme
        (
            "Davis LTD Gas Station",
            new Pos4(-44.41323f, -1754.978f, 29.42101f, 86.94448f),
            new Pos4(-47.73475f, -1755.601f, 29.42101f, 282.1713f),
            new List<string> { "mp_m_shopkeep_01" },
            new Vector3(-52.95604f, -1756.482f, 29.42211f),
            new List<string> { "V_ILEV_GASDOOR", "V_ILEV_GASDOOR_R" },
            new List<CarWaypointCollection>
            {
                        new CarWaypointCollection(new Pos4(38.32006f, -1877.808f, 21.71684f, 51.08133f), new Pos4(-69.93227f, -1777.064f, 27.84576f, 344.2718f), new Pos4(-63.52161f, -1753.819f, 28.63155f, 5.56369f)),
            },
            "NONE"
        );

        internal static List<ScenarioScheme> ScenarioSchemeList = new List<ScenarioScheme>
        {
            Scenario1,
            //Scenario2,
            //Scenario3,
            //Scenario4
        };
    }
}