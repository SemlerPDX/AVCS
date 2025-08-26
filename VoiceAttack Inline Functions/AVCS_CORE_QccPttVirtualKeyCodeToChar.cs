namespace AVCS_CORE_QccPttVirtualKeyCodeToChar
{
    // Push to Talk Button Inline-Function for VoiceAttack -  Get keyboard virtual keycode into key for text display
    // by SemlerPDX Jan2021/July2025
    // VETERANS-GAMING.COM

    using System;
    using System.Windows.Forms;

    /*
    Required Referenced Assemblies V1:
    System.dll;System.Core.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.ComponentModel.TypeConverter.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll

    Required Referenced Assemblies V2:
    System.dll;System.Core.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.ComponentModel.TypeConverter.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll
    */

    public class VAInline
    {
        private static bool _isDebugging = false;
        private static int _debugCount = 0;

        private static int _vkCode = 0;

        public void main()
        {
            _isDebugging = VA.GetBoolean("AVCS_Debug_ON") ?? false;
            if (_isDebugging)
            {
                SendDebugMessage("AVCS QCC PTT KeyCode Conversion Entered", 2);
            }

            var keyCheck = VA.GetText("~avcs_ptt_button_test") ?? string.Empty;
            if (keyCheck.StartsWith("STATE_KEYSTATE:"))
            {
                var keyCode = keyCheck.Split(':')[1];
                if (!int.TryParse(keyCode, out _vkCode))
                {
                    SendDebugMessage("AVCS ERROR: STATE_KEYSTATE value is not a valid integer!", 4);
                    VA.SetBoolean("AVCS_ERROR", true);
                }
            }

            VA.SetText("~avcs_return_char", string.Empty);

            if (string.IsNullOrEmpty(keyCheck))
            {
                SendDebugMessage("AVCS ERROR: keyCheck is null or emtpy!", 4);
                VA.SetBoolean("AVCS_ERROR", true);
                return;
            }

            // Joystick/POV: extract a pretty direction word and normalize the label
            if (keyCheck.ToUpper().Contains("POV"))
            {
                var parts = keyCheck.Split(':');
                if (parts.Length == 2)
                {
                    var dir = parts[1]
                        .Replace("DOWN", " Down")
                        .Replace("UP", " Up")
                        .Replace("RIGHT", " Right")
                        .Replace("LEFT", " Left");

                    VA.SetText("~avcs_return_char", dir.Trim());
                    keyCheck = parts[0];
                }
            }

            // Normalize the device/state label for display/testing
            keyCheck = keyCheck
                .Replace("STATE_", "")
                .Replace("MOUSE", " Mouse")
                .Replace("KEYSTATE", "Keyboard ID# ")
                .Replace("JOYSTICK", "Joystick #")
                .Replace("BUTTON", " Button")
                .Replace("Button:", "Button ")
                .Replace("POV", " POV#")
                .Replace("TRIGGER", " Trigger");

            VA.SetText("~avcs_ptt_button_test", keyCheck.Trim());

            // If no return character yet AND virtual-key code is greater than 0, map it
            if (string.IsNullOrEmpty(VA.GetText("~avcs_return_char")) && _vkCode > 0)
            {
                var text = VkToDisplay(_vkCode);
                VA.SetText("~avcs_return_char", text);
            }
        }

        private static string VkToDisplay(int vk)
        {
            // Letters and digits map directly
            if ((vk >= '0' && vk <= '9') || (vk >= 'A' && vk <= 'Z'))
            {
                return ((char)vk).ToString();
            }

            // Numpad 0..9
            if (vk >= (int)Keys.NumPad0 && vk <= (int)Keys.NumPad9)
            {
                return ((char)('0' + (vk - (int)Keys.NumPad0))).ToString();
            }

            // OEM punctuation
            switch (vk)
            {
                case 0xC0: /* VK_OEM_3     */ return "`";
                case 0xBD: /* VK_OEM_MINUS */ return "-";
                case 0xBB: /* VK_OEM_PLUS  */ return "=";
                case 0xDB: /* VK_OEM_4     */ return "[";
                case 0xDD: /* VK_OEM_6     */ return "]";
                case 0xBA: /* VK_OEM_1     */ return ";";
                case 0xDE: /* VK_OEM_7     */ return "'";
                case 0xBC: /* VK_OEM_COMMA */ return ",";
                case 0xBE: /* VK_OEM_PERIOD*/ return ".";
                case 0xBF: /* VK_OEM_2     */ return "/";
                case 0xDC: /* VK_OEM_5     */ return "\\";
                case (int)Keys.Space: return "Space";
            }

            // Function keys F1..F24
            if (vk >= (int)Keys.F1 && vk <= (int)Keys.F24)
            {
                return ((Keys)vk).ToString(); // returns "F1", "F2", ..., "F24"
            }

            // Arrow keys, etc. — fall back to KeysConverter for a readable label
            try
            {
                var k = (Keys)vk;
                var label = new KeysConverter().ConvertToString(k) ?? k.ToString();
                return label.Replace("Return", "Enter").Replace("Capital", "CapsLock");
            }
            catch
            {
                return "VK_" + vk.ToString();
            }
        }

        /// <summary>
        /// Color code examples:
        /// 1=Blue - 2=Green - 3=Yellow - 4=Red - 5=Purple - 6=Blank - 7=Orange - 8=Black - 9=Gray - 10=Pink
        /// </summary>
        public void SendDebugMessage(string message, int color)
        {
            _debugCount++;
            VA.SetText("~avcs_qcc_debug_" + _debugCount.ToString(), message);
            VA.SetInt("~avcs_qcc_debug_wclr_" + _debugCount.ToString(), color);
        }
    }

}
