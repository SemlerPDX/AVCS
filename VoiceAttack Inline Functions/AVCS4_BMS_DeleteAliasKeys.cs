namespace AVCS4_BMS_DeleteAliasKeys
{
    // AVCS - Mark Deprecated Alias Key Variables For Deletion
    // by SemlerPDX July2025
    // VETERANS-GAMING.COM

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /*
    Required Referenced Assemblies V1 and V2:
    Microsoft.CSharp.dll;System.dll;System.Collections.dll;System.Linq.dll
    */

    public class VAInline
    {
        private static readonly string KeyVarPrefix = "AVCS_BMS_KEY_";
        private static readonly string SavedVarPrefix = "AVCS_SFS_SAVED_name_";

        private static readonly HashSet<string> FlightAgency = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };

        /// <summary>
        /// This inline function processes the aliases and agency information to mark key variables for deletion.<br/>
        /// It may be called from the 'EDIT' logic flow of this Aliases command, or from the 'DELETE' logic flow.<br/>
        /// The job of this function is to set Save File System (SFS) variables that will be used to delete the<br/>
        /// values from the user save file, and nullify the variables in the active session.
        /// </summary>
        public void main()
        {
            var aliases = VA.GetText("~alias") ?? string.Empty;  //  Null check lives outside here, this will always have value, else allow throw
            var agency = VA.GetText("~agency") ?? string.Empty;  //  Null check lives outside here, this will always have value, else allow throw

            List<string> deprecatedAliases;
            var extractedAliases = VA.ExtractPhrases(aliases).ToList();

            // In 'EDIT' logic flow, '~alias' is edit of this, so this will always have value - else is 'DELETE' logic flow
            var oldAliases = VA.GetText("~oldAlias") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(oldAliases))
            {
                deprecatedAliases = extractedAliases;
            }
            else
            {
                oldAliases = oldAliases.Replace(",", ";");
                var extractedOldAliases = VA.ExtractPhrases(oldAliases).ToList();

                deprecatedAliases = extractedOldAliases
                    .Except(extractedAliases, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            var isFlightAgency = FlightAgency.Contains(agency);
            var newKeyVarPrefix = isFlightAgency ? KeyVarPrefix + agency + "_" : KeyVarPrefix;

            int savedRequests = 0;
            foreach (var alias in deprecatedAliases)
            {
                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                savedRequests++;

                var aliasConcat = alias.Replace(" ", "");
                var keyVarName = newKeyVarPrefix + aliasConcat;
                var savedVarName = SavedVarPrefix + savedRequests.ToString();

                VA.SetText(savedVarName, keyVarName);
            }

            VA.SetInt("AVCS_SFS_SAVED_requests", savedRequests);
        }
    }
}
