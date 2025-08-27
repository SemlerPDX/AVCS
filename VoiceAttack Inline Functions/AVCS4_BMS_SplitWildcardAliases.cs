namespace AVCS4_BMS_SplitWildcardAliases
{
    // AVCS - Split aliases with semicolon and Handle Wildcard Aliases for non-Flight Agencies
    // by SemlerPDX July2025

    using System;
    using System.Collections.Generic;

    /*
    Required Referenced Assemblies V1 and V2:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;
    */

    public class VAInline
    {
        private static readonly HashSet<string> FlightAgency = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };


        public void main()
        {
            var aliases = VA.GetText("~alias") ?? string.Empty;  //  Null check lives outside here, this will always have value, else allow throw
            var agency = VA.GetText("~agency") ?? string.Empty;  //  Null check lives outside here, this will always have value, else allow throw

            var isFlightAgency = FlightAgency.Contains(agency);

            string[] extractedAliases = VA.ExtractPhrases(aliases);
            List<string> preparedAliases = new List<string>();

            List<string> alreadyAddedAliases = new List<string>();
            foreach (var alias in extractedAliases)
            {
                if (string.IsNullOrWhiteSpace(alias) && !alreadyAddedAliases.Contains(alias))
                {
                    continue;
                }
                alreadyAddedAliases.Add(alias);

                // Add a wildcard asterisk prefix to non-flight agency aliases
                var optionalPrefix = isFlightAgency ? "" : "*";
                var preparedAlias = optionalPrefix + alias;
                preparedAliases.Add(preparedAlias);
            }

            var splitAliases = string.Join(";", preparedAliases);
            VA.SetText("~splitAlias", splitAliases);
        }
    }
}
