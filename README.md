# Hospitality Pay-to-Access Doors

A RimWorld mod that adds configurable payment requirements to doors, perfect for creating premium areas in your hospitality setup.

## Features

### 🚪 Pay-to-Access System
- Transform any door into a pay gate that charges guests for entry
- Configurable cost from 0 to 999 silver per door
- Visual feedback when guests can't afford passage

### 💰 Flexible Payment Modes
- **Pay Once**: Guests pay once and get permanent access to that door
- **Pay Every Time**: Charge guests for each door usage (ongoing revenue)

### 🎯 Smart Exemptions
Automatically exempts from payment:
- Your colonists and their pets
- Prisoners and slaves
- Animals (wild and tamed)
- **Robots (Misc. Robots++ and other robot mods)
- Anyone you don't want to charge

### 🤖 Robot Support
- **Auto-Detection**: Automatically identifies robots from popular mods
- **Misc. Robots++ Compatible**: Full support for all robot types
- **Configurable**: Option to charge robots if desired
- **Safe Implementation**: Uses reflection to avoid mod dependency issues

### 💎 Visual Payment System
- **Silver Collection**: Payments are dropped as physical silver at the door
- **Smart Placement**: Silver appears in accessible locations near doors
- **Lifetime Earnings**: Track total income from each door
- **Real-time Feedback**: See payments accumulate visually

### 🎮 Easy-to-Use Interface
- **Single-Click Configuration**: One button opens a comprehensive settings window
- **Multi-Selection**: Configure multiple doors at once
- **Visual Settings Panel**: Clear checkboxes and sliders for all options
- **Real-time Updates**: Changes apply instantly to selected doors

## Installation

1. Download the mod files
2. Place in your RimWorld `Mods` folder
3. Enable in the mod list (requires Hospitality mod as dependency)
4. Start a new game or load an existing save

## Usage

### Quick Start
1. **Select Door(s)**: Click on any door (or hold Shift to select multiple doors)
2. **Open Configuration**: Click the "Paid Door" button in the bottom gizmo panel
3. **Set Cost**: Enter desired silver cost or use quick-set buttons
4. **Choose Mode**: Select "Pay Once" or "Pay Every Time" payment mode
5. **Configure Exemptions**: Check/uncheck exemption boxes as desired
6. **Apply**: Settings take effect immediately

### Basic Setup
1. Select any door(s) on your map
2. Click the "Paid Door" gizmo button that appears in the bottom panel
3. Configure your settings in the popup window:
   - Set entry cost (0-999 silver)
   - Choose payment mode (Pay Once vs Pay Every Time)
   - Configure exemptions for different pawn types

### Advanced Configuration
The configuration window provides comprehensive control over:
- **Entry Costs**: Text field input with quick-set buttons (5, 10, 15, 20, 25 silver)
- **Payment Modes**: 
  - "Pay Once" - Guests pay once and get permanent access
  - "Pay Every Time" - Charge guests for each door usage
- **Exemption Settings**: Individual checkboxes for:
  - Colonists (including player-controlled pawns)
  - Allied factions
  - Prisoners
  - Robots (Misc. Robots++ and other robot mods)
  - Animals (always exempt, non-configurable)
- **Management Tools**:
  - Clear paid guests list (for Pay Once mode)
  - Reset all settings to defaults
  - View lifetime earnings per door
  - Reset earnings counters

### Payment Behavior
- **Pay Once Mode**: Guest pays the set amount and gains permanent access
- **Pay Every Time Mode**: Guest must pay each time they want to pass through
- **Silver Collection**: Payments are physically dropped at the door for collection
- **Robot Exemption**: Robots are exempt by default but can be configured to pay
- Guests without enough silver will be blocked from passing
- All payments are deducted from the guest's inventory

## Technical Details

### Architecture
- Built using RimWorld's ThingComp system for maximum compatibility
- Uses Harmony patches for seamless integration with door mechanics
- XML patches automatically add components to all door types
- Inspired by Hospitality mod's proven UI patterns

### Compatibility
- **Requires**: Hospitality mod (uses its guest system)
- **Robot Mods**: Compatible with Misc. Robots++ and other robot mods
- **Door Mods**: Should work with most door-adding mods
- **RimWorld Version**: 1.5/1.6+

### Performance
- Lightweight implementation with minimal performance impact
- Only processes payment logic when guests interact with doors
- Efficient caching of exemption status

## Troubleshooting

### Common Issues
- **Gizmo not appearing**: Make sure doors are selected and the mod is enabled
- **Guests not paying**: Check that they have silver in their inventory
- **Exemptions not working**: Verify the pawn type and exemption settings in door config
- **Robots being charged**: Check "Exempt Robots" checkbox in door configuration window
- **Silver not dropping**: Payments appear as physical silver near the door after successful payment
- **Window not opening**: Ensure you're clicking the "Paid Door" gizmo button when doors are selected

### Debug Information
Payment failures and exemption reasons are logged for debugging purposes.

## Development

### Building from Source
```bash
dotnet build Source/Project.csproj --configuration Release
```

### Code Structure
- `CompPayGate.cs`: Core payment logic and component with robot detection
- `CompProperties_PayGate.cs`: Component configuration properties and defaults
- `Gizmo_PaidDoor.cs`: UI gizmo button for door selection
- `Window_PayForAccessConfig.cs`: Comprehensive configuration window interface
- `Building_Door_Patches.cs`: Harmony patches for door integration and payment enforcement
- `Patches/`: XML patches for automatic component injection

## Recent Updates

### v1.1 - Robot Support & Visual Improvements
- ✅ **Robot Compatibility**: Full support for Misc. Robots++ and other robot mods
- ✅ **Smart Robot Detection**: Automatic robot identification with safe reflection
- ✅ **Visual Silver Drops**: Payments now appear as physical silver at doors
- ✅ **Enhanced UI**: Added robot exemption controls to configuration window
- ✅ **Lifetime Earnings**: Track total income from each door over time

## Credits

- **Author**: ProgrammerLily
- **Inspiration**: Hospitality mod's excellent UI patterns
- **Community**: RimWorld modding community for guidance and support

## License

This mod is provided as-is for the RimWorld community. Feel free to learn from the code and create derivative works.

---

*Transform your colony's doors into revenue streams with the Hospitality Pay-to-Access Doors mod!*