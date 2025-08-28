namespace AVCS_CORE_ResourcesUpdater
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Security.Cryptography;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    /*
    Required Referenced Assemblies in VoiceAttack V1:
    Microsoft.CSharp.dll;System.dll;System.Core.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll

    Required Referenced Assemblies in VoiceAttack V2:
    Microsoft.CSharp.dll;System.dll;System.Collections.dll;System.ComponentModel.Primitives.dll;System.ComponentModel.dll;System.Data.DataSetExtensions.dll;System.Diagnostics.Process.dll;System.Drawing.Common.dll;System.Drawing.Primitives.dll;System.Linq.dll;System.Net.Http.dll;System.Private.Xml.Linq.dll;System.Private.Xml.dll;System.Security.Cryptography.dll;System.Windows.Forms.Primitives.dll;System.Windows.Forms.dll
    */

    /// <summary>
    /// A VoiceAttack inline function class for the AVCS CORE updater.<br/>
    /// Responsible for orchestrating resource acquisition and validation through AVCS_CORE_MAIN return variables (the only short, closed-source class in AVCS)
    /// </summary>
    public class VAInline
    {
        /// <summary>
        /// Property indicating that user and AVCS website are online.
        /// </summary>
        public bool AvcsCoreIsOnline { get; private set; }

        /// <summary>
        /// Currently supported default profile shortnames for AVCS CORE updater systems.<br/>
        /// DefaultProfileShortnames = { "CORE20", "BMS20" }
        /// </summary>
        private static readonly string[] DefaultProfileShortnames = { "CORE20", "BMS20" };//, "RON20" };

        /// <summary>
        /// Profile build specific identifier, change along with core release/number strings to define the defaults for this version of AVCS CORE.<br/>
        /// Should be able to provide access to any interested users to invite-only beta testing branches if desired down the road.<br/>
        /// BuildBranches = { "alpha", "beta", "debug", "release" }
        /// </summary>
        private static readonly string[] BuildBranches = { "alpha", "beta", "debug", "release" };

        /// <summary>
        /// DefaultBuildBranch = "release"
        /// </summary>
        private static readonly string DefaultBuildBranch = "release";


        #region Path Strings
        /// <summary>
        /// List of extensions which are used to denote AVCS CORE inline functions.<br/>
        /// These are NOT proper dynamic link libaries for .NET, but compiled inline functions for use by VoiceAttack only.
        /// </summary>
        private static readonly string[] InlineFunctionExtensions = new[] { ".dll" };

        /// <summary>
        /// List of precompiled inline function and HTML template resource extensions.<br/>
        /// These are files which will always be validated against the hashtable because they can "do" things.<br/>
        /// Currently:  [".dll", ".html"]
        /// </summary>
        private static readonly string[] FunctionalExtensions = new[] { ".dll", ".html" };

        /// <summary>
        /// List of AVCS CORE Updater compiled inline functions which cannot be overwritten this session because at least one is in use.<br/>
        /// Currently:  ["AVCS_CORE_ResourcesUpdater_V1.dll", "AVCS_CORE_ResourcesUpdater_V2.dll"]
        /// </summary>
        private static readonly string[] CoreUpdaterFileNames = new[] { "AVCS_CORE_ResourcesUpdater_V1.dll", "AVCS_CORE_ResourcesUpdater_V2.dll" };

        /// <summary>
        /// Fully qualified URL path to secure AVCS CORE online database.<br/>
        /// AvcsCoreUrl = "https://veterans-gaming.com/semlerpdx/avcs/core/"
        /// </summary>
        private static readonly string AvcsCoreUrl = "https://veterans-gaming.com/semlerpdx/avcs/core/";

        /// <summary>
        /// RootDataUrl = "data/"
        /// </summary>
        private static readonly string RootDataUrl = "data/";

        /// <summary>
        /// Base URL for AVCS GitHub repository, used for linking to the AVCS open source codebase and latest changelog.<br/>
        /// </summary>
        private static readonly string AvcsGitHubUrlHome = "https://github.com/SemlerPDX/AVCS";

        /// <summary>
        /// Base URL for AVCS GitHub repository, used for linking to inline function open source code when combined with<br/>
        /// that function name (ending in .cs or .vb).
        /// </summary>
        private static readonly string AvcsGitHubUrlBase = "https://github.com/SemlerPDX/AVCS/blob/master/VoiceAttack%20Inline%20Functions/";


        /// <summary>
        /// Local location of all AVCS CORE folders under AppData\Roaming.<br/>
        /// AvcsApps = "VoiceAttack-AVCS Profiles"
        /// </summary>
        private static readonly string AvcsApps = "VoiceAttack-AVCS Profiles";

        /// <summary>
        /// Local location of all AVCS CORE resources (though sounds will be moved to a sounds folder and ignored).<br/>
        /// AvcsAppsResources = @"AVCS\resources"
        /// </summary>
        private static readonly string AvcsAppsResources = @"AVCS\resources";

        /// <summary>
        /// Local location of all AVCS profile HTML command reference resources (though dynamic HTML templates will be in the <see cref="AvcsAppsResources"/> folder).<br/>
        /// AvcsCommRefResources = @"AVCS\voice_commands"
        /// </summary>
        private static readonly string AvcsCommRefResources = @"AVCS\voice_commands";

        /// <summary>
        /// Local location of all AVCS CORE resources (though sounds will be moved to a sounds folder and ignored).<br/>
        /// AvcsAppsSounds = @"AVCS\sounds"
        /// </summary>
        private static readonly string AvcsAppsSounds = @"AVCS\sounds";

        /// <summary>
        /// AvcsAppsProfilesFile = @"AVCS\avcs_profiles.txt"
        /// </summary>
        private static readonly string AvcsAppsProfilesFile = @"AVCS\avcs_profiles.txt";

        /// <summary>
        /// AvcsAppsNewProfilesFile = @"AVCS\avcs_new_profiles.txt
        /// </summary>
        private static readonly string AvcsAppsNewProfilesFile = @"AVCS\avcs_new_profiles.txt";


        /// <summary>
        /// VersionFileName = "version.core"
        /// </summary>
        private static readonly string VersionFileName = "version.core";

        /// <summary>
        /// HashTableFileName = "hashtable.core"
        /// </summary>
        private static readonly string HashTableFileName = "hashtable.core";

        /// <summary>
        /// ResourceOptionsFileName = "core_resource_options.xml"
        /// </summary>
        private static readonly string ResourceOptionsFileName = "core_resource_options.xml";

        /// <summary>
        /// ResourceFilesSizeFileName = "core_resource_files.core"
        /// </summary>
        private static readonly string ResourceFilesSizeFileName = "core_resource_files.core";

        /// <summary>
        /// Default file name for list of deprecated resources which will not be used, and may be removed from respective resource folder(s).<br/>
        /// DeprecatedResourcesFileName = "deprecated_files.txt"
        /// </summary>
        private static readonly string DeprecatedResourcesFileName = "deprecated_files.txt";
        #endregion Path Strings


        /// <summary>
        /// Fully qualified URL to current branch resources file sizes list.  Example URL path value:<br/>
        /// AvcsResourcesFilesSizeUrl = @"../avcs/core/data/xml/(branch)/core_resource_files.core"
        /// </summary>
        private static string AvcsResourcesFilesSizeUrl { get; set; }

        /// <summary>
        /// Fully qualified path to all AVCS CORE resources.  Example folder path value:<br/>
        /// AvcsResourcesDirectory = @"..\AppData\Roaming\VoiceAttack-AVCS Profiles\AVCS\resources"<br/>
        /// (may be dynamically adjusted based on profile shortname and VA version number)<br/>
        /// i.e. @"..\AppData\Roaming\VoiceAttack-AVCS Profiles\AVCS\resources\CORE20\v2" or "BMS20\v1"<br/>
        /// </summary>
        private static string AvcsResourcesDirectory { get; set; }

        /// <summary>
        /// Fully qualified path to the AVCS CORE resource options XML file.  Example folder path value:<br/>
        /// ResourceOptionsPath = @"..\AppData\Roaming\VoiceAttack-AVCS Profiles\AVCS\resources\core_resource_options.xml"
        /// </summary>
        private static string ResourceOptionsPath { get; set; }

        /// <summary>
        /// Fully qualified path to all AVCS sounds. Due to V1 migration, nested 'avcs' folder is required.  Example folder path value:<br/>
        /// AvcsResourcesDirectory = @"..\AppData\Roaming\VoiceAttack-AVCS Profiles\AVCS\sounds"
        /// </summary>
        private static string AvcsSoundsDirectory { get; set; }

        /// <summary>
        /// Fully qualified path to all AVCS quick command reference HTML resources.  Example folder path value:<br/>
        /// AvcsCommRefDirectory = @"..\AppData\Roaming\VoiceAttack-AVCS Profiles\AVCS\voice_commands"
        /// </summary>
        private static string AvcsCommRefDirectory { get; set; }

        /// <summary>
        /// Current branch property for common use.  Default value:<br/>
        /// CurrentBranch = "release"
        /// </summary>
        private static string CurrentBranch { get; set; }

        /// <summary>
        /// AVCS resources build number property for common use.  Default value:<br/>
        /// AvcsResourcesBuildNumber = "2300"
        /// </summary>
        private static string AvcsResourcesBuildNumber { get; set; }


        // Parsing helpers - ez editing and/or more efficient for loops, and avoids char[] allocations for each call
        private const string LineMarkerDashHeader = "-";
        private static readonly char[] NewLineChars = new[] { '\n', '\r' };
        private static readonly char[] KeyValDelimiter = new[] { '=' };


        // Changelog Variables
        private static bool _hasDeprecatedResources = false;
        private static readonly List<string> DeprecatedResources = new List<string>();

        private static readonly string ChangelogPrefixAdded = "Added - ";
        private static readonly string ChangelogPrefixRemoved = "Removed - ";
        private static readonly string ChangelogPrefixUpdated = "Updated - ";

        private static bool _isLinkLimitMsgAdded = false;

        private static readonly int MaxLinkLabels = 3;
        private static readonly int MaxTotalLabels = 15;

        /// <summary>
        /// DEV NOTE:  This list must be adjusted as old VB.NET inlines are refactored into C# over time.<br/><br/>
        /// List of AVCS inline function file names which have ".vb" extensions on my GitHub for their open source uncompiled form.<br/>
        /// When presenting link labels for a changelog, instead of ".dll" being swapped for ".cs", these require ".vb" instead.
        /// </summary>
        private static readonly string[] VisualBasicFileNames = new[]
        {
            "AVCS4_BMS_LocalizeDecimalsText_V1.dll",
            "AVCS4_BMS_LocalizeDecimalsText_V2.dll",
            "AVCS_CORE_QccPttGetButton_V1.dll",
            "AVCS_CORE_QccPttGetButton_V2.dll"
        };


        // AVCS CORE Updater Requests Variables
        private const string LocalHashTableRequest = "Get Local Hash Table";
        private const string CheckForUpdateRequest = "Check for Update";
        private const string ApplyUpdateRequest = "Apply Update";

        private const string AvcsCoreMainFunction = "F_CORE_MAIN";

        [STAThread]
        public void main()
        {
            bool canInitialize = true;

            try
            {
                var isError = VA.GetBoolean("AVCS_ERROR") ?? false;
                if (isError)
                {
                    throw new Exception("AVCS ERROR: When an error has already occurred, AVCS cannot reinitialize until cleared.");
                }

                AvcsCoreIsOnline = VA.GetBoolean("AVCS_CORE_ONLINE") ?? false;

                canInitialize = AvcsResourcesInitialization();
            }
            catch
            {
                canInitialize = false;
                VA.WriteToLog("AVCS ERROR: Unable to complete initialization of AVCS CORE.", "red");
            }
            finally
            {
                VA.SetText("AVCS_CHECKSUMS", null);
                VA.SetText("AVCS_CHANGELOG", null);
                VA.SetBoolean("AVCS_UPDATE_PENDING", null);

                if (!canInitialize)
                {
                    VA.SetBoolean("AVCS_ERROR", true);
                    VA.SetBoolean("AVCS_MUST_RESTART", true);

                    VA.WriteToLog("AVCS SOLUTIONS NOTE: AVCS CORE will never require VoiceAttack to run 'as admin' and this will not be the solution.", "green");
                    VA.WriteToLog("AVCS SOLUTION 1: Restart VoiceAttack and reload AVCS CORE and any profile(s) used.", "yellow");
                    VA.WriteToLog("AVCS SOLUTION 1b: Verify internet connection, switch to AVCS CORE, and only if nothing happens, say, 'Re-initialize Profile'.", "yellow");
                    VA.WriteToLog("AVCS SOLUTION 2: If this issue occurred when you used a voice command:", "orange");
                    VA.WriteToLog("-say, 'Create a Bug Report', and follow prompts to say the bugged voice command and get a detailed report.", "orange");
                    VA.WriteToLog("AVCS SOLUTION 3: If this issue occurred when simply loading the profile, and AVCS cannot load at all, contact SemlerPDX.", "red");
                }
            }
        }


        /// <summary>
        /// Initializes the AVCS resources directories and files, checks for existing resources, and<br/>
        /// ensures they are all valid to the hashtable, or prompts the user to download updated resources.<br/><br/>
        ///  - If the user is a first time user, it will present a quick resource selection dialog and download those latest resources.<br/>
        ///  - If the user is a returning user, it will validate existing resources and check for updates.<br/>
        ///  - If an update is available, it will prompt the user to approve the update.<br/>
        ///  - If new profiles have been discovered for returning users, they will be added to the resources list for this and future update checks.<br/>
        ///  - If deprecated resources are found, it will write a list of those resources to file, then notify the user and offer cleanup.<br/>
        /// <br/>
        /// [ Placeholders for the not yet implemented Ready or Not profile (RON20) may remain until port into VoiceAttack V2 is completed.<br/>
        /// Updater itself may be refactored for more generic profile addressing, this was a bit rushed tbh... SemlerPDX July/Aug2025 ]
        /// </summary>
        /// <returns>A boolean indicating whether AVCS CORE resources are present and profiles are ready to initialize.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during AVCS resources initialization or validation along with relevant message.</exception>
        private bool AvcsResourcesInitialization()
        {
            var vaVersionPath = "v" + VA.VAVersion.Major.ToString();
            var avcsApps = GetAppConfigPath(AvcsApps); // Required to not run VoiceAttack 'as administrator'
            if (string.IsNullOrWhiteSpace(avcsApps) || !Directory.Exists(avcsApps))
            {
                throw new Exception("AVCS ERROR: Required root AVCS path is somehow not set.");
            }

            AvcsResourcesDirectory = Path.Combine(avcsApps, AvcsAppsResources);
            AvcsSoundsDirectory = Path.Combine(avcsApps, AvcsAppsSounds);
            AvcsCommRefDirectory = Path.Combine(avcsApps, AvcsCommRefResources);

            var avcsFolders = new string[]
            {
                AvcsResourcesDirectory,
                AvcsSoundsDirectory,
                AvcsCommRefDirectory
            };


            // Create the AVCS resource folders (only if does not exist)
            foreach (var folder in avcsFolders)
            {
                if (!TryCreateResourceFolder(folder))
                {
                    // Unable to create the required AVCS apps folder
                    VA.WriteToLog("AVCS ERROR: Unable to create the required AVCS folder for resource files at the following path:", "red");
                    VA.WriteToLog("'" + folder + "'", "blank");
                    throw new Exception("AVCS ERROR: Unable to create the required AVCS folder for resource files.");
                }
            }

            // Branch Override - default will always be 'release' when not set
            CurrentBranch = VA.GetText("AVCS_CORE_BRANCH") ?? DefaultBuildBranch;
            if (string.IsNullOrWhiteSpace(CurrentBranch) || !BuildBranches.Contains(CurrentBranch.Trim().ToLowerInvariant()))
            {
                throw new Exception("AVCS ERROR: Invalid branch name.");
            }

            CurrentBranch = CurrentBranch.Trim().ToLowerInvariant();

            // Need to read profiles XML
            ResourceOptionsPath = Path.Combine(AvcsResourcesDirectory, ResourceOptionsFileName);

            // Need to create string array of various folder paths to resources
            var resourceFolders = new List<string>();
            var profilesToCheck = new List<string>();
            bool hasEnabledV1 = false;
            bool hasEnabledV2 = false;
            bool hasBms = false;
            //bool hasRon = false;

            // Read XML
            var xmlRoot = TryReadXmlFile(ResourceOptionsPath);

            bool hasResourcesFile = xmlRoot != null && !IsEqual(xmlRoot.Name.LocalName, "empty");
            //bool hasResourcesFile = xmlRoot != null;
            bool isVersion2 = (VA.VAVersion.Major == 2);

            var coreShortname = DefaultProfileShortnames[0];
            var bmsShortname = DefaultProfileShortnames[1];
            //var ronShortname = DefaultProfileShortnames[2];

            // --- Core options ---
            var coreOptions = xmlRoot != null && hasResourcesFile ? TryGetCoreOptionsXml(xmlRoot) : Tuple.Create(false, false); // defaults

            hasEnabledV1 = coreOptions.Item1;
            hasEnabledV2 = coreOptions.Item2;

            // --- Profiles ---
            var enabledProfiles = xmlRoot != null && hasResourcesFile ? TryGetProfileListXml(xmlRoot) : new List<string>(); // returns a List<string> like ["BMS20", "RON20"]
            hasBms = enabledProfiles.Contains(bmsShortname);
            //hasRon = enabledProfiles.Contains(ronShortname);


            // Add the required initial core profile path and any saved profile paths to the resource folders list (only for this VA Version)
            AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, coreShortname, !isVersion2, isVersion2);
            profilesToCheck.Add(coreShortname);

            if (hasBms)
            {
                AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, bmsShortname, !isVersion2, isVersion2);
                profilesToCheck.Add(bmsShortname);
            }

            //if (hasRon)
            //{
            //    AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, ronShortname, !isVersion2, isVersion2);
            //    profilesToCheck.Add(ronShortname);
            //}

            // If this is a first time use, present the first time use selection dialog and set any/all resource folders for first use download
            if (!hasResourcesFile)
            {
                AvcsResourcesFilesSizeUrl = AvcsCoreUrl + RootDataUrl + "xml/" + CurrentBranch + "/" + ResourceFilesSizeFileName;

                var choices = ShowVersionGameSelectionDialog(isVersion2, hasEnabledV1, hasBms);//, hasRon);
                hasEnabledV1 = choices.Item1;
                hasEnabledV2 = choices.Item2;
                hasBms = choices.Item3;
                //hasRon = choices.Item4;

                // --- Save Core Options ---
                //xmlRoot = xmlRoot != null && hasResourcesFile ? TrySetCoreOptionsXml(xmlRoot, hasEnabledV1, hasEnabledV2) : new XElement("empty");
                var newRoot = new XElement("resourceOptions"); // ensure a proper root on first run
                xmlRoot = TrySetCoreOptionsXml(newRoot, hasEnabledV1, hasEnabledV2);

                // Add any new paths to the resource folders list based on user selection (if any, and if not already on list)
                AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, coreShortname, hasEnabledV1, hasEnabledV2);

                // --- Save profiles ---
                var toEnable = new List<string>();
                if (hasBms)
                {
                    toEnable.Add(bmsShortname);
                    AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, bmsShortname, hasEnabledV1, hasEnabledV2);
                    profilesToCheck.Add(bmsShortname);
                }
                //if (hasRon)
                //{
                //    toEnable.Add(ronShortname);
                //    AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, ronShortname, hasEnabledV1, hasEnabledV2);
                //    profilesToCheck.Add(ronShortname);
                //}

                xmlRoot = TrySetProfileListXml(xmlRoot, toEnable);

                // Save updated XML
                bool isWritten = TryWriteXmlFile(ResourceOptionsPath, xmlRoot);
            }
            else
            {
                // New and Existing Profile Adaptation for Resource Checks
                var avcsProfilesFile = Path.Combine(avcsApps, AvcsAppsProfilesFile);
                var avcsNewProfilesFile = Path.Combine(avcsApps, AvcsAppsNewProfilesFile);

                var profilesContent = TryReadProfilesFile(avcsProfilesFile);
                var newProfilesContent = TryReadProfilesFile(avcsNewProfilesFile);

                var hasNewBms = !hasBms && (
                    profilesContent.ToUpperInvariant().Contains(bmsShortname.ToUpperInvariant())
                    || newProfilesContent.ToUpperInvariant().Contains(bmsShortname.ToUpperInvariant())
                );

                //var hasNewRon = !hasRon && (
                //    profilesContent.ToUpperInvariant().Contains(ronShortname.ToUpperInvariant())
                //    || newProfilesContent.ToUpperInvariant().Contains(ronShortname.ToUpperInvariant())
                //);

                if (hasNewBms)// || hasNewRon)
                {
                    // --- Save Core Options ---
                    xmlRoot = xmlRoot != null && hasResourcesFile ? TrySetCoreOptionsXml(xmlRoot, hasEnabledV1, hasEnabledV2) : new XElement("empty");


                    // --- Save and Add profile(s) ---
                    var toEnable = new List<string>();
                    if (hasNewBms)
                    {
                        toEnable.Add(bmsShortname);

                        AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, bmsShortname, !isVersion2, isVersion2);
                        profilesToCheck.Add(bmsShortname);
                    }

                    //if (hasNewRon)
                    //{
                    //    toEnable.Add(ronShortname);

                    //    AddRelativeProfilePaths(resourceFolders, AvcsResourcesDirectory, ronShortname, !isVersion2, isVersion2);
                    //    profilesToCheck.Add(ronShortname);
                    //}

                    xmlRoot = TrySetProfileListXml(xmlRoot, toEnable);

                    // Save updated XML
                    TryWriteXmlFile(ResourceOptionsPath, xmlRoot);
                }
            }

            // Create the AVCS profile specific resource folders
            foreach (var folder in resourceFolders)
            {
                if (!TryCreateResourceFolder(folder))
                {
                    // Unable to create the required AVCS apps folder
                    VA.WriteToLog("AVCS ERROR: Unable to create the required AVCS folder for resource files at the following path:", "red");
                    VA.WriteToLog("'" + folder + "'", "blank");
                    throw new Exception("AVCS ERROR: Unable to create the required AVCS folder for resource files.");
                }
            }

            if (!File.Exists(ResourceOptionsPath))
            {
                VA.WriteToLog("AVCS ERROR: Required AVCS resource options file does not exist at the following path:", "red");
                VA.WriteToLog("'" + ResourceOptionsPath + "'", "blank");
                throw new Exception("AVCS ERROR: Required AVCS resource options file does not exist.");
            }

            // This will validate resources (if any), check for updates, and return true if is new/returning user with valid resources, or
            // false if not all resources are valid and was unable to repair or get required resources (such as user or AVCS website offline).
            bool isReadyToInit = true;
            foreach (var profileShortname in profilesToCheck)
            {
                if (string.IsNullOrEmpty(profileShortname) || !isReadyToInit)
                {
                    continue; // skip to next profile on any unlikely empty profile shortnames, or to end on any failure 
                }

                AvcsResourcesDirectory = Path.Combine(avcsApps, AvcsAppsResources, profileShortname, vaVersionPath);
                isReadyToInit = AvcsCoreCheckResources(profileShortname);
            }

            return isReadyToInit;
        }


        // ----- Helpers to Help with the things wot the methods above need Help wit -----
        /// <summary>
        /// Checks if the AVCS CORE resources are valid for the specified profile.<br/>
        /// </summary>
        /// <param name="shortName">The profile short name, an amalgam of the all-caps profile keyword name, and its major and minor version numbers (i.e. "CORE20").</param>
        /// <returns>True if resources are valid and up-to-date and AVCS CORE can initialize, false otherwise.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during resource validation or update checks.</exception>
        private bool AvcsCoreCheckResources(string profileShortname)
        {
            // Validate existing unencrypted resource files (if any)
            bool isReturningUser = HasAllRequiredResources(profileShortname);

            if (AvcsCoreIsOnline)
            {
                // Execute the main AVCS CORE command to get the latest hash table if user and AVCS website are both online
                // Next, if online, will return true if the changelog is different from what is on file locally
                // If different, the contents of the new changelog will be returned for a user update approval message
                // If different (including no local file), and is first time use, will get the latest resources so AVCS CORE can initialize for the first time
                VA.SetText("~passed1", CheckForUpdateRequest);
                VA.SetText("~passed2", profileShortname);
                VA.SetText("~passed3", CurrentBranch);
                VA.Command.Execute(AvcsCoreMainFunction, WaitForReturn: true, AsSubcommand: true, PassedText: "~passed1;~passed2;~passed3");

                var isError = VA.GetBoolean("AVCS_ERROR") ?? false;
                if (isError)
                {
                    throw new Exception("AVCS ERROR: Unable to check for updates or validate resources for profile '" + profileShortname + "'.");
                }
            }

            // Set the global build number variable here after every 'first' call to AVCS_CORE_MAIN, as it may change with each profile shortname
            AvcsResourcesBuildNumber = VA.GetText("AVCS_BUILD") ?? ""; // non-crucial, if it dies, it dies

            // If not online, bypass any update checks and return
            if (!AvcsCoreIsOnline)
            {
                if (!isReturningUser)
                {
                    VA.WriteToLog("AVCS ERROR: No resources found, and user or AVCS website is offline - cannot initialize AVCS CORE.", "red");
                }

                return isReturningUser;
            }

            var isUpdatePending = VA.GetBoolean("AVCS_UPDATE_PENDING") ?? false;
            if (isReturningUser && !isUpdatePending)
            {
                return true; // no updates, continue initialization
            }

            var latestChangeLog = VA.GetText("AVCS_CHANGELOG") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(latestChangeLog))
            {
                VA.WriteToLog("AVCS ERROR: Update pending but change log is somehow null/empty, or contains no entries.", "red");
                return isReturningUser;
            }

            if (isReturningUser && !IsUpdateApprovedByUser(latestChangeLog))
            {
                // If not first time use and update is not approved, just continue initialization
                return true; // all good, user can update another time
            }

            return GetUpdatedResourceFiles(profileShortname);
        }

        /// <summary>
        /// Checks if the AVCS resources folder has all required resources for the specified profile.<br/>
        /// </summary>
        /// <param name="shortName">The profile short name, an amalgam of the all-caps profile keyword name, and its major and minor version numbers (i.e. "CORE20").</param>
        /// <returns>True if resources folder for this profile has all valid required resource files, false otherwise.</returns>
        private bool HasAllRequiredResources(string profileShortname)
        {
            // Check local files for bare-minimum check of this being a returning user - if no files, no resources to validate
            var versionFilePath = Path.Combine(AvcsResourcesDirectory, VersionFileName);
            var hashTableFilePath = Path.Combine(AvcsResourcesDirectory, HashTableFileName);
            var hasVersionFiles = File.Exists(versionFilePath) && File.Exists(hashTableFilePath);

            // Validate all functional resource files in the AVCS resources folder
            return hasVersionFiles && HasValidResourceFiles(profileShortname);
        }

        /// <summary>
        /// Checks if the AVCS resources folder contains valid resource files for the specified profile.<br/>
        /// Also ensures valid paths have been set for each compiled inline function to a VA text variable named for<br/>
        /// the inline function (minus extension and version suffix).<br/>
        /// </summary>
        /// <param name="shortName">The profile short name, an amalgam of the all-caps profile keyword name, and its major and minor version numbers (i.e. "CORE20").</param>
        /// <returns>True if all required resource files are valid, false otherwise.</returns>
        private bool HasValidResourceFiles(string profileShortname)
        {
            bool isAllValid = true;
            List<string> invalidFilePaths = new List<string>();

            if (!Directory.Exists(AvcsResourcesDirectory))
            {
                VA.WriteToLog("AVCS ERROR: AVCS resources directory does not exist.", "red");
                return false;
            }

            try
            {
                // Execute the main AVCS CORE command to get the local hash table
                VA.SetText("~passed1", LocalHashTableRequest);
                VA.SetText("~passed2", profileShortname);
                VA.SetText("~passed3", CurrentBranch);
                VA.Command.Execute(AvcsCoreMainFunction, WaitForReturn: true, AsSubcommand: true, PassedText: "~passed1;~passed2;~passed3");

                var isError = VA.GetBoolean("AVCS_ERROR") ?? false;
                if (isError)
                {
                    throw new Exception("AVCS ERROR: Unable to check for updates or validate resources for profile '" + profileShortname + "'.");
                }

                var localHashTable = VA.GetText("AVCS_CHECKSUMS") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(localHashTable) || !localHashTable.Contains(KeyValDelimiter[0]))
                {
                    throw new Exception("AVCS ERROR: Hash table is somehow null/empty, or contains no entries.");
                }

                // Builds a list of files from the latest hash table and validate each functional resource in the AVCS resources folder
                string[] fileNames = GetFileListFromHashTable(localHashTable);

                var parsedHashTable = GetParsedHashTable(localHashTable) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (parsedHashTable.Count == 0)
                {
                    throw new Exception("AVCS ERROR: Parsed hash table dictionary is somehow empty.");
                }

                foreach (var fileName in fileNames)
                {
                    // Presently, the only AVCS resource files which can "do" things are precompiled inline functions and a small pagination JS within commref HTML template(s)
                    // Therefore they should be verified as true to the open source code on my GitHub via the hash table for safety & peace of mind
                    var fileExt = Path.GetExtension(fileName);
                    if (!IsEqual(fileExt, FunctionalExtensions))
                    {
                        continue;
                    }

                    // Determine if this file is in the resources folder or the command reference folder
                    var filePath = IsEqual(fileExt, InlineFunctionExtensions)
                        ? Path.Combine(AvcsResourcesDirectory, fileName)
                        : Path.Combine(AvcsCommRefDirectory, fileName);

                    if (!IsValidResourceFile(filePath, parsedHashTable))
                    {
                        invalidFilePaths.Add(filePath);
                        isAllValid = false;
                        continue; // let it slide and set isAllInvaid false, continue checking all files
                    }

                    // CRUCIAL: Set each HTML template and compiled inline function path to a VA text variable of its name (minus extension and any version suffix)
                    if (IsEqual(fileExt, FunctionalExtensions) && !IsEqual(fileName, CoreUpdaterFileNames))
                    {
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName).Replace("_V1", "").Replace("_V2", "");
                        VA.SetText(fileNameWithoutExt, filePath);
                        // To execute AVCS compiled inline functions, pass in the name of the function in ~passedtext1 to F_CORE_RUN
                        // - F_CORE_RUN will assemble path from {TXT:{TXT:~passedtext1}}, using passed params to wait and/or retain instance
                    }
                }
            }
            catch (Exception ex)
            {
                isAllValid = false;
                VA.WriteToLog(ex.Message, "red");
                VA.WriteToLog("AVCS SOLUTION: Try restarting VoiceAttack and switch to AVCS CORE. You do NOT need to run VoiceAttack as admin for AVCS CORE.", "red");
            }

            // If not all valid, attempt to delete the invalid file to get a new valid one in its place
            if (!isAllValid && invalidFilePaths.Count > 0)
            {
                VA.WriteToLog("AVCS UPDATER: Invalid or outdated AVCS resource file(s) removed:", "red");
                foreach (var invalidFilePath in invalidFilePaths)
                {
                    // If the file is not valid, it must be deleted so it cannot be utilized and so a new one can be downloaded to repair it
                    if (File.Exists(invalidFilePath) && TryDeleteResourceFile(invalidFilePath))
                    {
                        VA.WriteToLog(invalidFilePath, "blank");
                    }
                }
            }


            // If there are deprecated resources, ask user if they want AVCS to clean them up
            if (_hasDeprecatedResources && IsResourceCleanupApprovedByUser())
            {
                VA.WriteToLog("AVCS NOTE: Cleaning up deprecated resources....", "green");

                var color = "green";
                foreach (var resource in DeprecatedResources)
                {
                    // If the file is deprecated, it should not occupy space on users drive - delete upon approval
                    var filePath = Path.Combine(AvcsResourcesDirectory, resource);
                    if (File.Exists(filePath) && !TryDeleteResourceFile(filePath))
                    {
                        color = "red";
                        VA.WriteToLog("AVCS ERROR: Failed to delete deprecated resource file: " + resource, "red");
                    }
                }

                VA.WriteToLog("AVCS NOTE: Deprecated resources cleanup has concluded...", color);
                _hasDeprecatedResources = false;
                DeprecatedResources.Clear();
            }

            return isAllValid;
        }

        /// <summary>
        /// Builds and adds the profile resource files root paths to the supplied list of resource folders (if not already on it) relative to the<br/>
        /// profile name and version amalgam ('shortname'), and the current VA versions enabled.
        /// </summary>
        /// <param name="profilesList">The list of profile paths to add to.</param>
        /// <param name="rootDir">The root directory where all resource files are located.</param>
        /// <param name="shortName">The profile short name, an amalgam of the all-caps profile keyword name, and its major and minor version numbers (i.e. "CORE20").</param>
        /// <param name="hasVersion1">A boolean indicating if the profile has a version 1 resources path.</param>
        /// <param name="hasVersion2">A boolean indicating if the profile has a version 2 resources path.</param>
        private void AddRelativeProfilePaths(List<string> profilesList, string rootDir, string shortName, bool hasVersion1, bool hasVersion2)
        {
            var isVersion2 = VA.VAVersion.Major == 2;
            var addAltPaths = (isVersion2 && hasVersion1) || (!isVersion2 && hasVersion2);

            var vaVersionPath = "v" + (isVersion2 ? "2" : "1");
            var otherVaVersionPath = "v" + (isVersion2 ? "1" : "2");

            var profilePath = Path.Combine(rootDir, shortName, vaVersionPath);
            var profileAltPath = Path.Combine(rootDir, shortName, otherVaVersionPath);

            if (profilesList.Count > 0 && !profilesList.Contains(profilePath, StringComparer.OrdinalIgnoreCase))
            {
                profilesList.Add(profilePath);
            }

            if (addAltPaths && !profilesList.Contains(profileAltPath, StringComparer.OrdinalIgnoreCase))
            {
                profilesList.Add(profileAltPath);
            }
        }


        // ----- Helpers to Help with the things wot the Helper methods above themselves need Help wit -----
        private bool IsEqual(string input, string comparator) { return IsEqual(input, new[] { comparator }); }
        private bool IsEqual(string input, string[] comparators)
        {
            foreach (var comparator in comparators)
            {
                if (string.Equals(input.ToUpperInvariant(), comparator.ToUpperInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsFunctionalLink(string line, string[] exts)
        {
            return exts.Any(ext => line.ToLowerInvariant().Trim().EndsWith(ext.ToLowerInvariant()));
        }

        private bool IsVisualBasicSource(string dllFileName, string[] vbFiles)
        {
            // Get the base file name (without extension), case-insensitive
            var baseName = System.IO.Path.GetFileNameWithoutExtension(dllFileName);
            foreach (var vbFile in vbFiles)
            {
                var vbBase = System.IO.Path.GetFileNameWithoutExtension(vbFile);
                if (IsEqual(baseName, vbBase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsValidResourceFile(string resourceFilePath, Dictionary<string, string> hashDict)
        {
            try
            {
                if (hashDict.Count == 0)
                {
                    return false; // Invalid hash table
                }

                if (string.IsNullOrWhiteSpace(resourceFilePath) || !File.Exists(resourceFilePath))
                {
                    return false; // Invalid path or non-existent
                }

                string expectedHash;
                var resourceFileName = Path.GetFileName(resourceFilePath);
                if (!hashDict.TryGetValue(resourceFileName, out expectedHash))
                {
                    // Unknown file, treat as valid yet deprecated and due for removal in upcoming build update
                    _hasDeprecatedResources = true;
                    DeprecatedResources.Add(resourceFileName); // rather than attempt to delete, ask user?
                    return true;
                }

                // Must provide a literal string as a non-comparable string for IsEqual on expectedHash (currentHash could be string.Empty on error)
                expectedHash = string.IsNullOrWhiteSpace(expectedHash) ? "null" : expectedHash; // the word "null" or anything is sufficient

                var currentHash = GetFileHash(resourceFilePath);
                if (!IsEqual(currentHash, expectedHash))
                {
                    return false; // Invalid hash - does not match
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsUpdateApprovedByUser(string changelog)
        {
            if (!string.IsNullOrEmpty(changelog))
            {
                changelog = string.Join("\n", changelog.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()));
            }

            var result = ShowUpdateApprovalDialog(changelog);

            return result == DialogResult.Yes;
        }

        private bool IsResourceCleanupApprovedByUser()
        {
            // guard and early return on DeprecatedResources having no files as lines...
            if (DeprecatedResources.Count == 0)
            {
                VA.WriteToLog("AVCS ERROR: No deprecated resources found to remove - should not be possible at this stage.", "red");
                return false;
            }
            var isMultipleFiles = DeprecatedResources.Count > 1;
            var pluralSuffix = isMultipleFiles ? "s" : string.Empty;
            var pluralDeterminer = isMultipleFiles ? " some" : " a";
            var pluralPronoun = isMultipleFiles ? " they" : " it";
            var deprecatedResources = string.Join("\n", DeprecatedResources.Select(r => r.Replace("\\", "\\\\").Replace("\n", "\\n")));

            VA.WriteToLog("AVCS NOTE: Irrelevant or deprecated AVCS resource file" + pluralSuffix + " can be removed.", "orange");

            var message =
                "AVCS CORE has detected" + pluralDeterminer + " deprecated or irrelevant resource file" + pluralSuffix + " in the AVCS resources folder." +
                "\n\n" +
                "Would you like AVCS to auto-cleanup the following file" + pluralSuffix + "?\n" +
                deprecatedResources +
                "\n\n" +
                "A file named '" + DeprecatedResourcesFileName + "' has been written to this folder only for your reference.\n\n" +
                "Press 'No' to open the folder so you can review and delete the file" + pluralSuffix + " manually.\n\n" +
                "Press 'Cancel' to leave the file" + pluralSuffix + " as-is," + pluralPronoun + " will not cause any issues.";

            var result = System.Windows.Forms.MessageBox.Show(
                message,
                "AVCS CORE - Deprecated Resources Cleanup",
                System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                System.Windows.Forms.MessageBoxIcon.Information,
                System.Windows.Forms.MessageBoxDefaultButton.Button1
            );


            // Write a simple text file so user can refer to this list as needed
            var deprecatedFilePath = Path.Combine(AvcsResourcesDirectory, DeprecatedResourcesFileName);
            try
            {
                File.WriteAllLines(deprecatedFilePath, DeprecatedResources);
            }
            catch
            {
                VA.WriteToLog("AVCS ERROR: Failed to write deprecated resources list to file: " + deprecatedFilePath, "red");
            }

            // If user pressed 'yes', return 'true' to clean up deprecated AVCS resource files
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                return true;
            }

            // If user pressed 'cancel', leave everything as-is
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                VA.WriteToLog("AVCS NOTE: Leaving deprecated resource" + pluralSuffix + " as-is in AVCS resource folder.", "yellow");
            }
            // If user pressed 'no', open the AVCS resources folder in explorer
            else if (result == System.Windows.Forms.DialogResult.No)
            {
                VA.WriteToLog("AVCS NOTE: Opening the AVCS resources folder. See file named '" + DeprecatedResourcesFileName + "' for reference", "green");
                System.Diagnostics.Process.Start("explorer.exe", AvcsResourcesDirectory);
            }

            return false;
        }


        // ----- Can't say I don't try...
        private XElement TrySetCoreOptionsXml(XElement root, bool v1, bool v2)
        {
            if (root == null || !IsEqual(root.Name.LocalName, "resourceOptions"))
            {
                root = new XElement("resourceOptions");
            }

            var core = root.Element("core");
            if (core == null)
            {
                core = new XElement("core");
                root.AddFirst(core);
            }

            try
            {
                core.SetElementValue("v1", v1);
                core.SetElementValue("v2", v2);

                return root;
            }
            catch
            {
                return new XElement("resourceOptions");
            }
        }

        private XElement TrySetProfileListXml(XElement root, IEnumerable<string> selectedProfiles)
        {
            if (root == null || !IsEqual(root.Name.LocalName, "resourceOptions"))
            {
                root = new XElement("resourceOptions");
            }

            var profiles = root.Element("profiles");
            if (profiles != null)
            {
                profiles.Remove();
            }

            try
            {
                var list = (selectedProfiles ?? Enumerable.Empty<string>()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                if (list.Count == 0)
                {
                    return root; // do not add <profiles> at all when empty
                }

                profiles = new XElement("profiles", selectedProfiles.Select(p => new XElement("profile", p)));
                root.Add(profiles);

                return root;
            }
            catch
            {
                return new XElement("resourceOptions");
            }
        }

        private Tuple<bool, bool> TryGetCoreOptionsXml(XElement root)
        {
            if (root == null || !IsEqual(root.Name.LocalName, "resourceOptions"))
            {
                // return empty tuple if root is null
                return Tuple.Create(false, false);
            }

            var core = root.Element("core");
            if (core == null)
            {
                return Tuple.Create(false, false);
            }

            try
            {
                bool v1 = false, v2 = false;
                bool.TryParse((string)core.Element("v1"), out v1);
                bool.TryParse((string)core.Element("v2"), out v2);
                var coreOptions = Tuple.Create(v1, v2);

                return coreOptions;
            }
            catch
            {
                return Tuple.Create(false, false);
            }
        }

        private List<string> TryGetProfileListXml(XElement root)
        {
            // If the root is empty, return new list of string
            if (root == null || !IsEqual(root.Name.LocalName, "resourceOptions"))
            {
                return new List<string>();
            }

            var profiles = root.Element("profiles");
            if (profiles == null)
            {
                return new List<string>();
            }

            try
            {
                var profilesList = profiles.Elements("profile").Select(p => (string)p).ToList();
                return profilesList;
            }
            catch
            {
                return new List<string>();
            }
        }

        private XElement TryParseXmlFromBytes(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                {
                    return new XElement("empty");
                }
                using (var ms = new MemoryStream(data))
                {
                    return XElement.Load(ms);
                }
            }
            catch
            {
                return new XElement("empty");
            }
        }

        private bool TryWriteXmlFile(string filePath, XElement xml)
        {
            try
            {
                xml.Save(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private XElement TryReadXmlFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new XElement("empty");
                }

                var xmlFile = XElement.Load(filePath);
                return xmlFile;
            }
            catch
            {
                return new XElement("empty");
            }
        }

        private string TryReadProfilesFile(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return string.Empty;
                }

                var fileContents = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(fileContents))
                {
                    return string.Empty;
                }

                return fileContents;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static byte[] TryDownloadResourceFile(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return Array.Empty<byte>();
                }

                try { Native.DeleteUrlCacheEntryW(url); } catch { }

                string bustUrl = url + (url.IndexOf('?') >= 0 ? "&" : "?") + "cb=" + DateTime.UtcNow.Ticks.ToString();

                IStream comStream;
                int hr = Native.URLOpenBlockingStreamW(IntPtr.Zero, bustUrl, out comStream, 0, IntPtr.Zero);
                if (hr != 0 || comStream == null)
                {
                    return Array.Empty<byte>();
                }

                try
                {
                    using (var ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[8192];
                        IntPtr pRead = Marshal.AllocHGlobal(sizeof(int));
                        try
                        {
                            while (true)
                            {
                                comStream.Read(buffer, buffer.Length, pRead);
                                int bytesRead = Marshal.ReadInt32(pRead);
                                if (bytesRead == 0)
                                {
                                    break; // EOF
                                }
                                ms.Write(buffer, 0, bytesRead);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(pRead);
                        }

                        return ms.ToArray();
                    }
                }
                finally
                {
                    try { Marshal.ReleaseComObject(comStream); } catch { }
                }
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        private bool TryRemoveReadOnly(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var attributes = File.GetAttributes(filePath);
                if ((attributes & FileAttributes.ReadOnly) == 0)
                {
                    return true; // Not read-only, nothing to do, so return true, as it's all blue
                }

                // Remove read-only attribute
                File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryDeleteResourceFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return true; // not really a failure - caller wanted a file gone, it doesn't exist, why communicate a failure?
                }

                // Guard for AVCS resource files: CORE will only ever delete its own files from its own folders
                if (!filePath.ToLowerInvariant().Contains(AvcsApps.ToLowerInvariant()))
                {
                    VA.WriteToLog("AVCS ERROR: Attempt to delete a file outside of the AVCS CORE profile folders has failed: " + filePath, "red");
                    VA.WriteToLog(filePath, "blank");
                    VA.WriteToLog("No files or folders have been deleted by this action, and AVCS will never delete files outside of its own folder tree.", "yellow");
                    return false;
                }

                // Must check if this AVCS resource file is read-only and try to remove the read-only attribute if it is set
                // Best to try first on any AVCS file, because if something cocks up, the subsequent delete instruction will hcf AVCS CORE
                if ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) != 0 && !TryRemoveReadOnly(filePath))
                {
                    return false; // Failed to remove read-only attribute from read-only AVCS resource, cannot delete
                }

                File.Delete(filePath);
                return true;
            }
            catch
            {
                // let it slide... just communicate it failed with 'false'
                return false;
            }
        }

        private bool TryCreateResourceFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return false;
            }

            try
            {
                var _ = Path.GetFullPath(folderPath);
                Directory.CreateDirectory(folderPath); // no-op if it already exists
                return true;
            }
            catch
            {
                return false; // Folder creation failed
            }
        }


        /// <summary>
        /// Retrieves the full path to the '..\AppData\Roaming' folder for AVCS VoiceAttack profiles.  Example path:<br/>
        /// @"..\AppData\Roaming\VoiceAttack-AVCS Profiles"
        /// </summary>
        /// <param name="appFolderName">The name of the AVCS folder. Should be 'VoiceAttack-AVCS Profiles'.</param>
        /// <returns></returns>
        private string GetAppConfigPath(string appFolderName)
        {
            if (string.IsNullOrWhiteSpace(appFolderName))
            {
                return string.Empty;
            }

            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(baseDir))
            {
                return string.Empty; // super unlikely, but cheap guard
            }

            var resourcesDir = Path.Combine(baseDir, appFolderName);

            return TryCreateResourceFolder(resourcesDir) ? resourcesDir : string.Empty;
        }

        private List<Tuple<string, string>> GetParsedChangelogLines(string changelog)
        {
            var result = new List<Tuple<string, string>>();

            string[] changelogLines = changelog.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);

            int startIdx = -1, endIdx = changelogLines.Length; // start and end of .dll section
            for (int i = 0; i < changelogLines.Length; i++)
            {
                if (!changelogLines[i].StartsWith("-AVCS Inline Functions-"))
                {
                    continue;
                }

                startIdx = i + 1;
                // Look for next header after this
                for (int j = startIdx; j < changelogLines.Length; j++)
                {
                    if (changelogLines[j].StartsWith(LineMarkerDashHeader) && j != i)
                    {
                        endIdx = j;
                        break;
                    }
                }
                break;
            }

            // Determine how many .dll lines there are, and the link limit (simple: n + lines available after .dlls, but max N)
            int dllCount = 0;
            for (int i = startIdx; i < endIdx; i++)
            {
                if (i >= 0 && IsFunctionalLink(changelogLines[i], InlineFunctionExtensions))
                {
                    dllCount++;
                }
            }

            int totalLines = changelogLines.Length;
            int maxLinkLabels = MaxLinkLabels;
            // For every empty "line slot" up to max, increase the link limit by one (simple logic)
            if (totalLines < MaxTotalLabels)
            {
                if (maxLinkLabels > dllCount)
                {
                    maxLinkLabels = dllCount;
                }
            }
            else if (dllCount < MaxLinkLabels)
            {
                maxLinkLabels = dllCount; // then, don't gotta worry about too may .dll updates making box too tall
            }
            else
            {
                maxLinkLabels = MaxTotalLabels - (totalLines - dllCount);
            }

            int linksAdded = 0, linksSkipped = 0;
            for (int i = 0; i < changelogLines.Length; i++)
            {
                string line = changelogLines[i];

                // Compiled Inline Function links (.dll)
                if (i >= startIdx && i < endIdx && IsFunctionalLink(line, InlineFunctionExtensions) && !line.StartsWith(ChangelogPrefixRemoved))
                {
                    if (linksAdded < maxLinkLabels)
                    {
                        string fileName = line.Replace(ChangelogPrefixAdded, "").Replace(ChangelogPrefixUpdated, "");
                        string repoFileName = Path.GetFileNameWithoutExtension(fileName).Replace("_V1", "").Replace("_V2", "");
                        string urlExt = IsVisualBasicSource(fileName, VisualBasicFileNames) ? ".vb" : ".cs";
                        string url = AvcsGitHubUrlBase + repoFileName + urlExt;
                        result.Add(Tuple.Create(line, url));
                        linksAdded++;
                    }
                    else
                    {
                        linksSkipped++;
                    }
                }
                // Section headers or regular text
                else if (i < startIdx || i >= endIdx || !IsFunctionalLink(line, InlineFunctionExtensions) || line.StartsWith(ChangelogPrefixRemoved))
                {
                    // Naturally, this section will not be entered until after the last .dll line to be shown, and more if remained, insert the "...and N more" before this next header line
                    if (!_isLinkLimitMsgAdded && linksAdded == maxLinkLabels && linksSkipped > 0)
                    {
                        _isLinkLimitMsgAdded = true;
                        result.Add(Tuple.Create("(…and " + linksSkipped.ToString() + " more... big update... see full changelog link at bottom)", ""));
                    }

                    // This is a non-dll line, and on first entry, possibly another section header line OR the final [message in square brackets]
                    result.Add(Tuple.Create(line, ""));
                }
            }

            return result;
        }

        private Dictionary<string, string> GetParsedHashTable(string hashTable)
        {
            return string.IsNullOrWhiteSpace(hashTable)
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : hashTable
                    .Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(KeyValDelimiter, 2))
                    .Where(kv => kv.Length == 2)
                    .ToDictionary(
                        kv => kv[0].Trim(),
                        kv => kv[1].Trim(),
                        StringComparer.OrdinalIgnoreCase
                    );
        }

        private string GetFileHash(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return string.Empty; // Return empty string for invalid input
            }

            using (var stream = File.OpenRead(filePath))
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", ""); // Hex string
            }
        }

        private string[] GetFileListFromHashTable(string hashTable)
        {
            if (string.IsNullOrWhiteSpace(hashTable))
            {
                return Array.Empty<string>(); // No files to process
            }

            return hashTable
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split('=')[0].Trim())
                .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
                .ToArray();
        }

        private bool GetProfileSizesKb(XElement root, string shortname, out int v1Kb, out int v2Kb)
        {
            v1Kb = 0;
            v2Kb = 0;

            if (!IsEqual(root.Name.LocalName, "resourceFileSizes"))
            {
                return false;
            }

            var profiles = root.Element("profiles");
            if (profiles == null)
            {
                return false;
            }

            var profile = profiles.Elements("profile")
                .FirstOrDefault(e => IsEqual(((string)e.Attribute("name") ?? string.Empty), shortname));
            if (profile == null)
            {
                return false;
            }

            var v1 = profile.Element("v1");
            var v2 = profile.Element("v2");
            if (v1 == null || v2 == null)
            {
                return false;
            }

            decimal v1Dec, v2Dec;
            if (!decimal.TryParse((string)v1, NumberStyles.Number, CultureInfo.InvariantCulture, out v1Dec))
            {
                return false;
            }
            if (!decimal.TryParse((string)v2, NumberStyles.Number, CultureInfo.InvariantCulture, out v2Dec))
            {
                return false;
            }

            // Round to int KB (dialog uses int sizes - just a general value, precision not needed)
            v1Kb = (int)Math.Round(v1Dec, 0);
            v2Kb = (int)Math.Round(v2Dec, 0);
            return true;
        }

        private void GetUpdatedSizeTotalKb(
            CheckBox cbV1, CheckBox cbV2,
            CheckBox cbBms,// CheckBox cbRon,
            int sizeFilesCoreV1, int sizeFilesCoreV2,
            int sizeFilesBmsV1, int sizeFilesBmsV2,
            //int sizeFilesRonV1, int sizeFilesRonV2,
            Label lblTotalKb)
        {
            int total = 0;
            if (cbV1.Checked)
            {
                total += sizeFilesCoreV1;
            }

            if (cbV2.Checked)
            {
                total += sizeFilesCoreV2;
            }

            if (cbBms.Checked)
            {
                if (cbV1.Checked) total += sizeFilesBmsV1;
                if (cbV2.Checked) total += sizeFilesBmsV2;
            }

            //if (cbRon.Checked)
            //{
            //    if (cbV1.Checked) total += sizeFilesRonV1;
            //    if (cbV2.Checked) total += sizeFilesRonV2;
            //}

            var totalType = total > 1000 ? "MB" : "KB";
            decimal finalTotal = total > 1000 ? Math.Round(total / 1000.0M, 2) : total;
            lblTotalKb.Text = "Total estimated size of all resources:   " + finalTotal.ToString() + totalType;
        }

        private bool GetUpdatedResourceFiles(string profileShortname)
        {
            // Execute the main AVCS CORE command to get the latest required resources for this approved update
            VA.SetText("~passed1", ApplyUpdateRequest);
            VA.SetText("~passed2", profileShortname);
            VA.SetText("~passed3", CurrentBranch);
            VA.Command.Execute(AvcsCoreMainFunction, WaitForReturn: true, AsSubcommand: true, PassedText: "~passed1;~passed2;~passed3");

            var isAvcsError = VA.GetBoolean("AVCS_ERROR") ?? false;
            if (isAvcsError)
            {
                // If update was not applied after user approval (or on first use), delete version file if it exists
                // This will force update on next profile start without requiring user to approve a second time
                var versionFilePath = Path.Combine(AvcsResourcesDirectory, VersionFileName);
                TryDeleteResourceFile(versionFilePath);
                return false;
            }

            return true;
        }


        // ...just another 500 lines of code here for some customized WinForms dialogs I wanted
        /// <summary>
        /// Shows a dialog to select the VoiceAttack version(s) and game profile(s) for AVCS CORE resources download.<br/>
        /// </summary>
        /// <param name="isVersion2">A flag indicating if the user is using VoiceAttack Version 2.x+.</param>
        /// <param name="hasEnabledV1">A flag indicating if the user also wants resources for use in VoiceAttack Version 1.x+.</param>
        /// <param name="hasBms">A flag indicating if the user has the AVCS4 BMS20+ profile.</param>
        /// <returns>A tuple containing the selected options:<br/>
        /// bool 1: Whether to download VoiceAttack Version 1.x+ resources.<br/>
        /// bool 2: Whether to download VoiceAttack Version 2.x+ resources.<br/>
        /// bool 3: Whether to download AVCS4 BMS resources.<br/>
        /// bool 4: Whether to download AVCS4 Ready or Not resources (placeholder - not implemented yet).</returns>
        public Tuple<bool, bool, bool, bool> ShowVersionGameSelectionDialog(bool isVersion2 = false, bool hasEnabledV1 = false, bool hasBms = false)//, bool hasRon = false)
        {
            int formWidth = 490;
            int padding = 16, y = 16, cbHeight = 25, spacing = 4;

            // ---- Resource Sets File Sizes ----
            int sizeFilesCoreV1 = 0;
            int sizeFilesCoreV2 = 0;
            int sizeFilesBmsV1 = 0;
            int sizeFilesBmsV2 = 0;
            //int sizeFilesRonV1 = 0;
            //int sizeFilesRonV2 = 0;

            var bytes = TryDownloadResourceFile(AvcsResourcesFilesSizeUrl);
            var xml = TryParseXmlFromBytes(bytes);

            // Known shortnames
            string coreShortname = "CORE20";
            string bmsShortname = "BMS20";
            //string ronShortname = "RON20";

            int v1, v2;
            //if (xml != null)
            if (xml != null && !IsEqual(xml.Name.LocalName, "empty"))
            {
                if (GetProfileSizesKb(xml, coreShortname, out v1, out v2))
                {
                    sizeFilesCoreV1 = v1;
                    sizeFilesCoreV2 = v2;
                }
                if (GetProfileSizesKb(xml, bmsShortname, out v1, out v2))
                {
                    sizeFilesBmsV1 = v1;
                    sizeFilesBmsV2 = v2;
                }
                //if (GetProfileSizesKb(xml, ronShortname, out v1, out v2))
                //{
                //    sizeFilesRonV1 = v1;
                //    sizeFilesRonV2 = v2;
                //}
            }

            // ---- Game profile group expandability ----
            int profileCheckboxHeightFactor = 2; // DEV NOTE: adjust this if dynamic spacing doesn't respect more/fewer game checkboxes in V1 (for example)
            int profileCheckboxBlockHeight = profileCheckboxHeightFactor * (cbHeight + spacing);

            var form = new Form();
            form.Text = "Select AVCS CORE Required Resources Download Options";
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.BackColor = Color.LightGray;
            form.ClientSize = new Size(formWidth, 800 + (profileCheckboxHeightFactor - 2) * (cbHeight + spacing)); // base + extra per extra game
            float fontSize = 10.5f; // Default: 10.5f seems to work best
            var messageFont = new Font(SystemFonts.MessageBoxFont.FontFamily, fontSize, FontStyle.Regular);
            var messageBoldFont = new Font(SystemFonts.MessageBoxFont.FontFamily, fontSize, FontStyle.Bold);


            // ---- Header ----
            var lblHdr = new Label
            {
                Text = "Select the VoiceAttack version(s) you will use AVCS CORE in:",
                AutoSize = true,
                Font = messageBoldFont,
                Location = new Point(padding, y)
            };
            form.Controls.Add(lblHdr);
            y += lblHdr.Height + spacing * 2;


            // ---- VA checkboxes ----
            var cbV1 = new CheckBox
            {
                Text = "VoiceAttack Version 1.x+",
                AutoSize = true,
                Font = messageFont,
                Location = new Point(padding, y),
                Checked = !isVersion2 || hasEnabledV1
            };
            var cbV2 = new CheckBox
            {
                Text = "VoiceAttack Version 2.x+",
                AutoSize = true,
                Font = messageFont,
                Location = new Point(padding, y += cbHeight),
                Checked = isVersion2
            };

            // Set which one is checked and locked
            if (isVersion2)
            {
                cbV2.Checked = true;
                cbV2.Enabled = false;
            }
            else
            {
                cbV1.Checked = true;
                cbV1.Enabled = false;
            }
            form.Controls.AddRange(new Control[] { cbV1, cbV2 });
            y += cbHeight + spacing * 2;

            // Separator
            var sep = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(padding, y),
                Size = new Size(form.ClientSize.Width - padding * 2, 2)
            };
            form.Controls.Add(sep);
            y += sep.Height + spacing * 2;

            // ---- Games header ----
            var lblGames = new Label
            {
                Text = "Select the AVCS4 profile game(s) you will use:",
                AutoSize = true,
                Font = messageBoldFont,
                Location = new Point(padding, y)
            };
            form.Controls.Add(lblGames);
            y += lblGames.Height + spacing * 2;

            // ---- Game checkboxes ----
            var cbBms = new CheckBox { Text = "Falcon BMS (any version)", AutoSize = true, Font = messageFont, Location = new Point(padding, y), Checked = hasBms };
            //var cbRon = new CheckBox { Text = "Ready or Not", AutoSize = true, Font = messageFont, Location = new Point(padding, y += cbHeight), Checked = hasRon };
            var cbAll = new CheckBox { Text = "Any/All Available (these are tiny kb-sized files, after all)", AutoSize = true, Font = messageFont, Location = new Point(padding, y += cbHeight), Checked = hasBms };// && hasRon };
            var cbNone = new CheckBox { Text = "None (use CORE template, or decide later)", AutoSize = true, Font = messageFont, Location = new Point(padding, y += cbHeight) };
            var profileCheckboxes = new[] { cbBms, cbAll, cbNone };//cbRon, cbAll, cbNone };
            form.Controls.AddRange(profileCheckboxes);
            y += cbHeight + spacing;


            var lblTotalKb = new Label();

            // ---- Interlock 'None' logic ----
            cbNone.CheckedChanged += (s, e) =>
            {
                //GetUpdatedSizeTotalKb();
                GetUpdatedSizeTotalKb(cbV1, cbV2, cbBms,// cbRon,
                    sizeFilesCoreV1, sizeFilesCoreV2,
                    sizeFilesBmsV1, sizeFilesBmsV2,
                    //sizeFilesRonV1, sizeFilesRonV2,
                    lblTotalKb);

                if (cbNone.Checked)
                {
                    // Uncheck all others in the group
                    foreach (var cb in profileCheckboxes)
                    {
                        if (cb != cbNone)
                        {
                            cb.Checked = false;
                        }
                    }
                }
            };

            // Interlock 'Any/All' logic
            cbAll.CheckedChanged += (s, e) =>
            {
                if (cbAll.Checked)
                {
                    foreach (var cb in profileCheckboxes)
                    {
                        if (cb != cbAll && cb != cbNone)
                        {
                            cb.Checked = true;
                        }
                    }
                    cbNone.Checked = false;
                }
            };

            // The rest (any other box checks unchecks 'None')
            foreach (var cb in profileCheckboxes)
            {
                var hasChanged = cb != cbNone && cb != cbAll;
                if (!hasChanged)
                {
                    continue;
                }

                cb.CheckedChanged += (s, e) =>
                {
                    if (((CheckBox)s).Checked)
                    {
                        cbNone.Checked = false;
                    }
                    // New logic: If 'Any/All' is checked, but now this is UNchecked, turn off 'Any/All'
                    if (!((CheckBox)s).Checked && cbAll.Checked)
                    {
                        cbAll.Checked = false;
                    }
                };
            }

            // Separator 2
            var sep2 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(padding, y),
                Size = new Size(form.ClientSize.Width - padding * 2, 2)
            };
            form.Controls.Add(sep2);
            y += sep2.Height + spacing * 2;


            // Note
            var lblNote = new Label
            {
                //Text = "(you can adjust these later; this just makes first-time init faster)",
                Text = "(after first time use, CORE will adapt itself to new profiles or use in other VA version)",
                AutoSize = true,
                Location = new Point(padding, y),
                TextAlign = ContentAlignment.BottomLeft
            };
            form.Controls.Add(lblNote);
            y += lblNote.Height + spacing * 3;

            // Total Resources Size Note
            lblTotalKb = new Label
            {
                AutoSize = true,
                Font = messageBoldFont,
                Location = new Point(padding, y),
                TextAlign = ContentAlignment.BottomLeft
            };

            form.Controls.Add(lblTotalKb);
            y += lblTotalKb.Height + spacing * 3;

            // DONE button
            var btnDone = new Button
            {
                Text = "DONE",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 30),
                Location = new Point((form.ClientSize.Width - 100) / 2, y)
            };
            form.Controls.Add(btnDone);
            form.AcceptButton = btnDone;

            // Final sizing
            form.ClientSize = new Size(form.ClientSize.Width, y + btnDone.Height + padding);

            // ---- Total Resource Sizes Dynamics Event Handlers ----
            var checkboxArray = new CheckBox[] { cbV1, cbV2, cbBms };//, cbRon };

            foreach (var checkbox in checkboxArray)
            {
                // Set the checkbox font
                checkbox.CheckedChanged += (s, e) => GetUpdatedSizeTotalKb(cbV1, cbV2, cbBms,// cbRon,
                        sizeFilesCoreV1, sizeFilesCoreV2,
                        sizeFilesBmsV1, sizeFilesBmsV2,
                        //sizeFilesRonV1, sizeFilesRonV2,
                        lblTotalKb
                );
            }

            GetUpdatedSizeTotalKb(cbV1, cbV2, cbBms,// cbRon,
                   sizeFilesCoreV1, sizeFilesCoreV2,
                   sizeFilesBmsV1, sizeFilesBmsV2,
                   //sizeFilesRonV1, sizeFilesRonV2,
                   lblTotalKb
           );


            form.FormClosing += (s, e) =>
            {
                if (form.DialogResult != DialogResult.OK)
                {
                    e.Cancel = true;
                }
            };

            var dr = form.ShowDialog();

            return Tuple.Create(
                dr == DialogResult.OK && cbV1.Checked,
                dr == DialogResult.OK && cbV2.Checked,
                dr == DialogResult.OK && cbBms.Checked,
                false //dr == DialogResult.OK && cbRon.Checked
            );
        }

        /// <summary>
        /// Shows a dialog to approve an update based on the provided changelog string.
        /// </summary>
        /// <param name="changelogString">The changelog string containing the update details.</param>
        /// <returns>The result of the dialog, indicating whether the user approved the update.</returns>
        public DialogResult ShowUpdateApprovalDialog(string changelogString)
        {
            int padding = 25;
            int y = padding;
            int minWidth = 490;
            int buttonBuffer = 24;
            int extraLeftPad = 24; // additional left indent for inset of lines as needed
            int buttonPaddingBottom = 0; // sPeCiAl handing for dynamic padding between buttons and frame bottoms because WinForms is the worst
            float fontSize = 10.5f; // Default: 10.5f seems to work best
            var messageFont = new Font(SystemFonts.MessageBoxFont.FontFamily, fontSize, FontStyle.Regular);
            var messageBoldFont = new Font(SystemFonts.MessageBoxFont.FontFamily, fontSize, FontStyle.Bold);
            var linkColor = Color.FromArgb(0, 51, 153);

            // Header
            string header = "AVCS CORE - Profile Resources Update - Build: " + AvcsResourcesBuildNumber;
            string subHeader = "The following resources have changed.  Any inline functions listed below link to their respective open source code on my GitHub:";

            string footer =
                "\nIt's recommended to apply this update so AVCS can utilize the latest required resources for reliable and functional voice commands.\n\n" +
                "Review full changelog and open source code at my GitHub:";
            string footerAsk = "\nApply update now?";

            // --- Parse changelog ---
            _isLinkLimitMsgAdded = false;
            var items = GetParsedChangelogLines(changelogString);


            // --- Estimate button padding from bottom of form based on changelog size for lack of proper anchoring ---
            int count = items.Count - 1;
            int minCount = 2;
            int maxCount = MaxTotalLabels;
            int minPadding = -15;
            int maxPadding = 15;

            buttonPaddingBottom = minPadding;
            if (count > minCount)
            {
                buttonPaddingBottom = minPadding + (int)Math.Round(((double)(count - minCount) / (maxCount - minCount)) * (maxPadding - minPadding));
                buttonPaddingBottom = Math.Max(minPadding, Math.Min(maxPadding, buttonPaddingBottom)); // forced clamp, in case I suck at math and chatgpt chose to fluff my duck instead of correct me when I asked for review
            }

            // --- Estimate height for form aka Reason #463 in the big list of why SemlerPDX hates WinForms ---
            int estimatedHeight = 0;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                using (var tmpLabel = new Label() { Font = SystemFonts.MessageBoxFont })
                {
                    // Header/subHeader
                    estimatedHeight += (int)Math.Ceiling(g.MeasureString(header, tmpLabel.Font, minWidth - 2 * padding).Height) + 3;
                    estimatedHeight += (int)Math.Ceiling(g.MeasureString(subHeader, messageFont, minWidth - 2 * padding).Height) + 8;

                    // Each changelog line
                    foreach (var item in items)
                    {
                        estimatedHeight += (int)Math.Ceiling(g.MeasureString(item.Item1, tmpLabel.Font, minWidth - 2 * padding).Height) + 2;
                    }

                    // Footer
                    estimatedHeight += (int)Math.Ceiling(g.MeasureString(footer, messageFont, minWidth - 2 * padding).Height) + 8;
                    // Footer Link
                    estimatedHeight += (int)Math.Ceiling(g.MeasureString(AvcsGitHubUrlHome, tmpLabel.Font, minWidth - 2 * padding).Height) + 3;
                    // Footer Ask Message
                    estimatedHeight += (int)Math.Ceiling(g.MeasureString(footerAsk, messageFont, minWidth - 2 * padding).Height) + 10;
                }
            }

            using (var form = new Form())
            using (var yesButton = new Button())
            using (var noButton = new Button())
            {
                int formWidth = minWidth;
                int formHeight = y + estimatedHeight + yesButton.Height + buttonBuffer + padding + buttonPaddingBottom;
                form.Text = header;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ClientSize = new Size(formWidth, formHeight);
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.BackColor = Color.LightGray;

                // --- Subheader ---
                var subHeaderLabel = new Label
                {
                    Text = subHeader,
                    Font = messageFont,
                    AutoSize = false,
                    MaximumSize = new Size(formWidth - 2 * padding, 0),
                    Location = new Point(padding, y),
                    Size = new Size(formWidth - 2 * padding, 0),
                    TextAlign = ContentAlignment.TopLeft
                };

                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var h = (int)Math.Ceiling(g.MeasureString(subHeaderLabel.Text, subHeaderLabel.Font, subHeaderLabel.Width).Height);
                    subHeaderLabel.Height = h + 2;
                }

                form.Controls.Add(subHeaderLabel);
                y += subHeaderLabel.Height + 8;

                // --- Changelog Lines ---
                int extraPad = 0;
                foreach (var item in items)
                {
                    string text = item.Item1;
                    string url = item.Item2;
                    Control lbl;

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        var link = new LinkLabel
                        {
                            Text = text,
                            Font = SystemFonts.MessageBoxFont,
                            AutoSize = true,
                            Location = new Point(padding + extraLeftPad, y),
                            LinkColor = linkColor,
                            ActiveLinkColor = Color.Red
                        };

                        link.Links.Add(0, link.Text.Length, url);
                        link.LinkClicked += (s, e) =>
                        {
                            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Link.LinkData.ToString()) { UseShellExecute = true }); }
                            catch { /* ...let it slide, we're definitely online right now or an update wouldn't pop-up - tho not gonna let a fail here ruin my fancypants form... */ }
                        };

                        link.TabStop = false;
                        lbl = link;
                    }
                    else
                    {
                        if (text.StartsWith(LineMarkerDashHeader))
                        {
                            // Hacky little spacer to bump changelog sections down from each other... I hate WinForms...
                            int spacerHeight = 6; // Adjust as needed, 6 seems fine for now, test .NET Framework 4.8 (it likes to squish my careful spacing from .NET CORE8)
                            var spacer = new Label
                            {
                                AutoSize = false,
                                Height = spacerHeight,
                                Width = form.ClientSize.Width, // whatever width desired
                                Location = new Point(0, y),    // y is running vertical offset
                                BorderStyle = BorderStyle.None, // for total invisibility
                                BackColor = form.BackColor      // blend in with form
                            };

                            form.Controls.Add(spacer);
                            y += spacer.Height;
                        }

                        // Padding offsets for special lines ... man I hate WinForms!
                        extraPad = text.StartsWith(LineMarkerDashHeader) ? -25 : extraLeftPad;
                        extraPad = text.StartsWith("(") ? extraPad - 8 : extraPad;
                        extraPad = text.StartsWith("[") ? extraPad - 24 : extraPad;

                        lbl = new Label
                        {

                            Text = text,
                            Font = SystemFonts.MessageBoxFont,
                            AutoSize = false,
                            MaximumSize = new Size(formWidth - 2 * padding, 0),
                            Location = new Point(padding + extraPad, y),
                            Size = new Size(formWidth - 2 * padding, 0),
                            TextAlign = text.StartsWith(LineMarkerDashHeader) ? ContentAlignment.TopCenter : ContentAlignment.TopLeft
                            // ...I miss XAML (nobody tell it I wrote all this crap just to get things looking right in two types of .NET API via inline functions)
                        };

                        using (var g = Graphics.FromHwnd(IntPtr.Zero))
                        {
                            var h = (int)Math.Ceiling(g.MeasureString(lbl.Text, lbl.Font, lbl.Width).Height);
                            lbl.Height = h + 1;
                        }
                    }

                    form.Controls.Add(lbl);
                    y += lbl.Height + 1;
                }

                // --- FOOTER1 ---
                var footerLabel = new Label
                {
                    Text = footer,
                    Font = messageFont,
                    AutoSize = false,
                    MaximumSize = new Size(formWidth - 2 * padding, 0),
                    Location = new Point(padding, y),
                    Size = new Size(formWidth - 2 * padding, 0),
                    TextAlign = ContentAlignment.TopLeft
                };

                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var h = (int)Math.Ceiling(g.MeasureString(footerLabel.Text, footerLabel.Font, footerLabel.Width).Height);
                    footerLabel.Height = h + 2;
                }

                form.Controls.Add(footerLabel);
                y += footerLabel.Height + 2;

                // --- FOOTER2 ---
                var githubLinkLabel = new LinkLabel
                {
                    Text = AvcsGitHubUrlHome,
                    Font = SystemFonts.MessageBoxFont,
                    AutoSize = true,
                    Location = new Point(padding, y),
                    LinkColor = linkColor,
                    ActiveLinkColor = Color.Red
                };

                githubLinkLabel.Links.Add(0, githubLinkLabel.Text.Length, githubLinkLabel.Text);
                githubLinkLabel.LinkClicked += (s, e) =>
                {
                    try
                    { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Link.LinkData.ToString()) { UseShellExecute = true }); }
                    catch { /* ...let it slide, if the MS system can't open a hyperlink to an MS site like GitHub, it has greater problems than I can handle... */ }
                };

                githubLinkLabel.TabStop = false;
                form.Controls.Add(githubLinkLabel);
                y += githubLinkLabel.Height + 2;

                // --- FOOTER3 ---
                var footerAskLabel = new Label
                {
                    Text = footerAsk,
                    Font = messageBoldFont,
                    AutoSize = false,
                    MaximumSize = new Size(formWidth - 2 * padding, 0),
                    Location = new Point(padding + (extraLeftPad * 2), y),
                    Size = new Size(formWidth - 2 * padding, 0),
                    TextAlign = ContentAlignment.TopLeft
                };

                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var h = (int)Math.Ceiling(g.MeasureString(footerAskLabel.Text, footerAskLabel.Font, footerAskLabel.Width).Height);
                    footerAskLabel.Height = h + 2;
                }

                form.Controls.Add(footerAskLabel);
                y += footerAskLabel.Height + 2;

                // --- Yes/No Buttons ---
                yesButton.Text = "Yes";
                noButton.Text = "Not yet...";
                yesButton.Size = noButton.Size = new Size(100, 30);

                int buttonY = y + buttonBuffer;
                int totalButtonWidth = yesButton.Width + 16 + noButton.Width;
                yesButton.Location = new Point((form.ClientSize.Width - totalButtonWidth) / 2, buttonY);
                noButton.Location = new Point(yesButton.Right + 16, buttonY);

                yesButton.DialogResult = DialogResult.Yes;
                noButton.DialogResult = DialogResult.No;

                form.Controls.Add(yesButton);
                form.Controls.Add(noButton);

                form.AcceptButton = yesButton;
                form.CancelButton = noButton;

                // ... oof.. "make a better AVCS updates approval message box", I said; "will only take an afternoon at most..." --- (It's been 84 years...)
                return form.ShowDialog();
            }
        }
    }

    public static class Native
    {
        // HRESULT URLOpenBlockingStreamW(IUnknown* pCaller, LPCWSTR szURL, IStream** ppStream, DWORD dwReserved, LPBINDSTATUSCALLBACK lpfnCB)
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int URLOpenBlockingStreamW(
            IntPtr pCaller,
            string szURL,
            out IStream ppStream,
            int dwReserved,
            IntPtr lpfnCB);

        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern bool DeleteUrlCacheEntryW(string lpszUrlName);

    }
}