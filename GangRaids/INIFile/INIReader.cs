using System;
using System.Windows.Forms;
using Rage;

namespace GangRaids.INIFile
{
    internal static class INIReader
    {
        private static InitializationFile IniFile;
        internal static Keys MenuKey;
        internal static Keys MenuModifierKey;
        internal static string UnitName;

        internal static bool LoadINIFile()
        {
            IniFile = new InitializationFile(@"Plugins/LSPDFR/GangRaids.ini");
            if (!IniFile.Exists())
            {
                Game.LogTrivial("Could not find INI File. Make sure you installed Gang Raids correctly. Not loading Gang Raids...");
                Game.DisplaySubtitle("Gang Raids could not find its INI File. Make sure you installed it correctly. Not loading Gang Raids...");
                return false;
            }
            SetMenuKey();
            SetMenuModifierKey();
            SetUnitName();
            return true;
        }

        private static void SetMenuKey()
        {
            var keyString = IniFile.ReadString("KEYBINDS", "MenuKey");
            if (Enum.TryParse(keyString, false, out MenuKey))
            {
                Game.LogTrivial(string.Format("Set MenuKey to {0}", keyString));
            }
            else
            {
                Game.LogTrivial("Couldn't read player specified value for MenuKey, using default value L");
                Game.DisplayNotification("~b~Gang Raids: ~r~Couldn't read player specified value for MenuKey, using default value L!");
                MenuKey = Keys.L;
            }
        }

        private static void SetMenuModifierKey()
        {
            var keyString = IniFile.ReadString("KEYBINDS", "MenuModifierKey");
            if (Enum.TryParse(keyString, false, out MenuModifierKey))
            {
                Game.LogTrivial(string.Format("Set MenuKey to {0}", keyString));
            }
            else
            {
                Game.LogTrivial("Couldn't read player specified value for MenuModifierKey, using default value LControlKey");
                Game.DisplayNotification("~b~Gang Raids: ~r~Couldn't read player specified value for MenuModifierKey, using default value LControlKey!");
                MenuModifierKey = Keys.LControlKey;
            }
        }

        private static void SetUnitName()
        {
            var DivisionString = IniFile.ReadString("PERSONAL", "Division");
            var UnitTypeString = IniFile.ReadString("PERSONAL", "UnitType");
            var BeatString = IniFile.ReadString("PERSONAL", "Beat");
            UnitName = "";

            if (DivisionString.Length == 1)
            {
                UnitName += ("DIV_0" + DivisionString + " ");
            }
            else
            {
                UnitName += ("DIV_" + DivisionString + " ");
            }

            UnitName += (UnitTypeString + " ");

            if (BeatString.Length == 1)
            {
                UnitName += ("BEAT_0" + BeatString);
            }
            else
            {
                UnitName += ("BEAT_" + BeatString);
            }
        }
    }
}
