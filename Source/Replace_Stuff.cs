using System.Reflection;
using System.Linq;
using Verse;
using UnityEngine;
using HarmonyLib;
using RimWorld;

namespace Replace_Stuff
{
	public class Mod : Verse.Mod
	{
		public static Settings settings;
		public Mod(ModContentPack content) : base(content)
		{
			try
			{
				// initialize settings
				settings = GetSettings<Settings>();
#if DEBUG
				Harmony.DEBUG = true;
#endif
				new Harmony("404Utopia.rimworld.Replace_Stuff.main").PatchAll();
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Replace Stuff failed to initialize: {ex}");
				throw;
			}
		}

		[StaticConstructorOnStartup]
		public static class ModStartup
		{
			static ModStartup()
			{
				try
				{
					//Hugslibs-added defs will be queued up before this Static Constructor
					//So queue replace frame generation after that
					LongEventHandler.QueueLongEvent(() => ThingDefGenerator_ReplaceFrame.AddReplaceFrames(), null, true, null);
					LongEventHandler.QueueLongEvent(() => CoolersOverWalls.DesignatorBuildDropdownStuffFix.SanityCheck(), null, true, null);
				}
				catch (System.Exception ex)
				{
					Debug.LogError($"Replace Stuff failed during startup: {ex}");
				}
			}
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "TD.ReplaceStuff".Translate();
		}
	}
}