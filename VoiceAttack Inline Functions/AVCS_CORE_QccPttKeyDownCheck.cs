namespace AVCS_CORE_QccPttKeyDownCheck
{
    using System;
    using System.Threading;

    /*
    Required Assemblies in VoiceAttack V1 and V2:
    Microsoft.CSharp.dll;System.dll
    */

    /// <summary>
    /// A singleton style inline function class for VoiceAttack to monitor PTT (Push-To-Talk) key states.
    /// by SemlerPDX Jan2021/July2025
    /// VETERANS-GAMING.COM/AVCS
    /// </summary>
    public class VAInline
    {
        /// <summary>
        /// Indicates whether the PTT key monitoring is currently active.
        /// </summary>
        public static bool IsMonitorActive { get; private set; }

        public void main()
        {
            /// NOTE: Check box in Inline Function action to 'Retain Instance' for singleton, and use IsMonitorActive as guard
            if (IsMonitorActive)
            {
                return;
            }

            IsMonitorActive = true;

            string activeProfile = VA.GetText("AVCS_ACTIVE_PROFILE") ?? "CORE";
            bool listeningCanWake = VA.GetBoolean("AVCS_QCC_LISTENINGCANWAKE") ?? false;
            int listeningInterval = VA.GetInt("AVCS_" + activeProfile + "_TimeUntilStopListening") ?? 0;

            bool debugCheck = false;
            int keyMonitorType = 0;
            bool keyMonitorActive = true;

            while (keyMonitorActive)
            {
                // Top-level control variables
                activeProfile = VA.GetText("AVCS_ACTIVE_PROFILE") ?? "CORE";
                keyMonitorType = VA.GetInt("AVCS_" + activeProfile + "_PTT_MODE") ?? 0;
                listeningCanWake = VA.GetBoolean("AVCS_QCC_LISTENINGCANWAKE") ?? false;
                debugCheck = VA.GetBoolean("AVCS_PTT_DEBUG") ?? false;
                listeningInterval = VA.GetInt("AVCS_" + activeProfile + "_TimeUntilStopListening") ?? 0;

                if (listeningInterval <= 0)
                {
                    listeningInterval = 5500;
                }

                // Exit Check
                bool keyMonitoring = VA.GetBoolean("AVCS_" + activeProfile + "_RadioButtons_ON") ?? false;
                if (!keyMonitoring)
                {
                    //////////////////////////////////////////    DEBUG TESTING ////////////////////////////////////////////////
                    //VA.WriteToLog("PTT KEYMON EXITING!! AVCS_" + activeProfile + "_RadioButtons_ON == false", "pink");
                    //////////////////////////////////////////    DEBUG TESTING ////////////////////////////////////////////////
                    VA.SetBoolean("AVCS_RadioButtons_ON", false);
                    VA.SetBoolean("AVCS_PTT_MODE_ON", false);
                    keyMonitorType = 1;
                    keyMonitorActive = false;
                    PTTStartListening(keyMonitorType);
                    IsMonitorActive = false;
                    return;
                }


                int keyPollingInterval = VA.GetInt("AVCS_PTT_KEYPOLLINGINTERVAL") ?? 50;
                if (keyPollingInterval < 50)
                {
                    keyPollingInterval = 50; // enforce sensible default
                }

                Thread.Sleep(keyPollingInterval);

                bool keyDown = false;
                bool listenAwake = false;
                bool listeningAltered = false;

                // Keydown Check
                keyDown = CheckPTTkeyDownState(activeProfile);

                // Wake by Listen Check
                bool isListening = VA.GetBoolean("AVCS_QCC_LISTENING") ?? false;
                if (!keyDown && listeningCanWake && isListening)
                {
                    keyDown = true;
                    VA.SetBoolean("AVCS_QCC_LISTENING", false);
                }

                // Wake by Name Check
                bool isAwake = VA.GetBoolean("AVCS_QCC_AWAKE") ?? false;
                if (!keyDown && isAwake)
                {
                    keyDown = true;
                    listenAwake = true;
                    VA.SetBoolean("AVCS_QCC_AWAKE", false);
                }

                if (!keyDown)
                {
                    VA.SetBoolean("AVCS_RadioButtons_ON", true);
                    VA.SetBoolean("AVCS_PTT_MODE_ON", true);
                    continue;
                }
                //////////////////////////////////////////    DEBUG TESTING ////////////////////////////////////////////////
                //bool testButtOn = VA.GetBoolean("AVCS_" + activeProfile + "_RadioButtons_ON") ?? false;
                //VA.WriteToLog("CHECK:  AVCS_" + activeProfile + "_RadioButtons_ON == " + testButtOn.ToString(), "pink");

                //testButtOn = VA.GetBoolean("AVCS_CORE_RadioButtons_ON") ?? false;
                //VA.WriteToLog("CHECK:  AVCS_CORE_RadioButtons_ON == " + testButtOn.ToString(), "pink");
                //////////////////////////////////////////    DEBUG TESTING ////////////////////////////////////////////////

                VA.SetBoolean("AVCS_PTT_KeyDown", true);
                VA.SetBoolean("AVCS_" + activeProfile + "_PTT_KeyDown", true);

                keyMonitorType = VA.GetInt("AVCS_" + activeProfile + "_PTT_MODE") ?? 0;
                PTTStartListening(keyMonitorType);

                if (debugCheck)
                {
                    SendDebugMessage("AVCS PTT KEY PRESS / WAKE DETECTED", 2);
                }

                // If Listen Wake Hotfix enabled, allow VAS Loop to increase wait interval here
                if (listeningCanWake && VA.GetDecimal("AVCS_VAS_TIMEOUT") != null)
                {
                    listeningInterval = 8500;
                    listeningAltered = true;
                }

                // Secondary Loop - Wait until done / stop listening
                for (int i = 0; i <= listeningInterval; i += 500)
                {
                    Thread.Sleep(500);

                    if (debugCheck && i >= listeningInterval)
                    {
                        SendDebugMessage("AVCS PTT LISTENING INTERVAL COMPLETE", 4);
                    }

                    // Secondary Exit Check
                    keyMonitoring = VA.GetBoolean("AVCS_" + activeProfile + "_RadioButtons_ON") ?? false;
                    if (!keyMonitoring)
                    {
                        VA.SetBoolean("AVCS_RadioButtons_ON", false);
                        VA.SetBoolean("AVCS_PTT_MODE_ON", false);
                        keyMonitorType = 1;
                        keyMonitorActive = false;
                        PTTStartListening(keyMonitorType);
                        break;
                    }

                    // Extend interval if awakened again
                    isAwake = VA.GetBoolean("AVCS_QCC_AWAKE") ?? false;
                    if (isAwake)
                    {
                        i = 0;
                        listenAwake = true;
                        VA.SetBoolean("AVCS_QCC_AWAKE", false);
                    }

                    // If wait loop entered via keyDown, allow immediate on/off
                    if (listenAwake)
                    {
                        break;
                    }

                    keyDown = CheckPTTkeyDownState(activeProfile);
                    if (keyDown)
                    {
                        i = 0;
                        PTTStartListening(keyMonitorType);
                    }
                    else
                    {
                        PTTStopListening(keyMonitorType);
                    }
                }

                // Reset all function variables
                VA.SetBoolean("AVCS_PTT_KeyDown", false);
                VA.SetBoolean("AVCS_" + activeProfile + "_PTT_KeyDown", false);

                PTTStopListening(keyMonitorType);

                keyDown = false;
                listenAwake = false;

                if (listeningCanWake && listeningAltered)
                {
                    listeningAltered = false;
                    listeningInterval = 5500;
                    int altInterval = VA.GetInt("AVCS_" + activeProfile + "_TimeUntilStopListening") ?? 0;
                    if (altInterval > 0)
                    {
                        listeningInterval = altInterval;
                    }
                }
            }

            IsMonitorActive = false;
            keyMonitorActive = false;
            keyMonitorType = 1;
            PTTStartListening(keyMonitorType);
            VA.SetBoolean("AVCS_RadioButtons_ON", false);
            VA.SetBoolean("AVCS_PTT_MODE_ON", false);
        }

        private bool CheckPTTkeyDownState(string profile)
        {
            for (int keyIndex = 1; keyIndex <= 6; keyIndex++)
            {
                string keyVarName = "AVCS_" + profile + "_PTTBUTTON_" + keyIndex;
                string reverseVarName = "AVCS_" + profile + "_PTTREVERSE_" + keyIndex;

                string keyDef = VA.GetText(keyVarName) ?? string.Empty;
                if (string.IsNullOrEmpty(keyDef))
                {
                    continue;
                }

                bool isReverse = string.Equals(VA.GetText(reverseVarName), "reverse", StringComparison.OrdinalIgnoreCase);

                // TRIGGER
                if (keyDef.EndsWith("TRIGGER", StringComparison.OrdinalIgnoreCase))
                {
                    string valueStr = VA.ParseTokens("{" + keyDef + "}") ?? string.Empty;
                    int trigValue;
                    if (int.TryParse(valueStr, out trigValue))
                    {
                        if ((isReverse && trigValue < 20) || (!isReverse && trigValue > 20))
                        {
                            return true;
                        }
                    }
                }
                // POV
                else if (keyDef.Contains("POV"))
                {
                    var parts = keyDef.Split(':');
                    if (parts.Length == 2)
                    {
                        string povVar = parts[0];
                        string povDir = parts[1];
                        string valueStr = VA.ParseTokens("{" + povVar + "}") ?? string.Empty;
                        if ((isReverse && valueStr.Equals("CENTER")) ||
                            (!isReverse && valueStr.Equals(povDir) && !valueStr.Equals("CENTER")))
                        {
                            return true;
                        }
                    }
                }
                // Normal button/joystick/mouse
                else
                {
                    string valueStr = VA.ParseTokens("{" + keyDef + "}") ?? string.Empty;
                    if ((isReverse && valueStr.Equals("0")) ||
                        (!isReverse && valueStr.Equals("1")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void PTTStartListening(int type)
        {
            if (type == 0)
            {
                return;
            }

            var isEnabled = VA.State.GetListeningEnabled();
            if (!isEnabled)
            {
                Thread.Sleep(150);
                VA.State.SetListeningEnabled(true);
            }
        }

        private void PTTStopListening(int type)
        {
            if (type == 0)
            {
                return;
            }

            var isEnabled = VA.State.GetListeningEnabled();
            if (isEnabled)
            {
                Thread.Sleep(500);
                VA.State.SetListeningEnabled(false);
            }
        }

        private void SendDebugMessage(string text, int color)
        {
            int debugCount = 0;
            if (VA.GetText("AVCS_Debug_QMSG") != null)
            {
                debugCount = VA.GetInt("AVCS_Debug_QMSG") ?? 0;
            }
            debugCount++;
            VA.SetText("AVCS_Debug_TMSG_" + debugCount.ToString(), text);
            VA.SetInt("AVCS_Debug_WCLR_" + debugCount, color);
            VA.SetInt("AVCS_Debug_QMSG", debugCount);
            try
            {
                VA.Command.Execute("f_core_debug -log", true);
            }
            catch
            {
                VA.WriteToLog("AVCS ERROR: PTTGET02 - f_core_debug null - restart VoiceAttack or create bug report", "red");
            }
        }
    }

}
