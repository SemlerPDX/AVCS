using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AVCS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnContinue_Click(object? sender, EventArgs e)
        {
            // Close the form and ask Program to run TestInlines() afterwards
            Program.RequestRunTests();
            Close();
        }
        private void btnCancel_Click(object? sender, EventArgs e)
        {
            // Close the form
            Close();
        }

        private void btnChecksum_Click(object? sender, EventArgs e)
        {
            try
            {
                var hashtablePath = PromptForHashtablePath();
                if (string.IsNullOrWhiteSpace(hashtablePath))
                {
                    return; // user canceled
                }

                var map = ParseHashtable(hashtablePath);

                // Keep only *.dll / *.html with a 64-char hex hash
                var wanted = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in map)
                {
                    var name = kvp.Key?.Trim();
                    var val = kvp.Value?.Trim();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(val))
                    {
                        continue;
                    }

                    var ext = Path.GetExtension(name);
                    if (!IsFunctionalExt(ext))
                    {
                        continue;
                    }

                    if (!IsSha256Hex(val))
                    {
                        continue;
                    }

                    wanted[name] = val.ToUpperInvariant();
                }

                if (wanted.Count == 0)
                {
                    MessageBox.Show("No .dll or .html entries with SHA256 values were found in the hashtable.",
                        "AVCS CORE DEV TOOLKIT", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string outputFolder;
                var results = new List<string>();

                if (_chkSelectAll.Checked)
                {
                    // Ask for the folder containing the files
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Select the folder containing the files to verify";
                        if (fbd.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            return;
                        }

                        outputFolder = fbd.SelectedPath;

                        foreach (var kvp in wanted)
                        {
                            var file = Path.Combine(outputFolder, kvp.Key);
                            results.Add(CheckOne(file, kvp.Key, kvp.Value));
                        }
                    }
                }
                else
                {
                    // Single file flow: ask for a file, and ensure its filename is in the hashtable
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Title = "Select a file to verify (must exist in hashtable)";
                        ofd.Filter = "DLL and HTML (*.dll;*.html)|*.dll;*.html|All files (*.*)|*.*";
                        if (ofd.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(ofd.FileName))
                        {
                            return;
                        }

                        var chosenPath = ofd.FileName;
                        var chosenName = Path.GetFileName(chosenPath);

                        if (!wanted.TryGetValue(chosenName, out var expectedHash))
                        {
                            MessageBox.Show(
                                "The selected file name is not in the hashtable.\n\nExpected a name listed in hashtable.txt.",
                                "AVCS CORE DEV TOOLKIT",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }

                        outputFolder = Path.GetDirectoryName(chosenPath) ?? Environment.CurrentDirectory;
                        results.Add(CheckOne(chosenPath, chosenName, expectedHash));
                    }
                }

                // Write checksum.txt (overwrite silently)
                var outputPath = Path.Combine(outputFolder, "checksum.txt");
                File.WriteAllText(outputPath, string.Join(Environment.NewLine, results), Encoding.UTF8);

                var open = MessageBox.Show(
                    "Checksum complete.\n\nOpen the folder containing checksum.txt?",
                    "AVCS CORE DEV TOOLKIT",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (open == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = outputFolder,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        MessageBox.Show("Unable to open folder:\n\n" + ex.Message, "AVCS CORE DEV TOOLKIT",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // Exit either way after user decision
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Checksum failed:\n\n" + ex.Message, "AVCS CORE DEV TOOLKIT",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string PromptForHashtablePath()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select hashtable.txt";
                ofd.Filter = "Hashtable (hashtable.txt)|hashtable.txt|Text files (*.txt)|*.txt|All files (*.*)|*.*";

                var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var resourcesDir = Path.Combine(baseDir, "VoiceAttack-AVCS Profiles", "AVCS", "resources");
                if (Directory.Exists(resourcesDir))
                {
                    ofd.InitialDirectory = resourcesDir;
                }
                else
                {
                    ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }

                var dr = ofd.ShowDialog();
                if (dr != DialogResult.OK || string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    return string.Empty;
                }

                return ofd.FileName;
            }
        }

        private static Dictionary<string, string> ParseHashtable(string path)
        {
            // Format reference taken from the sample provided.
            // One entry per line: key=value; value may be blank; we only keep .dll/.html with a 64-char SHA256 hex.
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw?.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                // allow comments with leading '#'
                if (line.StartsWith("#"))
                {
                    continue;
                }

                var idx = line.IndexOf('=');
                if (idx <= 0)
                {
                    continue; // skip malformed
                }

                var key = line.Substring(0, idx).Trim();
                var val = (idx + 1 < line.Length) ? line.Substring(idx + 1).Trim() : string.Empty;

                if (!string.IsNullOrEmpty(key))
                {
                    map[key] = val;
                }
            }

            return map;
        }

        private static bool IsFunctionalExt(string? ext)
        {
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            return ext.Equals(".dll", StringComparison.OrdinalIgnoreCase)
                   || ext.Equals(".html", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSha256Hex(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Length != 64)
            {
                return false;
            }

            // Hex check
            return Regex.IsMatch(s, "^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant);
        }

        private static string CheckOne(string fullPath, string displayName, string expectedUpperHex)
        {
            if (!File.Exists(fullPath))
            {
                return displayName + ": Missing";
            }

            try
            {
                var actual = ComputeSha256UpperHex(fullPath);
                if (string.Equals(actual, expectedUpperHex, StringComparison.Ordinal))
                {
                    return displayName + ": OK";
                }

                return displayName + ": Mismatch (" + actual + ")";
            }
            catch (Exception ex)
            {
                return displayName + ": Error (" + ex.Message + ")";
            }
        }

        private static string ComputeSha256UpperHex(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(path))
            {
                var hash = sha.ComputeHash(fs);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
                }

                return sb.ToString();
            }
        }
    }
}
