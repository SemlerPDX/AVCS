namespace AVCS4_BMS_RadioCommandsGenerator
{using static VAStatics;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Windows.Forms;

    /*
    Required Referenced Assemblies V1:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;System.Windows.Forms.dll;

    Required Referenced Assemblies V2:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;System.Collections.dll;System.Linq.dll;System.Linq.Expressions.dll;System.Security.Claims.dll;System.Security.Principal.Windows.dll;System.Windows.Forms.dll;
    */

    /// <summary>
    /// AVCS4 BMS Command Generator - Automatic Radio Command and HTML Reference Guide Generator
    /// This class is used to parse the Menu.dat file from Falcon BMS and extract configuration data into AVCS4 BMS Keypress Variables.
    /// by SemlerPDX July2025
    /// https://veterans-gaming.com/avcs
    /// </summary>
    public class VAInline
    {
        private const string DefaultProfileName = "AVCS4 Falcon BMS Radios (v2.0)";
        private const string MenuDatSubfolderPath = @"Data\Art\CkptArt\Menu.dat";

        private const string PhrasesConfigFileName = "avcs_bms_data_3.cfg";
        private const string KeysConfigFileName = "avcs_bms_data_4.cfg";

        private static readonly HashSet<string> FlightMenus = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };

        private static string _currentProfileName = "";

        // Must set single-threaded apartment, required for win forms folder selection dialog
        [STAThread]
        public void main()
        {
            _currentProfileName = VA.ParseTokens("{PROFILE}") ?? DefaultProfileName;

            var rootFalconGamePath = VA.GetText("AVCS_BMS_ROOT_PROGRAM_PATH") ?? string.Empty; // Path to Falcon BMS root folder
            if (string.IsNullOrWhiteSpace(rootFalconGamePath))
            {
                VA.WriteToLog("AVCS ERROR: AVCS_BMS_ROOT_PROGRAM_PATH is not set or empty.", "red");
                var errorMessage = "AVCS_BMS_ROOT_PROGRAM_PATH is not set or empty.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var di = new DirectoryInfo(rootFalconGamePath);
            var falconGameTitle = di.Name;

            var menuDatPath = Path.Combine(rootFalconGamePath, MenuDatSubfolderPath);
            if (!File.Exists(menuDatPath))
            {
                VA.WriteToLog("AVCS ERROR: Menu.dat not found at '" + menuDatPath + "'", "red");
                VA.WriteToLog("AVCS ERROR: File does not exist or access denied.", "red");
                var errorMessage = "Menu.dat not found at '" + menuDatPath + "' Please ensure the path is correct, access is permitted, and the file exists.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var configFolderPath = VA.ParseTokens(@"{TXT:AVCS_APPS}\AVCS\CORE\BMS\{INT:AVCS_BMS_VER_MAJOR}\update");
            if (!Directory.Exists(configFolderPath))
            {
                VA.WriteToLog("AVCS ERROR: AVCS4 BMS config folder does not exist.", "red");
                var errorMessage = "AVCS4 BMS config folder does not exist.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var keysConfigPath = Path.Combine(configFolderPath, KeysConfigFileName);
            var phrasesConfigPath = Path.Combine(configFolderPath, PhrasesConfigFileName);

            var htmlFolderPath = VA.ParseTokens(@"{TXT:AVCS_APPS}\AVCS\voice_commands");
            var htmlOutFileName = VA.ParseTokens(@"commref_bms{INT:AVCS_BMS_VER_MAJOR}.html");
            var htmlOutPath = Path.Combine(htmlFolderPath, htmlOutFileName);

            var htmlMainTemplate = VA.GetText("~commandReferenceTemplate") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(htmlMainTemplate))
            {
                VA.WriteToLog("AVCS ERROR: AVCS_BMS_COMMREF_TEMPLATE is null or white space.", "red");
                var errorMessage = "AVCS_BMS_COMMREF_TEMPLATE is null or empty.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var htmlBlockTemplate = VA.GetText("~agencyBlockTemplate") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(htmlBlockTemplate))
            {
                VA.WriteToLog("AVCS ERROR: AVCS_BMS_COMMREF_BLOCK_TEMPLATE is null or white space.", "red");
                var errorMessage = "AVCS_BMS_COMMREF_BLOCK_TEMPLATE is null or empty.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var htmlPageTemplate = VA.GetText("~agencyPageTemplate") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(htmlPageTemplate))
            {
                VA.WriteToLog("AVCS ERROR: AVCS_BMS_COMMREF_PAGE_TEMPLATE is null or white space.", "red");
                var errorMessage = "AVCS_BMS_COMMREF_PAGE_TEMPLATE is null or empty.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var htmlItemTemplate = VA.GetText("~commandItemTemplate") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(htmlItemTemplate))
            {
                VA.WriteToLog("AVCS ERROR: AVCS_BMS_COMMREF_ITEM_TEMPLATE is null or white space.", "red");
                var errorMessage = "AVCS_BMS_COMMREF_ITEM_TEMPLATE is null or empty.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var htmlCompoundImage = VA.GetText("AVCS_BMS_COMMREF_COMPOUND_IMG") ?? string.Empty;
            var htmlWildcardImage = VA.GetText("AVCS_BMS_COMMREF_WILDCARD_IMG") ?? string.Empty;

            TemplateData templateData = new TemplateData(
                htmlMainTemplate,
                htmlBlockTemplate,
                htmlPageTemplate,
                htmlItemTemplate,
                htmlCompoundImage,
                htmlWildcardImage
            );


            var aliasesFile = VA.GetText("~commandGlobalAliases") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(aliasesFile))
            {
                VA.WriteToLog("AVCS ERROR: AVCS_BMS_COMMAND_ALIASES is null or white space. No aliases will be built.", "red");
                var errorMessage = "AVCS_BMS_COMMREF_TEMPLATE is null or empty. No aliases will be built, continuing anyway.";
                VA.SetText("AVCS_EX_MSG", errorMessage);
            }

            var parser = new MenuDatParser(VA, aliasesFile);
            ConfigData configData;
            try
            {
                var data = parser.ParseToCfgLines(menuDatPath);

                configData = new ConfigData(
                    data.KeyLines.ToList(),
                    data.CommandLines.ToList(),
                    data.CompoundCommandLines.ToList(),
                    data.AllCommandsLines.ToList(),
                    data.AgencyReferences.ToList()
                );

                if (configData == null)
                {
                    throw new Exception("Failed to parse Menu.dat file - config data is null.");
                }
            }
            catch (Exception ex)
            {
                // Handle any exception via AVCS Debug and error messaging system
                VA.WriteToLog("AVCS ERROR: FalconMenuDatParser exited early with the following message", "red");
                VA.WriteToLog("AVCS ERROR: " + ex.Message, "red");
                VA.SetText("AVCS_EX_MSG", ex.Message);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }



            // The phrase lines containing everything we can possibly say and some things we shouldn't
            var commandLines = string.Join(";", configData.CommandLines.ToList());
            var compoundCommandLines = string.Join(";", configData.CompoundCommandLines.ToList());
            var allCommandsLines = configData.AllCommandsLines.ToList();

            commandLines = "AVCS_BMS_COMMAND_PHRASES=[" + commandLines + "]";
            compoundCommandLines = "AVCS_BMS_COMPOUND_COMMAND_PHRASES=" + compoundCommandLines;
            var phraseLines = new List<string>() { commandLines, compoundCommandLines };

            foreach (var line in allCommandsLines)
            {
                phraseLines.Add(line);
            }


            // The almightly key lines that turn what we say into keypress macros
            var keyLines = configData.KeyLines.ToList();


            // The fancy pants HTML lines of the quick command reference guide which tells us what we can say to enact keypress macros
            var agencyReferences = configData.AgencyReferences;
            var htmlLines = BuildHtmlGuide(agencyReferences, templateData, falconGameTitle, htmlOutPath);



            // Try to write the configuration files and HTML guide to AVCS folder under VoiceAttack Apps directory
            try
            {
                WriteToFile(phrasesConfigPath, phraseLines);
                WriteToFile(keysConfigPath, keyLines);
                WriteToFile(htmlOutPath, htmlLines);
            }
            catch (Exception ex)
            {
                // Handle any exception via AVCS Debug and error messaging system
                VA.WriteToLog("AVCS ERROR: FalconMenuDatParser exited when trying to write files with the following message", "red");
                VA.WriteToLog("AVCS ERROR: " + ex.Message, "red");
                VA.SetText("AVCS_EX_MSG", ex.Message);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            var finalMessage = VA.ParseTokens(FinalMessageTemplate);
            finalMessage = finalMessage.Replace("falconGameTitle", falconGameTitle);

            ShowFinalMessageBox(finalMessage);
            ///... see? That wasn't so hard!  Only took 85 hours to write 1000 lines of code to parse a game file and write some config files! :D
        }


        private static void WriteToFile(string filePath, List<string> lines)
        {
            using (var sw = new StreamWriter(filePath, false, new UTF8Encoding(false)))
            {
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue; // Skip empty lines
                    }

                    sw.WriteLine(line);
                }
            }
        }

        private static List<string> BuildHtmlGuide(List<AgencyReference> agencyReferences, TemplateData templateData, string falconGameTitle, string htmlOutPath)
        {
            string htmlTemplate = templateData.MainTemplate;
            foreach (var agency in agencyReferences)
            {
                string allPagesHtml = "";
                var isFlightAgency = FlightMenus.Contains(agency.AgencyName);

                var imagePath = isFlightAgency
                    ? templateData.CompoundCommandTitleImage
                    : templateData.WildcardCommandTitleImage;

                for (int p = 0; p < agency.Pages.Count; ++p)
                {
                    var page = agency.Pages[p];
                    string commandItemsHtml = "";

                    for (int i = 0; i < page.Commands.Count; ++i)
                    {
                        var cmd = page.Commands[i];

                        // Use semicolon-separator for aliases to match VA style "or"
                        string aliases = (cmd.Aliases != null && cmd.Aliases.Count > 0)
                            ? "(" + string.Join("; ", cmd.Aliases) + ")"
                            : "";

                        string commandItemHtml = templateData.CommandItemTemplate
                            .Replace("INSERTITEMNUM", cmd.ItemNumber.ToString())
                            .Replace("INSERTMAINPHRASE", cmd.MainPhrase)
                            .Replace("INSERTALIASES", aliases);

                        commandItemsHtml += commandItemHtml + "\r\n";
                    }

                    string pageHtml = templateData.AgencyPageTemplate
                        .Replace("INSERTPAGENUM", (p + 1).ToString())
                        .Replace("INSERTPAGETITLE", page.PageName)
                        .Replace("INSERTCOMMANDITEMS", commandItemsHtml)
                        .Replace("DISPLAYBLOCK", p == 0 ? "block" : "none"); // Show first, hide others

                    allPagesHtml += pageHtml + "\r\n";
                }

                string agencyBlockHtml = templateData.AgencyBlockTemplate
                    .Replace("INSERTTITLEIMAGEPATH", imagePath)
                    .Replace("INSERTAGENCY", agency.AgencyName)
                    .Replace("INSERTALLPAGES", allPagesHtml);

                string marker = string.Format("<!-- {0} INSERTAGENCYBLOCK -->", agency.AgencyName.ToUpperInvariant());
                htmlTemplate = htmlTemplate.Replace(marker, agencyBlockHtml);
                htmlTemplate = htmlTemplate.Replace("FALCONGAMETITLE", falconGameTitle);
                htmlTemplate = htmlTemplate.Replace("AVCSPROFILENAME", _currentProfileName);
            }

            return htmlTemplate.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        }

        private static void ShowFinalMessageBox(string finalMessage)
        {
            MessageBox.Show(
                finalMessage,
                " AVCS4 BMS Radios - Automatic Command Generator     ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private const string FinalMessageTemplate = @"Profile will now restart." +
            @"{NEWLINE}{NEWLINE}Radio menu voice commands and a quick command reference" +
            @" guide have been generated for falconGameTitle " +
            @"{NEWLINE}{NEWLINE}To view this guide, say, ""Open the Command Reference""" +
            @"{NEWLINE}{NEWLINE}To create command aliases, say, ""Create a Command Alias""" +
            @"{NEWLINE}{NEWLINE}To change BMS version, say, ""Generate Radio Commands""" +
            @"{NEWLINE}{NEWLINE}{NEWLINE}Thanks for checking out AVCS4 BMS!{NEWLINE}-Sem";
    }

    /// <summary>
    /// Represents the configuration data parsed from the Menu.dat file for AVCS systems in VoiceAttack.
    /// </summary>
    public class ConfigData
    {
        /// <summary>
        /// Contains the key lines for the configuration file.
        /// </summary>
        public IEnumerable<string> KeyLines { get; set; }

        /// <summary>
        /// Contains the command lines for the configuration file.
        /// </summary>
        public IEnumerable<string> CommandLines { get; set; }

        /// <summary>
        /// Contains the compound command lines for the configuration file.
        /// </summary>
        public IEnumerable<string> CompoundCommandLines { get; set; }

        /// <summary>
        /// Contains all command lines, formatted for the configuration file.
        /// </summary>
        public IEnumerable<string> AllCommandsLines { get; set; }

        /// <summary>
        /// Contains the full list of agency/page/item reference objects for HTML export.
        /// </summary>
        public List<AgencyReference> AgencyReferences { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigData"/> class with the specified lines.
        /// </summary>
        /// <param name="keyLines">Key lines for the configuration file.</param>
        /// <param name="commandLines">Command lines for the configuration file.</param>
        /// <param name="compoundCommandLines">Compound command lines for the configuration file.</param>
        /// <param name="allCommandsLines">All command lines (pre-formatted) for the configuration file.</param>
        /// <param name="agencyReferences">List of agency references for HTML export.</param>"
        public ConfigData
        (
            List<string> keyLines = null,
            List<string> commandLines = null,
            List<string> compoundCommandLines = null,
            List<string> allCommandsLines = null,
            List<AgencyReference> agencyReferences = null
        )
        {
            KeyLines = keyLines;
            CommandLines = commandLines;
            CompoundCommandLines = compoundCommandLines;
            AllCommandsLines = allCommandsLines;
            AgencyReferences = agencyReferences;
        }
    }

    /// <summary>
    /// Represents the template data used for dynamically generating the AVCS4 BMS quick command reference guide HTML.
    /// </summary>
    public class TemplateData
    {
        /// <summary>
        /// The main template for the HTML guide, which includes placeholders for agency blocks.
        /// </summary>
        public string MainTemplate { get; set; }

        /// <summary>
        /// The template for the agency block in the HTML guide.
        /// </summary>
        public string AgencyBlockTemplate { get; set; }

        /// <summary>
        /// The template for the agency page in the HTML guide.
        /// </summary>
        public string AgencyPageTemplate { get; set; }

        /// <summary>
        /// The template for the command item in the HTML guide.
        /// </summary>
        public string CommandItemTemplate { get; set; }

        /// <summary>
        /// The image path for the compound command title in the HTML guide.
        /// </summary>
        public string CompoundCommandTitleImage { get; set; }

        /// <summary>
        /// The image path for the wildcard command title in the HTML guide.
        /// </summary>
        public string WildcardCommandTitleImage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateData"/> class with the specified templates.
        /// </summary>
        /// <param name="mainTemplate">The main template for the HTML guide, which includes placeholders for agency blocks.</param>
        /// <param name="agencyBlockTemplate">The template for the agency block in the HTML guide.</param>
        /// <param name="agencyPageTemplate">The template for the agency page in the HTML guide.</param>
        /// <param name="commandItemTemplate">The template for the command item in the HTML guide.</param>
        /// <param name="compoundCommandTitleImage">The image path for the compound command title in the HTML guide.</param>
        /// <param name="wildcardCommandTitleImage">The image path for the wildcard command title in the HTML guide.</param>
        public TemplateData(
            string mainTemplate,
            string agencyBlockTemplate,
            string agencyPageTemplate,
            string commandItemTemplate,
            string compoundCommandTitleImage,
            string wildcardCommandTitleImage
        )
        {
            MainTemplate = mainTemplate;
            AgencyBlockTemplate = agencyBlockTemplate;
            AgencyPageTemplate = agencyPageTemplate;
            CommandItemTemplate = commandItemTemplate;
            CompoundCommandTitleImage = compoundCommandTitleImage;
            WildcardCommandTitleImage = wildcardCommandTitleImage;
        }
    }

    /// <summary>
    /// Represents a reference to an agency in the Menu.dat file, containing its name and associated pages.
    /// </summary>
    public class AgencyReference
    {
        /// <summary>
        /// The name of the agency as defined in the Menu.dat file.
        /// </summary>
        public string AgencyName;

        /// <summary>
        /// Contains a list of pages associated with this agency, each with its own commands.
        /// </summary>
        public List<PageReference> Pages = new List<PageReference>();
    }

    /// <summary>
    /// Represents a reference to a page in the Menu.dat file, containing its name and associated commands.
    /// </summary>
    public class PageReference
    {
        /// <summary>
        /// The name of the page as defined in the Menu.dat file.
        /// </summary>
        public string PageName;

        /// <summary>
        /// Contains a list of commands associated with this page, each with its own item number and aliases.
        /// </summary>
        public List<CommandReference> Commands = new List<CommandReference>();
    }

    /// <summary>
    /// Represents a reference to a command in the Menu.dat file, containing its item number, main phrase, and aliases.
    /// </summary>
    public class CommandReference
    {
        /// <summary>
        /// The item number of the command as defined in the Menu.dat file, with 10 normalized to 0 for keyboard.
        /// </summary>
        public int ItemNumber;

        /// <summary>
        /// The main phrase of the command, which is the primary label used in the configuration.
        /// </summary>
        public string MainPhrase;

        /// <summary>
        /// Contains a list of aliases for the command, which are alternative labels that can be used.
        /// </summary>
        public List<string> Aliases = new List<string>();
    }

    /// <summary>
    /// Parses the Menu.dat file from Falcon BMS and extracts configuration data into AVCS4 BMS Keypress Variables.
    /// </summary>
    public sealed class MenuDatParser
    {

        private readonly HashSet<string> FlightMenus = new HashSet<string> { "WINGMAN", "ELEMENT", "FLIGHT" };

        private readonly Dictionary<string, List<string>> _aliases;

        private readonly List<string> _otherAgencyLabels = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDatParser"/> class.
        /// </summary>
        /// <param name="aliasesFilePath">The file path to the command phrase aliases file.</param>
        public MenuDatParser(dynamic VA, string aliasesFile = null)
        {
            _aliases = LoadAliases(VA, aliasesFile);
        }

        /// <summary>
        /// Parses the Menu.dat file and extracts configuration data into AVCS4 BMS Keypress Variables.
        /// </summary>
        /// <param name="rootFalconGamePath">The root path to the Falcon BMS game folder of any version.</param>
        /// <returns>A <see cref="ConfigData"/> object containing the parsed BMS radio menus and AVCS variables configuration data.</returns>
        public ConfigData ParseToCfgLines(string menuDatPath)
        {
            var outKeyLines = new List<string>();
            var outCommandLabels = new List<string>();
            var outCompoundCommandLabels = new List<string>();

            var outAllCommands = new List<string>();

            Dictionary<string, AgencyReference> htmlDict = new Dictionary<string, AgencyReference>();
            AgencyReference currentAgencyReference = null;
            PageReference currentPageReference = null;

            string currentMenuCommands = string.Empty;
            string currentAgency = null;
            string currentPage = null;
            string currentPageNumber = null;

            int pageIndex = 0; // 1-based under each menu
            int position = 0;  // 1-based under each page

            var menuDat = File.ReadLines(menuDatPath);

            foreach (var raw in menuDat)
            {
                var line = raw.Trim();
                if (!line.StartsWith("#"))
                {
                    continue;
                }

                // ─── MENU ───
                // "#menu <intName> <cat> <num> <num> <color> <AGENCY…>"
                // 0      1          2    3     4     5       6…
                if (line.StartsWith("#menu "))
                {
                    outAllCommands.Add(currentMenuCommands);
                    var menuTokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var isValidMenuLine = (menuTokens.Length >= 7);
                    if (!isValidMenuLine)
                    {
                        continue;
                    }

                    // Grab everything after the color word
                    var display = string.Join(" ", menuTokens.Skip(6));
                    currentAgency = ExtractMenuGroup(display);

                    if (currentAgency.StartsWith("OTHER AGENCIES"))
                    {
                        currentAgency = currentAgency.Replace("OTHER AGENCIES", "TANKER"); // special case for "Other Agencies Commands"
                    }

                    currentMenuCommands = currentAgency + ":";

                    currentAgencyReference = new AgencyReference
                    {
                        AgencyName = currentAgency,
                    };
                    htmlDict[currentAgency] = currentAgencyReference; // Add or replace the current agency

                    pageIndex = 0;
                    currentPage = null;
                    continue;
                }

                if (currentAgency == null)
                {
                    continue;   // haven't hit a #menu yet
                }

                // ─── PAGE ───
                // "#page <id> <skip> <skip> <color> <PAGE…>"
                //   0      1     2      3      4      5…
                if (line.StartsWith("#page "))
                {
                    var pageTokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var isValidPageLine = (pageTokens.Length >= 5);
                    if (!isValidPageLine)
                    {
                        continue;
                    }

                    pageIndex++;

                    // drop pageTokens[0]-[3], keep from [4] onward
                    var page = string.Join(" ", pageTokens.Skip(4));

                    currentPage = page;
                    currentPageNumber = pageIndex.ToString();

                    currentPageReference = new PageReference
                    {
                        PageName = currentPage
                    };
                    currentAgencyReference.Pages.Add(currentPageReference);


                    position = 0;

                    continue;
                }

                // ─── ITEM ───
                // "#item <type> <skip> <skip> <skip> <skip> <PAGE…>"
                //   0      1      2      3      4      5      6…
                var isValidLine = (line.StartsWith("#item ") && currentPage != null);
                if (!isValidLine)
                {
                    // if this is not a valid item line, skip this line
                    continue;
                }

                var itemTokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var isValidTokens = (itemTokens.Length >= 7);
                if (!isValidTokens)
                {
                    // if there are not enough itemTokens, skip this line
                    continue;
                }

                // Join itemTokens[6…] into the raw cleanLabel
                var rawLabel = string.Join(" ", itemTokens.Skip(6));
                var cleanLabel = CleanLabel(rawLabel);

                var isFlightMenu = IsFlightMenu(currentAgency);

                position++;
                var key = position % 10;   // 10th -> 0
                var currentVarNamePrefix = "AVCS_BMS_KEY_" + GetAgencySuffix(currentAgency);
                var keyData = currentPageNumber + "_" + currentAgency + "_" + key;
                var aliasedLabel = currentPage + "_" + cleanLabel;

                List<string> allLabels = new List<string>();
                List<string> validLabels = new List<string>();

                // Check in _aliases if this cleanLabel has alternate format alias(es)
                var isInAltAliases = _aliases.ContainsKey(aliasedLabel);

                cleanLabel = isInAltAliases ? aliasedLabel : cleanLabel;
                if (!_aliases.TryGetValue(cleanLabel, out allLabels))
                {
                    allLabels = new List<string> { cleanLabel }; // if no alias(es), add the cleanLabel itself
                }

                foreach (var label in allLabels)
                {
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue; // skip empty labels
                    }

                    var validLabel = isFlightMenu ? label : GetValidLabel(label, currentPage);

                    validLabels.Add(validLabel);
                    outKeyLines.Add(currentVarNamePrefix + validLabel + "=" + keyData);

                    var altSpeechLabel = ToHumanReadable(validLabel);
                    if (isFlightMenu)
                    {
                        outCompoundCommandLabels.Add(altSpeechLabel);
                    }
                    else
                    {
                        outCommandLabels.Add(altSpeechLabel);
                    }

                    var optionalSemicolon = currentMenuCommands.EndsWith(":") ? string.Empty : ";";
                    currentMenuCommands += optionalSemicolon + altSpeechLabel;
                }

                // Human readable (for HTML display)
                string mainPhrase = ToHumanReadable(validLabels[0]);

                // Build aliases for HTML, filtering out any that (case-insensitive) match the main phrase
                List<string> aliases = validLabels.Select(ToHumanReadable)
                                                .Where(a => !string.Equals(a, mainPhrase, StringComparison.OrdinalIgnoreCase))
                                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                                .ToList();

                var commandReference = new CommandReference
                {
                    ItemNumber = key,
                    MainPhrase = mainPhrase,
                    Aliases = aliases
                };
                currentPageReference.Commands.Add(commandReference);

            }

            outAllCommands.Add(currentMenuCommands);
            var allCommandVariables = GenerateAllCommandVariables(outAllCommands);

            var agencyRefs = htmlDict.Values.ToList();

            ConfigData configData = new ConfigData(
                outKeyLines,
                outCommandLabels,
                outCompoundCommandLabels,
                allCommandVariables,
                agencyRefs
            );

            return configData; /// boom! drops mic! grabs coffee!! picks up mic and inspects for damage cuz dayum I need it for voice commands... why did I drop that thing?! :D
        }


        private bool IsFlightMenu(string currentMenu)
        {
            return FlightMenus.Contains(currentMenu);
        }

        private string GetAgencySuffix(string currentAgency)
        {
            return IsFlightMenu(currentAgency) ? currentAgency + "_" : string.Empty;
        }

        private string GetValidLabel(string label, string page)
        {
            // If this new label already exists on running list, enforce a prefix of currentPage.ToTitleCase() + cleanLabel
            if (_otherAgencyLabels.Contains(label))
            {
                var labelPrefix = GetFirstWordInString(page);

                TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
                labelPrefix = textInfo.ToTitleCase(labelPrefix.ToLower());

                label = labelPrefix + label;
            }

            // Add to running list of labels not belonging to Flight agency
            _otherAgencyLabels.Add(label);

            return label;
        }

        private string GetFirstWordInString(string input)
        {
            // Split by whitespace and take the first word
            var words = input.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length > 0 ? words[0] : string.Empty;
        }

        private string ToHumanReadable(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Convert PascalCase to human-readable format, with handlers for numbers and acronyms
            var sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // If this char is uppercase or a number, and either:
                //  it's not the first char AND previous was lowercase, -OR-
                //  it's not the last char AND next is lowercase,
                // then this is a word boundary.
                var isUpperOrNumber = char.IsUpper(c) || !char.IsLetter(c);
                var isPrevLower = (i > 0 && char.IsLower(input[i - 1]));
                var isNextLower = (i + 1 < input.Length && char.IsLower(input[i + 1]));

                if (isUpperOrNumber && (isPrevLower || isNextLower))
                {
                    sb.Append(' ');
                }

                sb.Append(c);
            }

            // Trim in case there was a space inserted at the very start
            return sb.ToString().Trim();
        }

        private string ExtractMenuGroup(string display)
        {
            // From "ELEMENT COMMANDS" → "ELEMENT", from "AWACS COMMANDS" → "AWACS"
            const string suffix = " COMMANDS";
            if (display.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                display = display.Substring(0, display.Length - suffix.Length);
            }

            return display.ToUpperInvariant();
        }

        private string CleanLabel(string input)
        {
            // Remove everything but letters/digits/spaces, then CamelCase
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }

            var words = sb
                .ToString()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            sb.Clear();
            foreach (var w in words)
            {
                sb.Append(char.ToUpperInvariant(w[0]));
                if (w.Length > 1)
                {
                    sb.Append(w.Substring(1));
                }
            }

            return sb.ToString();
        }


        private List<string> GenerateAllCommandVariables(List<string> commandLines)
        {
            const string prefix = "AVCS_BMS_ALL_COMMANDS_";

            var allCommandVariables = commandLines
                .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains(':'))
                .Select(line =>
                {
                    var parts = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var agencyName = parts[0].Trim();
                    var commandName = parts.Length > 1 ? parts[1].Trim() : "";
                    return prefix + agencyName + "=" + commandName;
                })
                .ToList();

            return allCommandVariables;
        }

        private Dictionary<string, List<string>> LoadAliases(dynamic VA, string textFile)
        {
            // If no alias data, just return empty
            if (string.IsNullOrWhiteSpace(textFile))
            {
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            }

            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            var lines = textFile.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0].Trim();

                VA.SetText(key, null);

                var aliases = !parts[1].Contains(';') ? new List<string> { parts[1] } :
                    parts[1].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(a => a.Trim())
                            .ToList();

                if (!dict.ContainsKey(key))
                {
                    dict[key] = new List<string>();
                }

                dict[key].AddRange(aliases);
            }

            // In case of null (any issue not thrown), just return empty
            return dict ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

    }
}
