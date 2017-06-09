using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.HelperClasses.DriveByShootingHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.ScenarioCollections.DriveByShootingScenarios
{
    internal static class OwnerHouseScenarioTemplateCollection
    {
        internal static OwnerHouseScenarioTemplate Loc1 = new OwnerHouseScenarioTemplate
        (
            "136 Lowenstein Blvd",
            new List<Pos4>
            {
                new Pos4(430.6971f, -1715.947f, 28.88851f, 50.16858f),
                new Pos4(419.9437f, -1713.541f, 28.72921f, 321.0547f),
                new Pos4(430.3282f, -1715.731f, 28.88969f, 232.6785f),
            },
            new List<Pos4>
            {
                new Pos4(432.7789f, -1714.067f, 29.43012f, 74.28738f),
                new Pos4(431.0182f, -1725.447f, 29.60144f, 56.16805f),
                new Pos4(430.4247f, -1720.826f, 29.39616f, 29.12476f),
                new Pos4(442.0478f, -1723.501f, 29.33783f, 223.9738f),
                new Pos4(443.7695f, -1736.999f, 29.22107f, 334.4265f),
            }
        );
        internal static OwnerHouseScenarioTemplate Loc2 = new OwnerHouseScenarioTemplate
        (
            "375 Jamestown St",
            new List<Pos4>
            {
                new Pos4(400.663f, -1855.337f, 26.3691f, 43.2951f),
                new Pos4(401.5598f, -1842.296f, 26.51275f, 326.6991f),
                new Pos4(406.3914f, -1861.585f, 26.39761f, 223.6282f),
            },
            new List<Pos4>
            {
                new Pos4(412.4977f, -1856.055f, 27.32313f, 256.5159f),
                new Pos4(420.1726f, -1852.638f, 27.47546f, 153.4725f),
                new Pos4(401.9381f, -1851.605f, 26.92291f, 209.1036f),
                new Pos4(392.2663f, -1849.712f, 26.83874f, 262.3087f),
                new Pos4(401.3307f, -1849.148f, 27.31974f, 127.5275f),
            }
        );
        internal static OwnerHouseScenarioTemplate Loc3 = new OwnerHouseScenarioTemplate
        (
            "35 Grove St",
            new List<Pos4>
            {
                new Pos4(-55.72511f, -1785.602f, 27.4556f, 314.3537f),
                new Pos4(-55.40997f, -1785.546f, 27.45461f, 130.7797f),
                new Pos4(-57.32258f, -1796.375f, 27.02382f, 50.14635f),
            },
            new List<Pos4>
            {
                new Pos4(-48.51614f, -1788.576f, 27.87355f, 93.74596f),
                new Pos4(-50.04349f, -1783.559f, 28.3008f, 110.5121f),
                new Pos4(-38.82661f, -1772.129f, 27.8704f, 309.496f),
                new Pos4(-35.03073f, -1765.351f, 27.92273f, 110.5906f),
            }
        );
        internal static OwnerHouseScenarioTemplate Loc4 = new OwnerHouseScenarioTemplate
        (
            "98 Brogue Ave",
            new List<Pos4>
            {
                new Pos4(238.6556f, -1724.263f, 28.53531f, 321.6739f),
                new Pos4(243.5533f, -1740.388f, 28.54437f, 48.16378f),
                new Pos4(245.2255f, -1747.831f, 28.5465f, 227.3422f),
                new Pos4(273.9525f, -1751.624f, 28.74606f, 134.9858f),
            },
            new List<Pos4>
            {
                new Pos4(249.725f, -1731.058f, 29.6688f, 87.26959f),
                new Pos4(246.8988f, -1737.33f, 29.31866f, 17.17284f),
                new Pos4(259.9981f, -1741.496f, 29.3312f, 229.244f),
                new Pos4(268.2767f, -1742.829f, 29.50841f, 205.6877f),
            }
        );

        internal static List<OwnerHouseScenarioTemplate> OwnerHouseScenarioTemplateList = new List<OwnerHouseScenarioTemplate>
        {
            Loc1, Loc2, Loc3, Loc4
        };
    }
}
