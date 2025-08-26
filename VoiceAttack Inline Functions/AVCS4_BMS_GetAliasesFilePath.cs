namespace AVCS4_BMS_GetAliasesFilePath
{
    using System;
    using System.IO;

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
            var templatePath = VA.GetText("commref_bms_command_item_template") ?? string.Empty;
            if (string.IsNullOrEmpty(templatePath))
            {
                return;
            }

            var rootPath = Path.GetDirectoryName(templatePath);
            if (rootPath == null)
            {
                return;
            }

            var aliasesFile = Path.Combine(rootPath, "aliases.txt");
            VA.SetText("aliases", aliasesFile);
        }
    }
}
