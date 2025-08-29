namespace AVCS4_BMS_GetMenuData
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Windows.Forms;

    /*
    Required Referenced Assemblies V1:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll

    Required Referenced Assemblies V2:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;System.Data.dll;System.Security.Cryptography.dll;System.ComponentModel.Primitives.dll;System.Drawing.Primitives.dll;System.Windows.Forms.Primitives.dll;System.Data.DataSetExtensions.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll; System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll
    */

    /// <summary>
    /// AVCS4 Falcon Menu Dat Parser - Automatic Radio Command and HTML Reference Guide Generator
    /// This class is used to allow users to select any Falcon BMS program folder to extract configuration data into AVCS4 BMS Keypress Variables.
    /// by SemlerPDX July2025
    /// https://veterans-gaming.com/avcs
    /// </summary>
    public class VAInline
    {
        private static readonly string MenuDatSubfolderPath = @"Data\Art\CkptArt\Menu.dat";

        private static readonly string BlankConfigsMessage = "On first time use, you must select the Falcon BMS program folder for the version you will be using so commands can be generated.  This can be changed anytime in future.";
        private static readonly string ErrorPathMessage = @"You must select the ROOT Falcon BMS program folder such as 'C:\Program Files\Falcon BMS 4.38' and NOT any sub-folders within it.  Please press OK to try again.";

        [STAThread]
        public void main()
        {
            bool isEmptyConfigs = VA.GetBoolean("AVCS_BMS_MENU_DATA_EMPTY") ?? false;

            var rootFalconGamePath = VA.GetText("AVCS_BMS_ROOT_PROGRAM_PATH") ?? string.Empty;
            VA.SetBoolean("~avcs_canRetryGetFolder", false);

            if (isEmptyConfigs)
            {
                ShowMessageBox(BlankConfigsMessage);
            }

            // Use known root folder as starting folder, if any, else just nothing
            rootFalconGamePath = PickFolder(rootFalconGamePath);
            if (rootFalconGamePath == "cancelled")
            {
                if (isEmptyConfigs)
                {
                    VA.SetText("AVCS_BMS_ROOT_PROGRAM_PATH", null);
                    VA.WriteToLog("AVCS ERROR: File path to Falcon BMS Menu.dat file has not been provided.", "red");
                    VA.WriteToLog("AVCS CONCERN: Unable to read Menu.dat and create voice commands in sync with game.", "orange");
                    VA.WriteToLog("AVCS SOLUTION: Restart VoiceAttack and switch to AVCS4 BMS Radios, follow prompts.", "yellow");
                    VA.SetBoolean("AVCS_ERROR", true);
                    return;
                }

                VA.SetText("AVCS_BMS_ROOT_PROGRAM_PATH", "cancelled");
                VA.WriteToLog("AVCS: Say, \"Select the Falcon Game Folder\", to select a new Falcon program folder anytime.", "yellow");
                return;
            }

            var menuDatPath = Path.Combine(rootFalconGamePath, MenuDatSubfolderPath);
            if (!File.Exists(menuDatPath))
            {
                VA.SetBoolean("~avcs_canRetryGetFolder", true);
                VA.WriteToLog("AVCS ERROR: Falcon BMS Menu.dat file cannot be found at '" + menuDatPath + "'", "red");
                VA.WriteToLog("AVCS CONCERN - Unable to read Menu.dat and create voice commands in sync with game.", "orange");
                VA.SetBoolean("AVCS_ERROR", true);
                ShowMessageBox(ErrorPathMessage, true);
                return;
            }

            VA.SetBoolean("AVCS_ERROR", false);
            var menuDatFileHash = GetFileHash(menuDatPath);

            VA.SetText("AVCS_BMS_ROOT_PROGRAM_PATH", rootFalconGamePath);
            VA.SetText("AVCS_BMS_MENU_DAT_PATH", menuDatPath);
            VA.SetText("AVCS_BMS_MENU_DAT_HASH", menuDatFileHash);
        }

        private void ShowMessageBox(string message) { ShowMessageBox(message, false); }
        private void ShowMessageBox(string message, bool isError)
        {
            var icon = isError ? MessageBoxIcon.Error : MessageBoxIcon.Warning;
            var result = MessageBox.Show(
                message,
                " AVCS4 BMS Radios - Automatic Command Generator     ",
                MessageBoxButtons.OK,
                icon
            );
        }
        private string PickFolder(string rootFolder = "")
        {
            using (var owner = new Form())
            using (var dlg = new FolderBrowserDialog())
            {
                owner.ShowInTaskbar = false;
                owner.StartPosition = FormStartPosition.Manual;
                owner.Size = new System.Drawing.Size(0, 0);
                owner.Location = new System.Drawing.Point(-32000, -32000); // way off-screen
                owner.Opacity = 0;
                owner.Show();
                owner.TopMost = true;
                owner.Activate();
                owner.Focus();

                dlg.Description = "Select your Falcon BMS program folder:";
                dlg.RootFolder = Environment.SpecialFolder.MyComputer;

                if (!string.IsNullOrEmpty(rootFolder))
                {
                    dlg.SelectedPath = rootFolder;
                }

                // This brings the dialog to the foreground as topmost
                DialogResult result = dlg.ShowDialog(owner);

                owner.Close();

                if (result == DialogResult.OK)
                {
                    return dlg.SelectedPath;
                }
                else
                {
                    return "cancelled";
                }
            }
        }

        private string GetFileHash(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", ""); // Yoink dash from hex string
            }
        }
    }
}
