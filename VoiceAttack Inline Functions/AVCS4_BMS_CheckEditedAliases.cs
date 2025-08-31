namespace AVCS4_BMS_CheckEditedAliases
{
    using System;
    using System.Collections.Generic;

    /*
    Required Referenced Assemblies V1 and V2:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;
    */

    public class VAInline
    {
        private static readonly string AllCommandsVarPrefix = "AVCS_BMS_ALL_COMMANDS_";
        private static readonly HashSet<string> FlightAgency = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };

        private static readonly List<string> OtherAgencies = new List<string> { "AWACS", "ATC", "TANKER" };

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

            var currentAliasSet = BuildHashSet(currentAliases);
            var baseCommandSet = BuildHashSet(baseCommands);

            var aliases = VA.GetText("~alias") ?? string.Empty; // Null or Empty check lives just outside and above this inline - will always have value, else allow throw
            string[] extractedAliases = VA.ExtractPhrases(aliases);

            // This is an edit, so SOME parts of the new extracted aliases may exist in currentAliases
            // Filter extractedAliases to only unique and not in currentAliasSet
            List<string> unique = new List<string>();
            HashSet<string> added = new HashSet<string>();
            for (int i = 0; i < extractedAliases.Length; i++)
            {
                var alias = extractedAliases[i].Trim();
                if (string.IsNullOrEmpty(alias))
                {
                    continue;
                }

                if (!currentAliasSet.Contains(alias) && !added.Contains(alias))
                {
                    unique.Add(alias);
                    added.Add(alias);
                }
            }
            extractedAliases = unique.ToArray();

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

                if (currentAliasSet.Contains(alias) || baseCommandSet.Contains(alias))
                {
                    VA.SetBoolean("~avcs_alias_exists", true);
                    VA.SetText("~avcs_existing_alias", alias);
                    return;
                }
            }
        }

        private static HashSet<string> BuildHashSet(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var arr = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var v in arr)
            {
                string trimmed = v.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    result.Add(trimmed);
                }
            }

            return result;
        }

    }
}
