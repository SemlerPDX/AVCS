using System.Diagnostics;

namespace AVCS
{

    internal static class Program
    {
        internal static bool ShouldRunTests { get; private set; }

        /// <summary>
        /// Set by the form when user chooses to bypass checksum flow.
        /// </summary>
        internal static void RequestRunTests()
        {
            ShouldRunTests = true;
        }

        /// <summary>
        /// Dev scratchpad for ad-hoc inline testing outside VoiceAttack.
        /// </summary>
        private static void TestInlines()
        {
            // EXAMPLE (commented):
            //var tester = new AVCS_CORE_QccPttVirtualKeyCodeToChar_V1.VAInline();
            //tester.main();

            // Add whatever tiny repros or dialog tests needed here, then comment/remove as desired.
        }

        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            using (var form = new Form1())
            {
                Application.Run(form);
            }

            if (ShouldRunTests)
            {
                try
                {
                    TestInlines();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show("TestInlines() threw an exception:\n\n" + ex.Message, "AVCS CORE DEV TOOLKIT",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
