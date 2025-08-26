namespace AVCS4_BMS_SplitAliasCommand
{
    using System;

    /*
    Required Referenced Assemblies V1 and V2:
    Microsoft.CSharp.dll;System.dll
    */

    public class VAInline
    {

        /// <summary>
        /// Just a tiny helper, update here if other inlines using this are altered
        /// </summary>
        public void main()
        {
            var aliasedCommand = VA.GetText("~aliasedCommand") ?? string.Empty;

            if (!string.IsNullOrEmpty(aliasedCommand) && aliasedCommand.Contains(":"))
            {
                var aliasParts = aliasedCommand.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                VA.SetText("~command", aliasParts[0].Trim());
                VA.SetText("~alias", aliasParts[1].Trim());
            }
        }
    }
}
