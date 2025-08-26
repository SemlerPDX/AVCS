namespace AVCS4_BMS_CheckFalconUpdates
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /*
    Required Referenced Assemblies V1:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll

    Required Referenced Assemblies V2:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;System.Data.dll;System.Security.Cryptography.dll;System.Data.DataSetExtensions.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll
    */

    /// <summary>
    /// AVCS4 Falcon Menu Dat Updates Check - Automatic Radio Commands Update Check based on Falcon game updates
    /// This class is used to check the Menu.dat file from the currently chosen Falcon BMS program for changes from known hash of this file.
    /// by SemlerPDX July2025
    /// https://veterans-gaming.com/avcs
    /// </summary>
    public class VAInline
    {
        public void main()
        {
            VA.SetBoolean("~avcs_menu_dat_updated", null);

            var menuDatPath = VA.GetText("AVCS_BMS_MENU_DAT_PATH") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(menuDatPath))
            {
                VA.WriteToLog("AVCS ERROR - Save path to Falcon BMS Menu.dat file is missing!", "red");
                VA.WriteToLog("AVCS CONCERN - Unable to check for Menu.dat updates to keep commands in sync with game.", "orange");
                VA.WriteToLog("AVCS SOLUTION: Say, \"Select the Falcon Game Folder\", to fix this.", "yellow");
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var oldMenuDatHash = VA.GetText("AVCS_BMS_MENU_DAT_HASH") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(oldMenuDatHash))
            {
                VA.WriteToLog("AVCS ERROR - Known file hash of Falcon BMS Menu.dat file is missing!", "red");
                VA.WriteToLog("AVCS CONCERN - Unable to compare to current file hash of Menu.dat for auto-update systems.", "orange");
                VA.WriteToLog("AVCS SOLUTION: Restart VoiceAttack and AVCS4 BMS to fix this.", "yellow");
                VA.WriteToLog("AVCS SOLUTION 2: Say, \"Select the Falcon Game Folder\", if restart does not work.", "yellow");
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var isDifferent = IsMenuDatFileChanged(menuDatPath, oldMenuDatHash);
            VA.SetBoolean("~avcs_menu_dat_updated", isDifferent);
        }

        private bool IsMenuDatFileChanged(string menuDatPath, string oldHash)
        {
            string newHash = GetFileHash(menuDatPath);
            return !string.Equals(oldHash, newHash, StringComparison.OrdinalIgnoreCase);
        }

        private string GetFileHash(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", ""); // Hex string
            }
        }
    }
}
