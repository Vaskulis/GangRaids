﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GangRaids.HelperClasses;

namespace GangRaids.Scenarios
{
    class CopCarsBuilds
    {
        public static CopCarBuild police = new CopCarBuild
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

        public static CopCarBuild police2 = new CopCarBuild
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

        public static CopCarBuild police3 = new CopCarBuild
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

        public static CopCarBuild fbi2 = new CopCarBuild
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
