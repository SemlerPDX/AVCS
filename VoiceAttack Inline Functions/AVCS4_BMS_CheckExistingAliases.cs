namespace AVCS4_BMS_CheckExistingAliases
{
    using System;
    using System.Collections.Generic;

    /*
    Required Referenced Assemblies V1 and V2:
    Microsoft.CSharp.dll;System.dll
    */

    public class VAInline
    {
        private static readonly string AllCommandsVarPrefix = "AVCS_BMS_ALL_COMMANDS_";
        private static readonly HashSet<string> FlightAgency = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };

        private List<string> OtherAgencies = new List<string> { "AWACS", "ATC", "TANKER" };

        public void main()
        {
            string baseCommands = string.Empty;
            List<string> allCommandSets = new List<string>();

            var currentAliases = VA.GetText("~currentAliases") ?? string.Empty;
            var agency = VA.GetText("~agency") ?? string.Empty; // Null or Empty check lives outside and well above this inline - will always have value, else allow throw

            var isFlightAgency = FlightAgency.Contains(agency);

            if (!isFlightAgency)
            {
                foreach (var otherAgency in OtherAgencies)
                {
                    var allCommandsVarName = AllCommandsVarPrefix + otherAgency;
                    var newCommands = VA.GetText(allCommandsVarName) ?? string.Empty;
                    allCommandSets.Add(newCommands);
                }

                baseCommands = string.Join(";", allCommandSets);
            }
            else
            {
                var allCommandsVarName = AllCommandsVarPrefix + agency;
                baseCommands = VA.GetText(allCommandsVarName) ?? string.Empty;
            }


            var aliases = VA.GetText("~alias") ?? string.Empty; // Null or Empty check lives just outside and above this inline - will always have value, else allow throw
            string[] extractedAliases = VA.ExtractPhrases(aliases);


            int aliasesLength = extractedAliases.Length;
            if (aliasesLength > 10)
            {
                VA.SetBoolean("~avcs_alias_extreme_count", true);
                VA.SetInt("~avcs_alias_count", aliasesLength);
            }

            foreach (var alias in extractedAliases)
            {
                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                if (currentAliases.Contains(alias) || baseCommands.Contains(alias))
                {
                    VA.SetBoolean("~avcs_alias_exists", true);
                    VA.SetText("~avcs_existing_alias", alias);
                    return;
                }
            }
        }

    }
}
