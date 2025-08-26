namespace AVCS4_BMS_DeleteAliasCommands
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

        private static readonly HashSet<string> FlightAgency = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };

        public void main()
        {
            var agency = VA.GetText("~agency") ?? string.Empty;

            // Null check lives outside here, this will always have value, else allow throw
            var oldCommands = VA.GetText("~alias") ?? string.Empty;
            oldCommands = oldCommands.Replace(",", ";"); // Dynamic command phrases possible
            string[] extractedOldCommands = VA.ExtractPhrases(oldCommands);

            var isFlightAgency = FlightAgency.Contains(agency);
            var wildcardPrefix = isFlightAgency ? "" : "*";

            List<string> commandBuilder = new List<string>();
            foreach (var command in extractedOldCommands)
            {
                string formattedCommand = wildcardPrefix + command;
                commandBuilder.Add(formattedCommand);
            }

            string[] deprecatedCommands = commandBuilder.ToArray();

            var existingAgencyCommands = VA.GetText(AliasCommandsVarPrefix + agency) ?? string.Empty;
            var existingCommands = existingAgencyCommands.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Build a HashSet for deprecated (removal) commands
            HashSet<string> deprecatedSet = new HashSet<string>();
            for (int i = 0; i < deprecatedCommands.Length; i++)
            {
                string val = deprecatedCommands[i].Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    deprecatedSet.Add(val);
                }
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

            var joinedCommands = string.Join(";", finalCommands);


            if (string.IsNullOrWhiteSpace(joinedCommands))
            {
                // Joined empty - exiting and flagging for deletion
                return;
            }

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
