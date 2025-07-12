using System;
using System.Runtime.CompilerServices;

// Type forwarders for backward compatibility with saved settings
// This allows RimWorld to find the new classes when loading old save files
[assembly: TypeForwardedTo(typeof(Replace_Stuff_Continued.Settings))]
[assembly: TypeForwardedTo(typeof(Replace_Stuff_Continued.Mod))]

namespace Replace_Stuff
{
    // Legacy namespace aliases for backward compatibility
    [Obsolete("Use Replace_Stuff_Continued.Settings instead")]
    public class Settings : Replace_Stuff_Continued.Settings { }
    
    [Obsolete("Use Replace_Stuff_Continued.Mod instead")]
    public class Mod : Replace_Stuff_Continued.Mod { }
}