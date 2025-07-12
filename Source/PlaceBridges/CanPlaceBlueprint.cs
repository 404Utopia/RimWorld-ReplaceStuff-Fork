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
			LocalVariableInfo posInfo = method.GetMethodBody().LocalVariables.First(lv => lv.LocalType == typeof(IntVec3));
			
			// RimWorld 1.6 now uses GridsUtility.GetAffordances() instead of TerrainDef.affordances
			MethodInfo getAffordancesMethod = AccessTools.Method(typeof(Verse.GridsUtility), nameof(Verse.GridsUtility.GetAffordances));

			bool firstOnly = true;
			var instList = instructions.ToList();
			for(int i=0;i<instList.Count();i++)
			{
				var inst = instList[i];
				// Look for the GetAffordances() method call instead of field access
				if(inst.Calls(getAffordancesMethod) && firstOnly)
				{
					firstOnly = false;

					// RimWorld 1.6 pattern:
					// IL_xxxx: ldloc.?      // IntVec3 item
					// IL_xxxx: ldarg.2      // Map map  
					// IL_xxxx: call         class [mscorlib]System.Collections.Generic.List`1<class Verse.TerrainAffordanceDef> Verse.GridsUtility::GetAffordances(valuetype Verse.IntVec3, class Verse.Map)
					// IL_xxxx: ldloc.0      // terrainAffordanceNeed
					// IL_xxxx: callvirt     instance bool class [mscorlib]System.Collections.Generic.List`1<class Verse.TerrainAffordanceDef>::Contains(!0/*class Verse.TerrainAffordanceDef*/)
					// IL_xxxx: brtrue.s     IL_xxxx

					//Skip GetAffordances call, load terrainAffordanceNeed
					i++;
					yield return instList[i];

					//skip call to Contains
					i++;

					//replace with TerrainOrBridgesCanDo (below)
					yield return new CodeInstruction(OpCodes.Ldarg_0);//entDef
					yield return new CodeInstruction(OpCodes.Ldloc, posInfo.LocalIndex);//IntVec3 pos
					yield return new CodeInstruction(OpCodes.Ldarg_2);//Map
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CanPlaceBlueprint), nameof(TerrainOrBridgesCanDo)));

					//and it'll continue with the brtrue
				}
				else
					yield return inst;
			}
		}

		public static bool TerrainOrBridgesCanDo(TerrainDef tDef, TerrainAffordanceDef neededDef, BuildableDef def, IntVec3 pos, Map map)
		{
			if (neededDef == null || def == null || map == null) return false;
			
			// RimWorld 1.6: Use GetAffordances() which checks foundation terrain first
			if (pos.GetAffordances(map)?.Contains(neededDef) == true)
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
			TerrainDef currentTerrain = map.terrainGrid.TerrainAt(pos);
			if (DesignatorContext.designating && BridgelikeTerrain.FindBridgeFor(currentTerrain, neededDef, map) != null)
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
