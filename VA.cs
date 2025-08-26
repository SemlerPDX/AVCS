/// <summary>
/// A simple simulation of the inline functions dynamic VA object.
/// </summary>
public static class VA
{
    public class State
    {
        public static bool GetListeningEnabled() => true; // Simulate Get Listening state
        public static void SetListeningEnabled(bool isListening = false) { } // Simulate Set Listening state
    }
    public class Command
    {
        //simulate execute
        public static void Execute(string commandName, bool WaitForReturn = false, bool AsSubcommand = false, string PassedText = "")
        {
            // Simulate executing a VoiceAttack command
        }
    }

    public class VAVersion
    {
        public static int Major => 1; // Simulate Get VoiceAttack major version
    }

    public class Profile
    {
        public static void Reset()
        {
            // Simulate resetting the VoiceAttack profile
        }
    }

    public static int? GetInt(string key)
    {
        // Simulate getting an integer from VoiceAttack
        return 23;
    }

    public static decimal? GetDecimal(string key)
    {
        // Simulate getting a decimal from VoiceAttack
        return 42.23M;
    }

    public static string? GetText(string key)
    {
        // Simulate getting text from VoiceAttack
        return "SampleText";
    }

    public static bool? GetBoolean(string key)
    {
        // Simulate getting a boolean from VoiceAttack
        return true;
    }

    public static void SetInt(string key, int? value)
    {
        // Simulate setting integer in VoiceAttack
    }

    public static void SetDecimal(string key, decimal? value)
    {
        // Simulate setting decimal in VoiceAttack
    }

    public static void SetText(string key, string? value)
    {
        // Simulate setting text in VoiceAttack
    }

    public static void SetBoolean(string key, bool? value)
    {
        // Simulate setting a boolean in VoiceAttack
    }

    public static void WriteToLog(string message, string color = "blank")
    {
        // Simulate writing to VoiceAttack log
    }

    public static void ClearLog()
    {
        // Simulate clearing text in VoiceAttack
    }

    public static string[] ExtractPhrases(string text)
    {
        // Simulate extracting phrases from a string
        return text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string? ParseTokens(string tokens)
    {
        // Simulate parsing text tokens
        return string.Empty;
    }
}


/// <summary>
/// Instance-shaped shim that mirrors the VA surface for dynamic call sites.
/// </summary>
public sealed class VAShim
{
    public VAShim()
    {
        State = new StateShim();
        Command = new CommandShim();
        VAVersion = new VAVersionShim();
    }

    // ---- nested objects (mirror VoiceAttack shape) ----

    public StateShim State { get; }
    public CommandShim Command { get; }
    public VAVersionShim VAVersion { get; }

    public sealed class StateShim
    {
        public bool GetListeningEnabled()
        {
            return VA.State.GetListeningEnabled();
        }

        public void SetListeningEnabled(bool isListening = false)
        {
            VA.State.SetListeningEnabled(isListening);
        }
    }

    public sealed class CommandShim
    {
        public void Execute(string commandName, bool WaitForReturn = false, bool AsSubcommand = false, string PassedText = "")
        {
            VA.Command.Execute(commandName, WaitForReturn, AsSubcommand, PassedText);
        }
    }

    public sealed class VAVersionShim
    {
        public int Major
        {
            get { return VA.VAVersion.Major; }
        }
    }

    public int? GetInt(string key) { return VA.GetInt(key); }
    public decimal? GetDecimal(string key) { return VA.GetDecimal(key); }
    public string? GetText(string key) { return VA.GetText(key); }
    public bool? GetBoolean(string key) { return VA.GetBoolean(key); }

    public void SetInt(string key, int? value) { VA.SetInt(key, value); }
    public void SetDecimal(string key, decimal? value) { VA.SetDecimal(key, value); }
    public void SetText(string key, string? value) { VA.SetText(key, value); }
    public void SetBoolean(string key, bool? value) { VA.SetBoolean(key, value); }

    public void WriteToLog(string message, string color = "blank")
    {
        VA.WriteToLog(message, color);
    }

    public void ClearLog()
    {
        VA.ClearLog();
    }

    public string? ParseTokens(string tokens)
    {
        return VA.ParseTokens(tokens);
    }
}

/// <summary>
/// Static holder exposing a field named 'VA' that points to the instance shim.
/// The global using below will bring this field into scope as a simple-name 'VA'.
/// </summary>
public static class VAStatics
{
    public static dynamic VA = new VAShim();
}