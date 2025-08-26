Namespace AVCS4_BMS_LocalizeDecimalsText
	Imports Microsoft.VisualBasic
	Imports System
	Imports System.Globalization

	'Required Referenced Assemblies V1 and V2:
	'Microsoft.VisualBasic.dll;System.dll

	Public Class VAInline
		Dim decimalSeparator As String = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
		Dim activeProfile As String = "BMS"
		Dim timingChoices As String = "0.10;0.07;0.06;0.05;0.04;0.03;0.02"
		Dim timeHeld As String = "0.05"
		Dim timeBetween As String = "0.05"

		'Recasts text variables containing decimal choices with current culture decimal separators
		Public Sub Main()

			If Not (decimalSeparator.Equals(".")) Then
				If ((VA.GetText("AVCS_ACTIVE_PROFILE")) IsNot Nothing) Then
					activeProfile = VA.GetText("AVCS_ACTIVE_PROFILE")
				End If

				If ((VA.GetText("AVCS_" + activeProfile + "_KEYPRESS_TIMING_CHOICES")) IsNot Nothing) Then
					timingChoices = VA.GetText("AVCS_" + activeProfile + "_KEYPRESS_TIMING_CHOICES")
				End If

				If ((VA.GetDecimal("AVCS_" + activeProfile + "_TimeKeyIsHeldDown")) IsNot Nothing) Then
					timeHeld = VA.GetDecimal("AVCS_" + activeProfile + "_TimeKeyIsHeldDown").ToString()
				End If

				If ((VA.GetDecimal("AVCS_" + activeProfile + "_TimeBetweenKeys")) IsNot Nothing) Then
					timeBetween = VA.GetDecimal("AVCS_" + activeProfile + "_TimeBetweenKeys").ToString()
				End If

				VA.SetText("AVCS_" + activeProfile + "_KEYPRESS_TIMING_CHOICES", timingChoices.Replace(".", decimalSeparator))

				Dim decimalValue As Decimal
				Dim decStrValue As String = timeHeld.Replace(".", decimalSeparator)
				If Decimal.TryParse(decStrValue, NumberStyles.Any, CultureInfo.CurrentCulture, decimalValue) Then
					VA.SetDecimal("AVCS_" + activeProfile + "_TimeKeyIsHeldDown", decimalValue)
				End If

				decStrValue = timeBetween.Replace(".", decimalSeparator)
				If Decimal.TryParse(decStrValue, NumberStyles.Any, CultureInfo.CurrentCulture, decimalValue) Then
					VA.SetDecimal("AVCS_" + activeProfile + "_TimeBetweenKeys", decimalValue)
				End If

			End If

		End Sub

	End Class
End Namespace