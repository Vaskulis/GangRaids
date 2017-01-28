using System.Collections.Generic;
using GangRaids.HelperClasses.DrugDealHelpers;

namespace GangRaids.Scenarios.DrugDealScenarios
{
    class CopCarsBuilds
    {
        internal static CopCarBuild police = new CopCarBuild
        (
            "Police",
            new List<string>
            {
                "s_m_y_cop_01",
                "s_f_y_cop_01"
            },
            new Dictionary<string, List<string>>
            {
                {"weapon_pistol", new List<string> {"component_at_pi_flsh"} }
            },
            new List<int> { -1, 0 }
        );

        internal static CopCarBuild police2 = new CopCarBuild
        (
            "Police2",
            new List<string>
            {
                "s_m_y_cop_01",
                "s_f_y_cop_01"
            },
            new Dictionary<string, List<string>>
            {
                {"weapon_pistol", new List<string> {"component_at_pi_flsh"} }
            },
            new List<int> { -1, 0 }
        );

        internal static CopCarBuild police3 = new CopCarBuild
        (
            "Police3",
            new List<string>
            {
                "s_m_y_cop_01",
                "s_f_y_cop_01"
            },
            new Dictionary<string, List<string>>
            {
                {"weapon_pistol", new List<string> {"component_at_pi_flsh"} }
            },
            new List<int> { -1, 0 }
        );

        internal static CopCarBuild fbi2 = new CopCarBuild
        (
            "FBI2",
            new List<string>
            {
                "s_m_y_swat_01"
            },
            new Dictionary<string, List<string>>
            {
                {"weapon_carbinerifle", new List<string> {"component_at_ar_flsh", "component_at_scope_medium"} },
                {"weapon_pumpshotgun", new List<string> {"component_at_ar_flsh"} },
                {"weapon_smg", new List<string> {"component_at_ar_flsh"} },
                {"weapon_assaultshotgun", new List<string> {"component_at_ar_flsh"} },
                {"weapon_combatmg", new List<string> { } }
            },
            new List<int> { -1, 3, 4, 5, 6}
        );
    }
}
