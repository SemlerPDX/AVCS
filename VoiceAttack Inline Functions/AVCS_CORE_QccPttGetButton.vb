Namespace AVCS_CORE_QccPttGetButton
	' Push to Talk Button Inline-Function for VoiceAttack -  Get/Set any Keyboard/Mouse/Joystick Button as a PTT Button
	'  by SemlerPDX Jan2021/Oct2021
	'  VETERANS-GAMING.COM

	' V1 Required Referenced Assemblies:
	' System.dll;System.Core.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.ComponentModel.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll

	' V2 Required Referenced Assemblies:
	' System.dll;System.Core.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.ComponentModel.dll;System.ComponentModel.TypeConvertor.dll;System.Deployment.dll;System.Drawing.dll;System.Net.Http.dll;System.Windows.Forms.dll;System.Xml.dll;System.Xml.Linq.dll

	Imports Microsoft.VisualBasic
	Imports System
	Imports System.Collections
	Imports System.Collections.Generic
	Imports System.ComponentModel
	Imports System.ComponentModel.TypeConvertor
	Imports System.Threading
	Imports System.Threading.Tasks
	Imports System.Windows.Forms


	Public Class VAInline
		Dim mouseButtons() As String = {"STATE_LEFTMOUSEBUTTON", "STATE_RIGHTMOUSEBUTTON", "STATE_MIDDLEMOUSEBUTTON", "STATE_FORWARDMOUSEBUTTON", "STATE_BACKMOUSEBUTTON"}
		Dim gamepadTriggers() As String = {"LEFTTRIGGER", "RIGHTTRIGGER"}

		Dim buttonsAlreadySetList As New List(Of String)
		Dim buttonsAlwaysDownList As New List(Of String)
		Dim buttonsAlwaysDown As Boolean = False
		Dim buttonDown As String = ""
		Dim buttonCheck As String = ""
		Dim checkPOV As String = ""
		Dim buttonSet As Boolean = False
		Dim userReady As Boolean = False
		Dim counter As Integer = 0

		Dim keyCheck As String = ""
		Dim keyString As String = ""
		Dim keyCode() As String

		Dim activeProfile As String = "CORE"

		Dim debugCheck As Boolean
		Dim debugCount As Integer = 0


		Private Function SendDebugMessage(ByVal debugText As String, ByVal debugColor As Integer)
			'1=Blue - 2=Green - 3=Yellow - 4=Red - 5=Purple - 6=Blank - 7=Orange - 8=Black - 9=Gray - 10=Pink
			If ((VA.GetText("AVCS_Debug_QMSG")) IsNot Nothing) Then
				debugCount = VA.GetInt("AVCS_Debug_QMSG")
			End If
			debugCount += 1
			VA.SetText("AVCS_Debug_TMSG_" + debugCount.ToString(), debugText)
			VA.SetInt("AVCS_Debug_WCLR_" + debugCount.ToString(), debugColor)
			VA.SetInt("AVCS_Debug_QMSG", debugCount)
			vaProxy.Command.Execute("f_core_debug -log", True)
			debugCount = 0
		End Function

		Private Function BuildPTTkeysList()
			For keyIndex As Integer = 1 To 6
				'Check for PTT buttons already set
				If ((VA.GetText("AVCS_" + activeProfile + "_PTTBUTTON_" + keyIndex.ToString())) IsNot Nothing) Then
					buttonCheck = VA.GetText("AVCS_" + activeProfile + "_PTTBUTTON_" + keyIndex.ToString())
					buttonsAlreadySetList.Add(buttonCheck)
				End If
			Next
		End Function

		Private Function BuildAlwaysDownList()
			'Mouse Buttons Baseline State Loop
			If ((VA.ParseTokens("{STATE_ANYMOUSEDOWN}")).Equals("1")) Then
				For m As Integer = 0 To 4
					Dim mb As String = mouseButtons(m).ToString()
					If ((VA.ParseTokens("{" + mb + "}")).Equals("1")) Then
						buttonsAlwaysDownList.Add(mouseButtons(m))
						buttonsAlwaysDown = True
						If (debugCheck) Then
							SendDebugMessage("MOUSE BUTTON (" + mb + ") DISREGARED AS ALWAYS DOWN", 4)
						End If
					End If
				Next
			End If

			'Keyboard Key Baseline State Loop
			For k As Integer = 1 To 254
				If ((VA.ParseTokens("{STATE_ANYKEYDOWN}")).Equals("1")) Then
					If ((VA.ParseTokens("{STATE_KEYSTATE:" + k.ToString() + "}")).Equals("1")) Then
						buttonsAlwaysDownList.Add("STATE_KEYSTATE:" + k.ToString())
						buttonsAlwaysDown = True
						If (debugCheck) Then
							SendDebugMessage("KEYBOARD KEY (virtual-key code " + k.ToString() + ") DISREGARED AS ALWAYS DOWN", 4)
						End If
					End If
				End If
			Next

			If (VA.ParseTokens("{STATE_JOYSTICKANYENABLED}").Equals("1")) Then
				'Joystick Button Baseline State Loop
				For d As Integer = 1 To 4
					If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ENABLED}")).Equals("1")) Then

						'Joystick Buttons Baseline "Pressed" States
						If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ANYBUTTON}")).Equals("1")) Then
							For i As Integer = 1 To 128
								If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "BUTTON:" + i.ToString() + "}")).Equals("1")) Then
									buttonsAlwaysDownList.Add("STATE_JOYSTICK" + d.ToString() + "BUTTON:" + i.ToString())
									buttonsAlwaysDown = True
									If (debugCheck) Then
										SendDebugMessage("JOYSTICK " + d.ToString() + " BUTTON " + i.ToString() + " DISREGARED - POSSIBLE DUAL STAGE TRIGGER", 4)
									End If
								End If
							Next
							'Joystick POV Buttons Baseline States (4-way only)
							If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POVENABLED}")).Equals("1")) Then
								For i As Integer = 1 To 4
									If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POV" + i.ToString() + "TYPE}")).Equals("4")) Then
										'Loop though this Joystick POV 1-4
										If (Not ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POV" + i.ToString() + "}")).Equals("CENTER"))) Then
											checkPOV = VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POV" + i.ToString() + "}")
											buttonsAlwaysDownList.Add("STATE_JOYSTICK" + d.ToString() + "POV" + i.ToString() + ":" + checkPOV)
											buttonsAlwaysDown = True
											If (debugCheck) Then
												SendDebugMessage("JOYSTICK " + d.ToString() + " POV " + i.ToString() + ":" + checkPOV + " DISREGARED", 4)
											End If
										End If
									End If
								Next
							End If

						End If

						Dim isNumeric As Boolean = False
						Dim integerValue As Integer
						Dim numCheckStr As String

						'Joystick as Gamepad Baseline Trigger States (with 20 of 255 as 'deadzone')
						If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ISGAMEPAD}")).Equals("1")) Then
							For t As Integer = 0 To 1

								isNumeric = False
								numCheckStr = VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + gamepadTriggers(t) + "}")
								If Integer.TryParse(numCheckStr, integerValue) Then
									isNumeric = True
								End If

								If (isNumeric) Then
									If (integerValue > 20) Then
										buttonsAlwaysDownList.Add("STATE_JOYSTICK" + d.ToString() + gamepadTriggers(t))
										buttonsAlwaysDown = True
										If (debugCheck) Then
											SendDebugMessage("Always Down " + gamepadTriggers(t), 3)
										End If
									End If
								End If
							Next
						End If

					End If
				Next
			End If
		End Function


		Private Function GetReverseMouseButton(ByRef buttonSet As Boolean)
			'Check Always Down Mouse Button for "Unpressed" state
			If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
				For Each buttonAlwaysDown As String In buttonsAlwaysDownList
					If (buttonAlwaysDown.EndsWith("MOUSEBUTTON")) Then
						If ((VA.ParseTokens("{" + buttonAlwaysDown + "}")).Equals("0")) Then
							buttonSet = True
							buttonDown = buttonAlwaysDown 'changed from buttonCheck Sept2021
							VA.SetBoolean("~avcs_ptt_reverse", True)
							If (debugCheck) Then
								SendDebugMessage("(DST)- REVERSE MOUSE BUTTON (" + buttonDown + ") SET", 2)
							End If
						End If
					End If
				Next
			End If
		End Function


		Private Function GetMouseButton(ByRef buttonSet As Boolean)
			'Mouse Button Current States
			If ((VA.ParseTokens("{STATE_ANYMOUSEDOWN}")).Equals("1")) Then
				For m As Integer = 0 To 4
					If (buttonSet = False) Then
						If ((VA.ParseTokens("{" + mouseButtons(m) + "}")).Equals("1")) Then
							buttonCheck = mouseButtons(m)
							If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
								If (Not (buttonsAlwaysDownList.Contains(buttonCheck))) Then
									buttonSet = True
									buttonDown = buttonCheck
									If (debugCheck) Then
										SendDebugMessage("(DST)- MOUSE BUTTON (" + mouseButtons(m) + ") SET", 2)
									End If
								Else
									If (debugCheck) Then
										SendDebugMessage("MOUSE BUTTON (" + mouseButtons(m) + ") DISREGARED - DISCOVERED ON LIST", 4)
									End If
								End If
							Else
								buttonSet = True
								buttonDown = buttonCheck
								If (debugCheck) Then
									SendDebugMessage("MOUSE BUTTON (" + mouseButtons(m) + ") SET", 2)
								End If
							End If
						End If
					End If
				Next
			End If
		End Function


		Private Function GetReverseKeyboardKey(ByRef buttonSet As Boolean)
			'Check Always Down Keyboard Key for "Unpressed" state
			If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
				For Each buttonAlwaysDown As String In buttonsAlwaysDownList
					If (buttonSet = False) Then
						If (buttonAlwaysDown.StartsWith("STATE_KEYSTATE")) Then
							If ((VA.ParseTokens("{" + buttonAlwaysDown + "}")).Equals("0")) Then
								buttonSet = True
								buttonDown = buttonAlwaysDown
								VA.SetBoolean("~avcs_ptt_reverse", True)
								If (debugCheck) Then
									SendDebugMessage("(DST)- REVERSE KEYBOARD KEY (" + buttonAlwaysDown + ") SET", 2)
								End If
							End If
						End If
					End If
				Next
			End If
		End Function


		Private Function GetKeyboardKey(ByRef buttonSet As Boolean)
			'Keyboard Keys Current State Loop
			If ((VA.ParseTokens("{STATE_ANYKEYDOWN}")).Equals("1")) Then
				For k As Integer = 1 To 254
					If (buttonSet = False) Then
						If ((VA.ParseTokens("{STATE_KEYSTATE:" + k.ToString() + "}")).Equals("1")) Then
							buttonCheck = "STATE_KEYSTATE:" + k.ToString()
							If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
								If (Not (buttonsAlwaysDownList.Contains(buttonCheck))) Then
									buttonSet = True
									buttonDown = buttonCheck
									If (debugCheck) Then
										SendDebugMessage("(DST)- KEYBOARD KEY (virtual-key code " + k.ToString() + ") SET", 2)
									End If
								Else
									If (debugCheck) Then
										SendDebugMessage("KEYBOARD KEY (virtual-key code " + k.ToString() + ") DISREGARED - DISCOVERED ON LIST", 4)
									End If
								End If
							Else
								buttonSet = True
								buttonDown = buttonCheck
								If (debugCheck) Then
									SendDebugMessage("KEYBOARD KEY (virtual-key code " + k.ToString() + ") SET", 2)
								End If
							End If
						End If
					End If
				Next
			End If
		End Function


		'Check Always Down Style Joystick Buttons for "Unpressed" state 
		Private Function GetReverseJoystickbutton(ByRef buttonSet As Boolean)
			For d As Integer = 1 To 4
				If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ENABLED}")).Equals("1")) Then
					If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
						For Each buttonAlwaysDown As String In buttonsAlwaysDownList
							If (buttonSet = False) Then
								If (buttonAlwaysDown.StartsWith("STATE_JOYSTICK" + d.ToString())) Then
									If ((VA.ParseTokens("{" + buttonAlwaysDown + "}")).Equals("0")) Then
										buttonSet = True
										buttonDown = buttonAlwaysDown 'changed from buttonCheck Sept2021
										VA.SetBoolean("~avcs_ptt_reverse", True)
										If (debugCheck) Then
											SendDebugMessage("(DST)- REVERSE JOYSTICK " + d.ToString() + " BUTTON (" + buttonAlwaysDown + ") HAS BEEN SET", 2)
										End If
									End If
								End If
							End If
						Next
					End If
				End If
			Next
		End Function


		'Standard Button Current "Pressed" state 
		Private Function GetJoystickButton(ByRef buttonSet As Boolean)
			For d As Integer = 1 To 4
				If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ENABLED}")).Equals("1")) Then
					If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ANYBUTTON}")).Equals("1")) Then
						For i As Integer = 1 To 128
							If (buttonSet = False) Then
								If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "BUTTON:" + i.ToString() + "}")).Equals("1")) Then
									buttonCheck = "STATE_JOYSTICK" + d.ToString() + "BUTTON:" + i.ToString()
									If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
										If (Not (buttonsAlwaysDownList.Contains(buttonCheck))) Then
											buttonSet = True
											buttonDown = buttonCheck
											If (debugCheck) Then
												SendDebugMessage("(DST)- JOYSTICK " + d.ToString() + " BUTTON " + i.ToString() + " SET", 2)
											End If
										Else
											If (debugCheck) Then
												SendDebugMessage("JOYSTICK " + d.ToString() + " BUTTON " + i.ToString() + " DISREGARED - DISCOVERED ON LIST", 4)
											End If
										End If
									Else
										buttonSet = True
										buttonDown = buttonCheck
										If (debugCheck) Then
											SendDebugMessage("JOYSTICK " + d.ToString() + " BUTTON " + i.ToString() + " SET", 2)
										End If
									End If
								End If
							End If
						Next
					End If
				End If
			Next
		End Function

		'Check Always Down POV for "Unpressed" state
		Private Function GetReverseJoystickPOV(ByRef buttonSet As Boolean)
			For d As Integer = 1 To 4
				If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POVENABLED}")).Equals("1")) Then
					For i As Integer = 1 To 4
						checkPOV = "STATE_JOYSTICK" + d.ToString() + "POV" + i.toString()
						If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
							If (buttonsAlwaysDownList.Contains(checkPOV)) Then
								For Each buttonAlwaysDown As String In buttonsAlwaysDownList
									If (buttonSet = False) And (buttonAlwaysDown.StartsWith(checkPOV)) Then
										If ((VA.ParseTokens("{" + checkPOV + "}")).Equals("CENTER")) Then
											buttonSet = True
											buttonDown = buttonAlwaysDown
											VA.SetBoolean("~avcs_ptt_reverse", True)
											If (debugCheck) Then
												SendDebugMessage("(DST)- REVERSE JOYSTICK " + d.ToString() + " POV (" + buttonAlwaysDown + ") HAS BEEN SET", 2)
											End If
										End If
									End If
								Next
							End If
						End If
					Next
				End If
			Next
		End Function

		'Check Joystick POV for "Pressed" state
		Private Function GetJoystickPOV(ByRef buttonSet As Boolean)
			For d As Integer = 1 To 4
				If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POVENABLED}")).Equals("1")) Then
					For i As Integer = 1 To 4
						If (buttonSet = False) Then
							If ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POV" + i.toString() + "TYPE}")).Equals("4")) Then
								checkPOV = VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "POV" + i.toString() + "}")
								If (Not (checkPOV.Equals("CENTER"))) Then
									buttonCheck = "STATE_JOYSTICK" + d.ToString() + "POV" + i.toString() + ":" + checkPOV
									If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
										If (Not (buttonsAlwaysDownList.Contains(buttonCheck))) Then
											buttonSet = True
											buttonDown = buttonCheck
											If (debugCheck) Then
												SendDebugMessage("JOYSTICK " + d.ToString() + "POV" + i.ToString() + ":" + checkPOV + " SET", 2)
											End If
										Else
											If (debugCheck) Then
												SendDebugMessage("JOYSTICK " + d.ToString() + "POV" + i.ToString() + ":" + checkPOV + " DISREGARED - DISCOVERED ON LIST", 4)
											End If
										End If
									Else
										buttonSet = True
										buttonDown = buttonCheck
										If (debugCheck) Then
											SendDebugMessage("JOYSTICK " + d.ToString() + "POV" + i.ToString() + ":" + checkPOV + " SET", 2)
										End If
									End If
								End If
							End If
						End If
					Next
				End If
			Next
		End Function

		'---------------------------------------------------------------------------
		'---------------------------------------------------------------------------

		'Check Always Down Triggers for "Unpressed" state 
		Private Function GetReverseJoystickTrigger(ByRef buttonSet As Boolean)
			Dim isNumeric As Boolean = False
			Dim integerValue As Integer
			Dim numCheckStr As String

			For d As Integer = 1 To 4
				If (((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ENABLED}")).Equals("1")) And ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ISGAMEPAD}")).Equals("1"))) Then
					If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
						For Each buttonAlwaysDown As String In buttonsAlwaysDownList
							If (buttonSet = False) Then
								If (buttonAlwaysDown.EndsWith("TRIGGER")) Then

									isNumeric = False
									numCheckStr = VA.ParseTokens("{" + buttonAlwaysDown + "}")
									If Integer.TryParse(numCheckStr, integerValue) Then
										isNumeric = True
									End If

									If (isNumeric) Then
										If (integerValue < 20) Then
											buttonSet = True
											buttonDown = buttonCheck
											VA.SetBoolean("~avcs_ptt_reverse", True)
											If (debugCheck) Then
												SendDebugMessage("(DST)- REVERSE JOYSTICK GAMEPAD " + d.ToString() + " TRIGGER (" + buttonAlwaysDown + ") HAS BEEN SET", 2)
											End If
										End If
									End If
								End If
							End If
						Next
					End If
				End If
			Next
		End Function

		'Check Triggers for "Pressed" state 
		Private Function GetJoystickTrigger(ByRef buttonSet As Boolean)
			Dim isNumeric As Boolean = False
			Dim integerValue As Integer
			Dim numCheckStr As String

			For d As Integer = 1 To 4
				If (((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ENABLED}")).Equals("1")) And ((VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + "ISGAMEPAD}")).Equals("1"))) Then
					'Gamepad Triggers Current State (with 20 of 255 as 'deadzone')
					For t As Integer = 0 To 1
						If (buttonSet = False) Then

							isNumeric = False
							numCheckStr = VA.ParseTokens("{STATE_JOYSTICK" + d.ToString() + gamepadTriggers(t) + "}")
							If Integer.TryParse(numCheckStr, integerValue) Then
								isNumeric = True
							End If

							If (isNumeric) Then
								If (integerValue > 20) Then
									buttonCheck = "STATE_JOYSTICK" + d.ToString() + gamepadTriggers(t)
									If ((buttonsAlwaysDown) And (buttonsAlwaysDownList IsNot Nothing)) Then
										If (Not (buttonsAlwaysDownList.Contains(buttonCheck))) Then
											buttonSet = True
											buttonDown = buttonCheck
										Else
											If (debugCheck) Then
												SendDebugMessage("JOYSTICK GAMEPAD " + d.ToString() + gamepadTriggers(t) + " DISREGARED - DISCOVERED ON LIST", 4)
											End If
										End If
									Else
										buttonSet = True
										buttonDown = buttonCheck
										If (debugCheck) Then
											SendDebugMessage("JOYSTICK GAMEPAD " + d.ToString() + gamepadTriggers(t) + " SET", 2)
										End If
									End If
								End If
							End If
						End If
					Next
				End If
			Next
		End Function



		Public Sub Main()
			Dim isNumeric As Boolean = False
			Dim integerValue As Integer
			Dim numCheckStr As String

			Dim isAnyJoystickEnabled As Boolean = False
			If (VA.ParseTokens("{STATE_JOYSTICKANYENABLED}").Equals("1")) Then
				isAnyJoystickEnabled = True
			End If

			If (VA.GetBoolean("AVCS_Debug_ON") IsNot Nothing) Then
				debugCheck = VA.GetBoolean("AVCS_Debug_ON")
			End If

			If ((VA.GetText("AVCS_ACTIVE_PROFILE")) IsNot Nothing) Then
				activeProfile = VA.GetText("AVCS_ACTIVE_PROFILE")
			End If

			If (debugCheck) Then
				VA.ClearLog()
				SendDebugMessage("inline begin....", 3)
			End If


			'Get Button/Key Baseline States
			BuildPTTkeysList()
			BuildAlwaysDownList()
			VA.SetBoolean("~avcs_getting_input", True)

			If (debugCheck) Then
				SendDebugMessage("Always Down Array Complete", 3)
			End If

			'Begin a pause loop to wait for timeout or user input (key/button press or voice 'cancel' phrase)
			While (Not (userReady))
				Thread.CurrentThread.Sleep(1000)
				counter += 1

				'Check for Timeout or TTS User Prompt Instructions complete
				If ((VA.GetBoolean("~avcs_user_ready")) IsNot Nothing) Then
					userReady = VA.GetBoolean("~avcs_user_ready")
				End If
				If ((counter >= 15) Or (userReady)) Then
					If (userReady) Then
						VA.WriteToLog("== PLEASE PRESS ANY KEY OR BUTTON NOW ==", "green")
					Else
						VA.WriteToLog("cancelled - command timed out... TTS or primary function error", "red")
						If (debugCheck) Then
							SendDebugMessage("AVCS PTT SET cancelled - command timed out", 3)
						End If
						VA.SetBoolean("~avcs_getting_input", False)
						Exit Sub
					End If
				End If
			End While


			'Reset counter for Keypress/Button monitor
			counter = 0
			userReady = False
			VA.SetBoolean("~avcs_user_ready", False)


			'Begin a pause loop to wait for Joystick/Gamepad Input or spoken 'cancel/done' speech
			While (Not (userReady))
				Thread.CurrentThread.Sleep(75)
				counter += 1

				If (buttonSet = False) Then
					GetReverseMouseButton(buttonSet)
					GetMouseButton(buttonSet)
					GetReverseKeyboardKey(buttonSet)
					GetKeyboardKey(buttonSet)

					If (Not (isAnyJoystickEnabled)) Then
						Continue While
					End If

					GetReverseJoystickbutton(buttonSet)
					GetJoystickButton(buttonSet)
					GetReverseJoystickPOV(buttonSet)
					GetJoystickPOV(buttonSet)
					GetReverseJoystickTrigger(buttonSet)
					GetJoystickTrigger(buttonSet)
				End If

				'Check for Input, Timeout or TTS User Prompt Instructions complete
				If ((VA.GetBoolean("~avcs_user_ready")) IsNot Nothing) Then
					userReady = VA.GetBoolean("~avcs_user_ready")
				End If
				If ((counter >= 200) Or ((buttonSet) And (Not (String.IsNullOrEmpty(buttonDown)))) Or (userReady)) Then
					If ((buttonSet) And (Not (String.IsNullOrEmpty(buttonDown)))) Then
						If (buttonsAlreadySetList IsNot Nothing) Then
							If (buttonsAlreadySetList.Contains(buttonDown)) Then
								VA.SetBoolean("~avcs_getting_input", False)
								VA.SetText("~avcs_button_choice", "alreadyset")
								If (debugCheck) Then
									SendDebugMessage("ERROR - Existing PTT button has already been set to " + buttonDown, 4)
								End If
								Exit While
							End If
						End If
						VA.SetBoolean("~avcs_getting_input", False)
						VA.SetText("~avcs_button_choice", buttonDown)
						If (debugCheck) Then
							SendDebugMessage("PTT button has been set " + buttonDown, 2)
						End If
						Exit While
					ElseIf (userReady) Then
						VA.SetBoolean("~avcs_getting_input", False)
						VA.WriteToLog("command cancelled - user input not detected", "red")
						If (debugCheck) Then
							SendDebugMessage("AVCS PTT SET user input not detected", 4)
						End If
						Exit While
					ElseIf (counter >= 200) Then
						VA.SetBoolean("~avcs_getting_input", False)
						VA.WriteToLog("command timed out... user input not detected", "red")
						If (debugCheck) Then
							SendDebugMessage("AVCS PTT SET command timed out", 4)
						End If
						Exit While
					End If
				End If
			End While

			If ((buttonSet) And (Not (String.IsNullOrEmpty(buttonDown)))) Then
				If (buttonDown.StartsWith("STATE_KEYSTATE:")) Then
					keyCode = buttonDown.Split(":")

					isNumeric = False
					numCheckStr = keyCode(1)
					If Integer.TryParse(keyCode(1), integerValue) Then
						isNumeric = True
					End If


					If (isNumeric) Then

						'keyString = kc.ConvertToString(integerValue)
						'keyString = kc.ConvertTo(integerValue, GetType(String)).ToString()

						'Dim kc As New System.Windows.Forms.KeysConverter()
						'Dim s As String = kc.ConvertTo(Nothing, System.Globalization.CultureInfo.InvariantCulture, myKeys, GetType(String)).ToString()


						'keyString = s.Replace("Oem","")
						keyString = GetKeyName(integerValue)
						VA.WriteToLog("PTT Button Set to " + buttonDown + " == " + keyString, "green")
					End If
				Else
					VA.WriteToLog("PTT Button Set to " + buttonDown, "green")
				End If
			End If

		End Sub

		Function GetKeyName(keyCode As Integer) As String
			Dim keyName As String = [Enum].GetName(GetType(System.Windows.Forms.Keys), keyCode)
			If keyName Is Nothing Then Return keyCode.ToString()
			If keyName.StartsWith("Oem") Then keyName = keyName.Substring(3)
			If keyName.StartsWith("D") AndAlso keyName.Length = 2 AndAlso Char.IsDigit(keyName(1)) Then
				keyName = keyName.Substring(1)
			End If
			Return keyName
		End Function

	End Class

End Namespace