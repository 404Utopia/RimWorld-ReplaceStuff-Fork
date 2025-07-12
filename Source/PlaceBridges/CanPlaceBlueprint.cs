using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Replace_Stuff_Continued.PlaceBridges
{
	public static class PlaceBridges
	{
		public static TerrainDef GetNeededBridge(BuildableDef def, IntVec3 pos, Map map, ThingDef stuff)
		{
			if (!pos.InBounds(map)) return null;
			if (map?.terrainGrid == null) return null;
			
			TerrainAffordanceDef needed = def.GetTerrainAffordanceNeed(stuff);
			if (needed == null) return null;
			
			TerrainDef currentTerrain = map.terrainGrid.TerrainAt(pos);
			if (currentTerrain == null) return null;
			
			return BridgelikeTerrain.FindBridgeFor(currentTerrain, needed, map);
		}
	}

	[HarmonyPatch(typeof(GenConstruct), "CanBuildOnTerrain")]
	class CanPlaceBlueprint
	{
		//public static bool CanBuildOnTerrain(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, Thing thingToIgnore = null, ThingDef stuffDef = null)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
		{
			// Find local variables dynamically
			LocalVariableInfo itemInfo = method.GetMethodBody().LocalVariables.First(lv => lv.LocalType == typeof(IntVec3));
			LocalVariableInfo terrainAffordanceNeedInfo = method.GetMethodBody().LocalVariables.First(lv => lv.LocalType == typeof(TerrainAffordanceDef));
			
			// RimWorld 1.6 now uses GridsUtility.GetAffordances() instead of TerrainDef.affordances
			MethodInfo getAffordancesMethod = AccessTools.Method(typeof(Verse.GridsUtility), nameof(Verse.GridsUtility.GetAffordances));

			bool patched = false;
			var instList = instructions.ToList();
			for(int i=0; i < instList.Count - 2; i++)
			{
				var inst = instList[i];
				var nextInst = instList[i + 1];
				var nextNextInst = instList[i + 2];
				
				// Look for the exact pattern: GetAffordances call followed by terrainAffordanceNeed load followed by Contains call
				if(inst.Calls(getAffordancesMethod) && 
				   nextInst.IsLdloc() && 
				   nextNextInst.opcode == OpCodes.Callvirt && !patched)
				{
					patched = true;

					// Keep the GetAffordances call result on the stack
					yield return inst;
					
					// Skip the terrainAffordanceNeed load and Contains call, but get the terrainAffordanceNeed for our method
					i += 2;

					// Our method signature: TerrainOrBridgesCanDo(List<TerrainAffordanceDef> affordances, TerrainAffordanceDef neededDef, BuildableDef def, IntVec3 pos, Map map)
					// Stack currently has: affordances list from GetAffordances call
					yield return new CodeInstruction(OpCodes.Ldloc, terrainAffordanceNeedInfo.LocalIndex); // TerrainAffordanceDef needed
					yield return new CodeInstruction(OpCodes.Ldarg_0); // BuildableDef def
					yield return new CodeInstruction(OpCodes.Ldloc, itemInfo.LocalIndex); // IntVec3 pos
					yield return new CodeInstruction(OpCodes.Ldarg_2); // Map
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CanPlaceBlueprint), nameof(TerrainOrBridgesCanDo)));
				}
				else
					yield return inst;
			}
			
			// Return remaining instructions
			for(int i = instList.Count - 2; i < instList.Count; i++)
			{
				if(i >= 0 && i < instList.Count)
					yield return instList[i];
			}
		}

		public static bool TerrainOrBridgesCanDo(List<TerrainAffordanceDef> affordances, TerrainAffordanceDef neededDef, BuildableDef def, IntVec3 pos, Map map)
		{
			if (affordances == null || neededDef == null || def == null || map == null) return false;
			
			// First check if the affordances already contain what we need
			if (affordances.Contains(neededDef))
				return true;

			if (def is TerrainDef)
				return false;

			//Now it's gonna also check bridges:
			//Bridge blueprint there that will support this:
			//TODO isn't this redundant?
			if (pos.GetThingList(map).Any(t =>
				t.def.entityDefToBuild is TerrainDef bpTDef &&
				bpTDef.affordances?.Contains(neededDef) == true))
				return true;

			//Player not choosing to build and bridges possible: ok (elsewhere in code will place blueprints)
			TerrainDef currentTerrain = map.terrainGrid?.TerrainAt(pos);
			if (DesignatorContext.designating && currentTerrain != null && BridgelikeTerrain.FindBridgeFor(currentTerrain, neededDef, map) != null)
				return true;

			return false;
		}
	}

	//This should technically go inside Designator's DesignateSingleCell, but this is easier.
	[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.PlaceBlueprintForBuild))]
	class InterceptBlueprintPlaceBridgeFrame
	{
		//public static Blueprint_Build PlaceBlueprintForBuild(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff)
		public static void Prefix(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff)
		{
			if (sourceDef == null || map == null || faction != Faction.OfPlayer || sourceDef.IsBridgelike()) return;

			try
			{
				foreach (IntVec3 pos in GenAdj.CellsOccupiedBy(center, rotation, sourceDef.Size))
					EnsureBridge.PlaceBridgeIfNeeded(sourceDef, pos, map, rotation, faction, stuff);
			}
			catch (System.Exception ex)
			{
				Verse.Log.Warning($"Replace Stuff (Continued) error in bridge placement prefix: {ex.Message}");
			}
		}
	}

	public class EnsureBridge
	{
		public static void PlaceBridgeIfNeeded(BuildableDef sourceDef, IntVec3 pos, Map map, Rot4 rotation, Faction faction, ThingDef stuff)
		{
			if (sourceDef == null || map == null || !pos.InBounds(map)) return;
			
			TerrainDef bridgeDef = PlaceBridges.GetNeededBridge(sourceDef, pos, map, stuff);

			if (bridgeDef == null)
				return;

			// Check if there's already a blueprint for this bridge being built
			var thingList = pos.GetThingList(map);
			if (thingList?.Any(t => t?.def?.entityDefToBuild == bridgeDef) == true)
				return;//Already building!

			try
			{
				Log.Message($"Replace Stuff (Continued) placing {bridgeDef} for {sourceDef}({sourceDef.GetTerrainAffordanceNeed(stuff)}) on {map.terrainGrid?.TerrainAt(pos)}");
				GenConstruct.PlaceBlueprintForBuild(bridgeDef, pos, map, rotation, faction, null);//Are there bridge precepts/styles?...
			}
			catch (System.Exception ex)
			{
				Verse.Log.Warning($"Replace Stuff (Continued) failed to place bridge {bridgeDef} at {pos}: {ex.Message}");
			}
		}
	}

	[HarmonyPatch(typeof(GenConstruct), "PlaceBlueprintForInstall")]
	class InterceptBlueprintPlaceBridgeFrame_Install
	{
		//public static Blueprint_Install PlaceBlueprintForInstall(MinifiedThing itemToInstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		public static void Prefix(MinifiedThing itemToInstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		{
			ThingDef def = itemToInstall.InnerThing.def;
			InterceptBlueprintPlaceBridgeFrame.Prefix(def, center, map, rotation, faction, itemToInstall.InnerThing.Stuff);
		}
	}

	[HarmonyPatch(typeof(GenConstruct), "PlaceBlueprintForReinstall")]
	class InterceptBlueprintPlaceBridgeFrame_Reinstall
	{
		//public static Blueprint_Install PlaceBlueprintForReinstall(Building buildingToReinstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		public static void Prefix(Building buildingToReinstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		{
			ThingDef def = buildingToReinstall.def;
			InterceptBlueprintPlaceBridgeFrame.Prefix(def, center, map, rotation, faction, buildingToReinstall.Stuff);
		}
	}


	[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
	public static class DontWipeBridgeBlueprints
	{
		//public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
		public static bool Prefix(BuildableDef oldEntDef, bool __result)
		{
			if (oldEntDef is ThingDef tdef && (GenConstruct.BuiltDefOf(tdef) ?? oldEntDef).IsBridgelike())
			{
				__result = false;
				return false;
			}
			return true;
		}
	}
}
