# Replace Stuff (Continued)

[![RimWorld](https://img.shields.io/badge/RimWorld-1.6-blue.svg)](https://rimworldgame.com/)
[![GitHub release](https://img.shields.io/github/v/release/404Utopia/RimWorld-ReplaceStuff-Fork)](https://github.com/404Utopia/RimWorld-ReplaceStuff-Fork/releases)

A continuation of **Uuugggg's** original "Replace Stuff" mod for RimWorld 1.6.

**Place buildings anywhere, on top of existing things, and the game will handle it. NOT dead for 1.6!**

## What This Mod Does

Replace Stuff (Continued) allows you to place buildings anywhere you want - on top of existing things, over rocks, water, or undiscovered areas - and the game will intelligently handle the construction process.

### Core Features
- **Replace building materials**: Replace a wooden wall with slate wall without deconstructing
- **Upgrade buildings**: Doors to autodoors, beds to bigger beds, workbenches to electric versions  
- **Build over obstacles**: Rocks will be mined away, bridges built over water automatically
- **Preserve settings**: Destroyed buildings that are auto-rebuilt remember their settings
- **Over-wall coolers**: Install coolers without breaking walls
- **Works with 1.6 blueprints**: Enhanced functionality beyond vanilla 1.6 blueprint system

### Why This is Better Than Vanilla 1.6

While RimWorld 1.6 added blueprints over walls, it doesn't do many of the advanced features this mod provides:
- 1.6 doesn't make replacement a single job
- 1.6 doesn't preserve settings of replaced beds/workbenches  
- 1.6 doesn't handle building over rocks/water automatically
- 1.6 doesn't include over-wall coolers
- 1.6 doesn't remember settings when buildings are auto-rebuilt

## Detailed Features

### Replace Stuff
Use the new Replace tool in the "Architect/Structure" menu to replace any building's materials. Or place a building with new stuff over an old building.

Benefits of replacement vs deconstruction:
- Upgrading walls doesn't break the room, keeps freezers cold
- Replacing workbenches preserves the bill list
- Replacing beds preserves their owner
- Work needed is same as normal deconstruction + construction
- Materials needed is just the stuff being replaced
- Get back 75% of replaced materials (normal deconstruction rate)

### Upgrade Buildings
Build new buildings over old ones and they'll keep the old one in place until construction is done:
- Walls, doors, auto-doors
- Beds to other beds
- Coolers to other coolers  
- Fueled stove/tailoring bench to electric versions
- Tables and workbenches
- Supports modded buildings like RimFridges and Fences mod

### Auto-Rebuild Settings Memory
Buildings remember their settings when auto-rebuilt:
- Work tables remember bills
- Coolers/heaters remember temperature settings
- Hydroponics remember plant selection
- Storage buildings remember filter settings

### Build Over Obstacles
- **Rocks**: Place blueprints over mountain rock, miners will clear it automatically
- **Water**: Place blueprints over water, bridges will be built automatically
- **Fog of war**: Build in undiscovered areas (invalid blueprints canceled after revealing)
- **Toggle available**: Bottom-right screen toggle to disable rock mining when placing blueprints

### Over-Wall Coolers
- Behave exactly like normal coolers but don't act as walls
- Must be placed on top of walls (doesn't break walls)
- Includes double-width over-wall coolers for thick freezer walls
- Can place even if hot/cold tiles are blocked (remove blockage later)
- Settings option to hide vanilla or over-wall coolers from build menu

### Minor Improvements
- Pawns don't block construction
- Corner walls are buildable from inside
- Smarter resource delivery to blocked construction sites

## Requirements

- **RimWorld 1.6**
- **[Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)** (required dependency)

## Installation

### Steam Workshop
*Coming soon - will be published to Steam Workshop once fully tested*

### Manual Installation
1. Download the latest release from the [Releases page](https://github.com/404Utopia/RimWorld-ReplaceStuff-Fork/releases)
2. Extract the ZIP file to your RimWorld Mods folder:
   - **Windows**: `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\`
   - **Mac**: `~/Library/Application Support/Steam/steamapps/common/RimWorld/Mods/`
   - **Linux**: `~/.steam/steam/steamapps/common/RimWorld/Mods/`
3. Enable the mod in your RimWorld mod list
4. Make sure Harmony is loaded before this mod

## How to Use

1. Use the Replace tool in Architect/Structure menu, OR
2. Simply place any building over an existing one with different materials
3. The game will automatically handle mining, bridge building, and replacement
4. Buildings under construction generally can't be used until replacement is complete

## Compatibility Notes

- Works with existing saves (no new save required)
- Remove any active replacement jobs before uninstalling
- Compatible with modded buildings, bridges, and workbenches
- Buildings with quality get new quality level when replaced
- Replacement jobs can fail like normal construction (loses 50% materials)

## Development Status

**Important Notice**: I (404Utopia) am a **new developer** who honestly has **no idea what I'm doing**. I simply love this mod and wanted to see it continue working in RimWorld 1.6 when I noticed it hadn't been updated.

This is my attempt to revive and maintain a mod that I found incredibly useful. I'm learning as I go and doing my best to:
- Keep the original functionality intact
- Fix compatibility issues with RimWorld 1.6
- Maintain code quality (though I'm still learning!)

### What's Been Updated for 1.6
- Fixed compilation errors and API changes
- Updated logging system for new RimWorld API
- Ensured compatibility with RimWorld 1.6's blueprint system
- Updated build system and project structure
- Added automated releases via GitHub Actions

## Credits and Attribution

**All credit for the original concept, design, and implementation goes to [Uuugggg](https://github.com/alextd).** This mod is a continuation of their excellent work.

- **Original Author**: [Uuugggg](https://github.com/alextd)
- **Original Mod**: [Replace Stuff](https://github.com/alextd/RimWorld-ReplaceStuff)

## Note to the Original Author

**To Uuugggg**: If you would prefer that this continuation be removed or if you plan to update the original mod yourself, please reach out and I will **immediately take this down**. I have tremendous respect for your work and don't want to step on any toes. This was created purely out of love for the mod and a desire to keep it alive for the community.

## Bug Reports and Support

Since I'm new to RimWorld modding, there may be bugs or issues I've missed. If you encounter any problems:

1. Check the [Issues page](https://github.com/404Utopia/RimWorld-ReplaceStuff-Fork/issues) to see if it's already reported
2. If not, please create a new issue with:
   - Your RimWorld version
   - Your mod list
   - Steps to reproduce the problem
   - Any error messages from the dev console

## Contributing

I welcome contributions from more experienced modders! If you see something that could be improved or have suggestions for a newcomer, please:
- Open an issue for discussion
- Submit a pull request with improvements
- Help with testing and bug reports

---

*Made with love for the RimWorld community. Special thanks to Uuugggg for creating such an essential mod.*