# dnSpy Exploration Guide for Bonkipelago

## Setup

1. **Download dnSpy**:
   - Go to: https://github.com/dnSpy/dnSpy/releases
   - Download the latest release (dnSpy-net-win64.zip)
   - Extract to a folder

2. **Open Assembly-CSharp.dll**:
   - Launch dnSpyEx.exe (or dnSpy.exe)
   - File → Open → Navigate to:
     `C:\Program Files (x86)\Steam\steamapps\common\Megabonk\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll`

## Navigation Tips

- **Search**: Press `Ctrl+Shift+K` to search for types (classes)
- **Go to Method**: `Ctrl+Shift+M` to search for methods
- **Find References**: Right-click a method → "Analyze" to see what calls it
- **Decompile Options**: Use C# mode (not IL) for easier reading

---

## What to Find

### 1. CHEST SYSTEM (`InteractableChest`)

**Search for**: `InteractableChest`

**Look for methods like**:
- `Open()` / `Interact()` / `OnInteract()`
- `GiveItem()` / `GrantReward()`
- Any method that's called when player opens chest

**What to note**:
- Exact method name
- Parameters (e.g., `Player player`)
- If it returns anything

**Example patch**:
```csharp
[HarmonyPatch(typeof(InteractableChest), "MethodNameHere")]
public class ChestOpenPatch { ... }
```

---

### 2. ENEMY/BOSS SYSTEM (`Enemy`)

**Search for**: `Enemy`

**Look for**:
- Death methods: `Die()`, `OnDeath()`, `Kill()`, `OnKilled()`
- Damage methods: `TakeDamage()`, `Damage()`, `Hit()`
- Boss detection: Fields/properties like `isBoss`, `bossType`, `enemyType`

**Important checks**:
- How to detect if enemy is a boss
- When exactly the death method is called (before/after death animation?)

**Example**:
```csharp
[HarmonyPatch(typeof(Enemy), "Die")]
public class EnemyDeathPatch { ... }
```

---

### 3. LEVEL-UP SYSTEM

**Search for**:
- `LevelUpScreen` (we saw this in the dumps!)
- `PlayerLevel` / `Experience` / `XP`
- `Upgrade` / `Choice`

**Look for**:
- Method that generates the 3 random choices
- Method that's called when player selects a choice
- Data structures holding available weapons/tomes
- Weapon/Tome list or array

**Key methods to find**:
- How random choices are generated
- How a choice is applied to the player
- Where the weapon/tome pool is stored

---

### 4. ITEM/WEAPON/TOME GRANTING

**Search for**:
- `MyPlayer` (we saw this in dumps!)
- `Inventory` / `PlayerInventory`
- `Weapon` / `Tome` / `Item`

**Look for methods like**:
- `AddWeapon()` / `GiveWeapon()` / `EquipWeapon()`
- `AddTome()` / `GiveTome()`
- `AddItem()` / `GiveItem()`

**What to note**:
- What parameter they take (int ID? string name? object?)
- If there's a weapon/tome enum or ID system
- How to get a list of all weapons/tomes

---

## Specific Investigations

### Finding Weapon/Tome Lists

Look for:
- Static lists/arrays: `public static Weapon[] allWeapons`
- Enums: `public enum WeaponType { ... }`
- ScriptableObject references
- Methods like `GetAllWeapons()`, `GetWeaponById(int id)`

### Understanding Item Selection at Level-Up

1. Find `LevelUpScreen` class
2. Look for method that shows the UI (might be called `Show()`, `Open()`, `Display()`)
3. Find where the 3 random items are chosen
4. Trace back to see where the item pool comes from

### Boss Detection

From our dumps, we know:
- Boss enemies have a child GameObject: `Render/EnemyStatusSymbols(Clone)/isBoss`

Check in `Enemy` class if there's:
- `bool isBoss` field/property
- `EnemyType` enum with a Boss value
- Method like `IsBoss()` or `GetEnemyType()`

---

## How to Report Findings

For each system, note:

1. **Class name**: (e.g., `InteractableChest`)
2. **Method name**: (e.g., `Open`)
3. **Full signature**: (e.g., `public void Open(Player player)`)
4. **When it's called**: (e.g., "When player presses E on chest")
5. **Any relevant fields**: (e.g., "chest.itemId holds the item to give")

---

## Example Workflow

1. Search for `InteractableChest`
2. Double-click to open the class
3. Browse through methods
4. Find `Interact()` method
5. Read the code to understand what it does
6. Note: "InteractableChest.Interact() is called when player presses E"
7. Update patch file with actual method name
8. Repeat for Enemy, LevelUpScreen, MyPlayer

---

## Tips

- **Don't get overwhelmed**: Start with just ONE system (chests are easiest)
- **Read the code**: The decompiled code is usually readable
- **Follow references**: Right-click → Analyze to see what calls a method
- **Look for Unity events**: Methods like `OnTriggerEnter`, `OnCollisionEnter`
- **Check MonoBehaviour methods**: `Start()`, `Awake()`, `Update()`

---

## Next Steps After Finding Methods

1. Update the patch files in `Patches/` folder
2. Uncomment the `[HarmonyPatch]` attribute
3. Replace `"MethodNameHere"` with actual method name
4. Build the project
5. Test in-game
6. Check MelonLoader logs to see if patches applied successfully
