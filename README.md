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
- Anyone you don't want to charge

### 🎮 Easy-to-Use Interface
- **Cost Adjustment**: Up/down buttons to modify door costs
- **Auto Mode**: Quick setup with reasonable default pricing
- **Multi-Selection**: Configure multiple doors at once
- **Right-Click Menu**: Access advanced options and payment mode settings

## Installation

1. Download the mod files
2. Place in your RimWorld `Mods` folder
3. Enable in the mod list (requires Hospitality mod as dependency)
4. Start a new game or load an existing save

## Usage

### Basic Setup
1. Select any door(s) on your map
2. Use the door cost gizmo buttons that appear in the bottom panel:
   - **↑** button: Increase cost by 25 silver
   - **↓** button: Decrease cost by 25 silver  
   - **Auto** button: Set reasonable default cost

### Advanced Configuration
1. Right-click the door cost gizmo for advanced options:
   - Toggle between "Pay Once" and "Pay Every Time" modes
   - Fine-tune exact costs
   - View payment statistics

### Payment Behavior
- **Pay Once Mode**: Guest pays the set amount and gains permanent access
- **Pay Every Time Mode**: Guest must pay each time they want to pass through
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
- **Compatible**: Should work with most door-adding mods
- **RimWorld Version**: 1.5/1.6+

### Performance
- Lightweight implementation with minimal performance impact
- Only processes payment logic when guests interact with doors
- Efficient caching of exemption status

## Troubleshooting

### Common Issues
- **Gizmos not appearing**: Make sure doors are selected and the mod is enabled
- **Guests not paying**: Check that they have silver in their inventory
- **Exemptions not working**: Verify the pawn type (colonist/prisoner/etc.)

### Debug Information
Payment failures and exemption reasons are logged for debugging purposes.

## Development

### Building from Source
```bash
dotnet build Source/Project.csproj --configuration Release
```

### Code Structure
- `CompPayGate.cs`: Core payment logic and component
- `Gizmo_PayGateDoor.cs`: User interface for door configuration
- `Building_Door_Patches.cs`: Harmony patches for door integration
- `Patches/`: XML patches for component injection

## Credits

- **Author**: ProgrammerLily
- **Inspiration**: Hospitality mod's excellent UI patterns
- **Community**: RimWorld modding community for guidance and support

## License

This mod is provided as-is for the RimWorld community. Feel free to learn from the code and create derivative works.

---

*Transform your colony's doors into revenue streams with the Hospitality Pay-to-Access Doors mod!*