namespace AVCS4_BMS_SaveAliasKeys
{
    // AVCS - Create Alias Key Variables For Save
    // by SemlerPDX July2025
    // -- outputs VA Text Variables for the AVCS Save File System to iterate through and save to user save file

    using System;
    using System.Collections.Generic;

    /*
    Required Referenced Assemblies V1 and V2:
    Microsoft.CSharp.dll;System.dll
    */

    public class VAInline
    {
        private static readonly string KeyVarPrefix = "AVCS_BMS_KEY_";
        private static readonly string SavedVarPrefix = "AVCS_SFS_SAVED_name_";
        private static readonly string SavedValuePrefix = "AVCS_SFS_SAVED_value_";

        private static readonly HashSet<string> FlightAgency = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };


        public void main()
        {
            var aliases = VA.GetText("~alias") ?? string.Empty;  //  Null check lives outside here, this will always have value, else allow throw
            var agency = VA.GetText("~agency") ?? string.Empty;  //  Null check lives outside here, this will always have value, else allow throw

            var command = VA.GetText("~command") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(command))
            {
                VA.WriteToLog("AVCS ERROR:  cannot locate value of '~command' - should not be empty at this point", "pink");
                VA.Profile.Reset();
                return;
            }

            command = command.Replace(" ", "");

            int savedRequests = VA.GetInt("AVCS_SFS_SAVED_requests") ?? 2; // Should be two saved variables waiting at least

            string[] extractedAliases = VA.ExtractPhrases(aliases);

            var isFlightAgency = FlightAgency.Contains(agency);
            var newKeyVarPrefix = isFlightAgency ? KeyVarPrefix + agency + "_" : KeyVarPrefix;

            var keyValue = VA.GetText(newKeyVarPrefix + command) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(keyValue))
            {
                VA.WriteToLog("AVCS ERROR:  cannot locate value of '" + newKeyVarPrefix + command + "' saved key variable", "pink");
                VA.Profile.Reset();
                return;
            }

            List<string> alreadyAdded = new List<string>();
            foreach (var alias in extractedAliases)
            {
                if (string.IsNullOrWhiteSpace(alias) && !alreadyAdded.Contains(alias))
                {
                    continue;
                }

                alreadyAdded.Add(alias);
                savedRequests++;

                var aliasConcat = alias.Replace(" ", "");
                var keyVarName = newKeyVarPrefix + aliasConcat;

                VA.SetText(keyVarName, keyValue);

                var savedVarName = SavedVarPrefix + savedRequests.ToString();
                VA.SetText(savedVarName, keyVarName);

                var savedVarValue = SavedValuePrefix + savedRequests.ToString();
                VA.SetText(savedVarValue, keyValue);
            }

            VA.SetInt("AVCS_SFS_SAVED_requests", savedRequests);
        }
    }
}
