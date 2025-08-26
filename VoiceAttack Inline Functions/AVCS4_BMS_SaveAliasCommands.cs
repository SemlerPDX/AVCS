namespace AVCS4_BMS_SaveAliasCommands
{
    using System;
    using System.Collections.Generic;

    /*
    Required Referenced Assemblies V1 and V2:
    Microsoft.CSharp.dll;System.dll
    */

    public class VAInline
    {
        private static readonly string AliasCommandsVarPrefix = "AVCS_BMS_ALIAS_COMMANDS_";
        private static readonly string SavedVarPrefix = "AVCS_SFS_SAVED_name_";
        private static readonly string SavedValuePrefix = "AVCS_SFS_SAVED_value_";

        public void main()
        {
            // Null check lives outside here, this will always have value, else allow throw
            var agency = VA.GetText("~agency") ?? string.Empty;
            var oldCommands = VA.GetText("~oldAlias") ?? string.Empty; // Dynamic Phrases possible
            if (string.IsNullOrEmpty(oldCommands))
            {
                // No old commands to process, return early
                return;
            }

            oldCommands = oldCommands.Replace(",", ";");
            string[] deprecatedCommands = VA.ExtractPhrases(oldCommands);

            var existingAgencyCommands = VA.GetText(AliasCommandsVarPrefix + agency) ?? string.Empty;
            var existingCommands = existingAgencyCommands.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Build a HashSet for deprecated (removal) commands
            HashSet<string> deprecatedSet = new HashSet<string>();
            for (int i = 0; i < deprecatedCommands.Length; i++)
            {
                string val = deprecatedCommands[i].Trim();
                if (!string.IsNullOrEmpty(val))
                    deprecatedSet.Add(val);
            }

            List<string> finalCommands = new List<string>();

            // Extract all aliases from potential dynamic phrase structures into individual commands
            for (int i = 0; i < existingCommands.Length; i++)
            {
                string command = existingCommands[i].Trim();
                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }

                if (deprecatedSet.Contains(command))
                {
                    continue;
                }

                finalCommands.Add(command);
            }

            var newCommands = VA.GetText("~alias") ?? string.Empty; //.Replace(",", ";"); // should be dynamic phrase, unchanged
            string[] editedCommands = VA.ExtractPhrases(newCommands);
            var joinedEditedCommands = string.Join(";", editedCommands);

            finalCommands.Add(joinedEditedCommands);
            var joinedCommands = string.Join(";", finalCommands);

            int savedRequests = VA.GetInt("AVCS_SFS_SAVED_requests") ?? 0;
            savedRequests++;
            VA.SetInt("AVCS_SFS_SAVED_requests", savedRequests);

            VA.SetText(AliasCommandsVarPrefix + agency, joinedCommands);

            var savedVarName = SavedVarPrefix + savedRequests.ToString();
            VA.SetText(savedVarName, AliasCommandsVarPrefix + agency);

            var savedVarValue = SavedValuePrefix + savedRequests.ToString();
            VA.SetText(savedVarValue, joinedCommands);
        }
    }

}
