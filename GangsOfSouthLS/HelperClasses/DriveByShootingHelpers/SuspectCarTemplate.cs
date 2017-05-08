using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using System.Drawing;
using GangsOfSouthLS.HelperClasses.CommonUtilities;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    class SuspectCarTemplate
    {
        internal string LicensePlate { get; private set; }
        private Color ARGBColor;
        internal string ColorString { get; private set; }
        internal string VehModel { get; private set; }
        internal string VehClass { get; private set; }
        internal string Address { get; set; }

        private Dictionary<string, Color> ColorDict = new Dictionary<string, Color>
        {
            { "silver", Color.FromArgb(-11447207) },
            { "gray", Color.FromArgb(-14934495) },
            { "black", Color.FromArgb(-16250872) },
            { "light blue", Color.FromArgb(-12688774) },
            { "white", Color.FromArgb(-968896) },
            { "red", Color.FromArgb(-11924982) },
            { "brown", Color.FromArgb(-14542568) }
        };
        private Dictionary<VehicleClass, string> VehicleClassDict = new Dictionary<VehicleClass, string>
        {
            { VehicleClass.Coupe, "coupe" },
            { VehicleClass.SUV, "SUV" },
            { VehicleClass.Van, "van" },
            { VehicleClass.Compact, "compact car" },
            { VehicleClass.Muscle, "muscle car" },
            { VehicleClass.SportClassic, "classic sports car" },
            { VehicleClass.Sport, "sports car" },
            { VehicleClass.Sedan, "sedan" }
        };
        private Dictionary<string, VehicleClass> VehicleModelDict = new Dictionary<string, VehicleClass>
        {
            { "Minivan", VehicleClass.Van },
            { "Phoenix", VehicleClass.Muscle },
            { "Emperor", VehicleClass.Sedan },
            { "SabreGT", VehicleClass.Muscle },
            { "Burrito", VehicleClass.Van },
            { "Fugitive", VehicleClass.Sedan },
            { "Ruiner", VehicleClass.Muscle },
            { "Prairie", VehicleClass.Compact },
            { "Asea", VehicleClass.Sedan },
            { "Blista", VehicleClass.Compact },
            { "Washington", VehicleClass.Sedan },
            { "Asterope", VehicleClass.Sedan },
            { "Cavalcade", VehicleClass.SUV },
            { "Cavalcade2", VehicleClass.SUV },
            { "Baller", VehicleClass.SUV },
            { "Bison", VehicleClass.Van },
            { "Futo", VehicleClass.Sport },
            { "Granger", VehicleClass.SUV },
            { "Stanier", VehicleClass.Sedan },
        };

        internal SuspectCarTemplate()
        {
            LicensePlate = UsefulFunctions.GetRandomLicensePlate();
            Address = "";

            var colorDictElement = ColorDict.RandomElement();
            ColorString = colorDictElement.Key;
            ARGBColor = colorDictElement.Value;

            var modelDictElement = VehicleModelDict.RandomElement();
            VehModel = modelDictElement.Key;
            VehClass = VehicleClassDict[modelDictElement.Value];
        }

        internal Vehicle CreateCar(Pos4 position)
        {
            var veh = position.CreateVehicle(VehModel);
            veh.LicensePlate = LicensePlate;
            veh.PrimaryColor = ARGBColor;
            return veh;
        }
    }
}
