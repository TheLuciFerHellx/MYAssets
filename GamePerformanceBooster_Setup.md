# 🚀 GamePerformanceBooster — Setup Guide
### Car-OUT Jam Puzzle Game | Auto FPS Booster for ALL Devices

---

## 📋 WHAT THIS SCRIPT DOES

One script. Drop it in scene. Done.
It **auto-detects** your player's device (Low / Mid / High) and applies the best settings automatically.
- ✅ Works on Android, iOS, PC
- ✅ No coding needed — just drag and drop
- ✅ Persists across all scenes (DontDestroyOnLoad)

---

## 📁 STEP 1 — The Full Script

> **File already created at:** `Assets/Script/GamePerformanceBooster.cs`
> 
> If missing, create a new C# script named `GamePerformanceBooster` and paste this:

```csharp
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

/// <summary>
/// ============================================================
///  GAME PERFORMANCE BOOSTER — Car-OUT Jam Puzzle
///  Made by: Antigravity AI (Claude Sonnet)
/// ============================================================
///  HOW TO USE:
///  1. Create an empty GameObject in your scene
///  2. Name it "PerformanceBooster"
///  3. Drag this script onto it
///  4. Press Play — it auto-detects your device and applies best settings
///  WORKS ON: Android, iOS, PC, Editor — All Devices Low to High
/// ============================================================
/// </summary>
public class GamePerformanceBooster : MonoBehaviour
{
    public static GamePerformanceBooster Instance;

    public enum DeviceTier { Low, Mid, High }
    public DeviceTier CurrentTier { get; private set; }

    [Header("═══ TARGET FPS ═══")]
    public int targetFPS_High = 60;
    public int targetFPS_Mid  = 60;
    public int targetFPS_Low  = 30;

    [Header("═══ AUTO DETECT ═══")]
    public bool autoDetect = true;
    public DeviceTier forcedTier = DeviceTier.Mid;

    [Header("═══ PHYSICS OPTIMIZATION ═══")]
    public bool optimizePhysics = true;

    [Header("═══ RENDER OPTIMIZATION ═══")]
    public bool optimizeShadows = true;
    public bool optimizeLights  = true;

    [Header("═══ MEMORY ═══")]
    public bool cleanMemoryOnStart = true;

    [Header("═══ DEBUG INFO (Read Only) ═══")]
    [SerializeField] private string _detectedDevice = "";
    [SerializeField] private int    _ramMB = 0;
    [SerializeField] private string _gpuName = "";
    [SerializeField] private string _appliedTier = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        DetectDeviceTier();
        ApplyAllOptimizations();
    }

    void Start()
    {
        if (cleanMemoryOnStart)
            StartCoroutine(CleanMemoryRoutine());
    }

    private void DetectDeviceTier()
    {
        if (!autoDetect)
        {
            CurrentTier  = forcedTier;
            _appliedTier = "FORCED: " + forcedTier.ToString();
            return;
        }

        int    ram      = SystemInfo.systemMemorySize;
        int    gpu      = SystemInfo.graphicsMemorySize;
        int    cpuCount = SystemInfo.processorCount;
        _ramMB          = ram;
        _gpuName        = SystemInfo.graphicsDeviceName;
        _detectedDevice = SystemInfo.deviceModel;

        if      (ram >= 4096 && gpu >= 2048 && cpuCount >= 4) CurrentTier = DeviceTier.High;
        else if (ram < 2048  || gpu < 512)                    CurrentTier = DeviceTier.Low;
        else                                                   CurrentTier = DeviceTier.Mid;

        _appliedTier = "AUTO: " + CurrentTier.ToString();
        Debug.Log($"[Booster] Device:{_detectedDevice} RAM:{ram}MB GPU:{gpu}MB CPU:{cpuCount} → Tier:{CurrentTier}");
    }

    private void ApplyAllOptimizations()
    {
        SetTargetFPS();
        SetQualityLevel();
        SetPhysicsRate();
        SetShadows();
        SetLights();
        SetVSync();
    }

    private void SetTargetFPS()
    {
        Application.targetFrameRate = CurrentTier switch
        {
            DeviceTier.High => targetFPS_High,
            DeviceTier.Mid  => targetFPS_Mid,
            DeviceTier.Low  => targetFPS_Low,
            _               => 60
        };
    }

    private void SetQualityLevel()
    {
        int q = CurrentTier switch
        {
            DeviceTier.High => Mathf.Min(5, QualitySettings.names.Length - 1),
            DeviceTier.Mid  => Mathf.Min(2, QualitySettings.names.Length - 1),
            DeviceTier.Low  => 0,
            _               => 2
        };
        QualitySettings.SetQualityLevel(q, true);
    }

    private void SetPhysicsRate()
    {
        if (!optimizePhysics) return;
        Time.fixedDeltaTime = CurrentTier == DeviceTier.Low ? 0.04f : 0.02f;
    }

    private void SetShadows()
    {
        if (!optimizeShadows) return;
        switch (CurrentTier)
        {
            case DeviceTier.High:
                QualitySettings.shadows         = ShadowQuality.All;
                QualitySettings.shadowDistance  = 50f;
                break;
            case DeviceTier.Mid:
                QualitySettings.shadows         = ShadowQuality.HardOnly;
                QualitySettings.shadowDistance  = 25f;
                break;
            case DeviceTier.Low:
                QualitySettings.shadows         = ShadowQuality.Disable;
                QualitySettings.shadowDistance  = 0f;
                break;
        }
    }

    private void SetLights()
    {
        if (!optimizeLights) return;
        QualitySettings.pixelLightCount = CurrentTier switch
        {
            DeviceTier.High => 4,
            DeviceTier.Mid  => 2,
            DeviceTier.Low  => 0,
            _               => 2
        };
    }

    private void SetVSync()
    {
        QualitySettings.vSyncCount = CurrentTier == DeviceTier.Low ? 0 : 1;
    }

    private IEnumerator CleanMemoryRoutine()
    {
        yield return null;
        yield return null;
        System.GC.Collect();
        yield return Resources.UnloadUnusedAssets();
        Debug.Log("[Booster] Memory cleaned.");
    }

    // ── PUBLIC API ──────────────────────────────────────
    /// Call this on every new level load to clear memory
    public void OnLevelLoad()
    {
        StartCoroutine(CleanMemoryRoutine());
    }

    /// Temporarily raise FPS cap for a duration (seconds)
    public void TemporaryBoost(int fps, float duration)
    {
        StartCoroutine(TemporaryBoostRoutine(fps, duration));
    }

    private IEnumerator TemporaryBoostRoutine(int fps, float duration)
    {
        Application.targetFrameRate = fps;
        yield return new WaitForSeconds(duration);
        SetTargetFPS();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        targetFPS_High = Mathf.Clamp(targetFPS_High, 30, 120);
        targetFPS_Mid  = Mathf.Clamp(targetFPS_Mid,  30, 60);
        targetFPS_Low  = Mathf.Clamp(targetFPS_Low,  20, 30);
    }
#endif
}
```

---

## 🎮 STEP 2 — Add To Your Scene (3 Steps Only)

### Step 2A — Create the GameObject

1. Open your **Main Game Scene** in Unity
2. In the **Hierarchy** panel → Right-click → **`Create Empty`**
3. Name it exactly: **`PerformanceBooster`**

```
Hierarchy:
├── Main Camera
├── GameManager
├── PerformanceBooster     ← NEW OBJECT HERE
└── ...
```

### Step 2B — Attach the Script

1. Select the `PerformanceBooster` object in Hierarchy
2. In the **Inspector** → click **`Add Component`**
3. Search for **`GamePerformanceBooster`** → click it

### Step 2C — Inspector Settings (Recommended)

| Setting | Value | Why |
|---------|-------|-----|
| `Auto Detect` | ✅ ON | Auto-detects player's device |
| `Target FPS High` | 60 | Flagship phones |
| `Target FPS Mid` | 60 | Normal phones |
| `Target FPS Low` | 30 | Budget phones |
| `Optimize Physics` | ✅ ON | Saves CPU on low-end |
| `Optimize Shadows` | ✅ ON | Saves GPU on low-end |
| `Optimize Lights` | ✅ ON | Saves GPU on low-end |
| `Clean Memory On Start` | ✅ ON | Prevents stutter |

> ✅ **That's it for basic setup!** Press Play and it works automatically.

---

## 🔗 STEP 3 — Connect To LevelManager.cs

This is the important part. Every time a new level loads, you want to **clean memory** so no stutter builds up over time.

### Open This File:
📄 `Assets/Script/Level/LevelManager.cs`

### Find This Coroutine (around Line 402):

```csharp
IEnumerator LevelLoadSequence()
{
    spawnPassengers.TotalCarsSpawn.Clear();
    ...
}
```

### Add ONE Line At The TOP Of The Coroutine:

```csharp
IEnumerator LevelLoadSequence()
{
    // ✅ ADD THIS LINE — cleans memory every level load
    GamePerformanceBooster.Instance?.OnLevelLoad();

    // ── existing code below — DO NOT CHANGE ──
    spawnPassengers.TotalCarsSpawn.Clear();

    if (PowerUps.Instance != null && PowerUps.Instance.vipSlot != null)
    {
        ParkingSlotManger vip = PowerUps.Instance.vipSlot;
        vip.gameObject.SetActive(false);
        if (ParkingGameManager.Instance != null && ParkingGameManager.Instance.AllParkingSlotManager.Contains(vip))
            ParkingGameManager.Instance.AllParkingSlotManager.Remove(vip);
        vip.ClearSlot();
    }

    yield return new WaitForSeconds(0.5f);
    carSpawner.SpawnCarAsPerLevelNeed();

    yield return new WaitForSeconds(0.2f);
    spawnPassengers.PassengerToSpawnBasedOnCarSpawn();

    yield return new WaitForEndOfFrame();
    ParkingGameManager.Instance.isLevelLoading = false;
}
```

> **Why `?.` (null-conditional)?** — It means "only call if Instance exists". Safe — will never crash even if the booster is missing from the scene.

---

## 📊 What Happens On Each Device

| | 🔴 Budget Phone | 🟡 Normal Phone | 🟢 Flagship / PC |
|--|----------------|----------------|-----------------|
| **RAM** | < 2 GB | 2–4 GB | 4 GB+ |
| **Tier** | LOW | MID | HIGH |
| **FPS Target** | 30 | 60 | 60 |
| **Shadows** | DISABLED | Hard Only | Full |
| **Lights** | Vertex only | 2 lights | 4 lights |
| **Physics** | 25/sec | 50/sec | 50/sec |
| **VSync** | OFF | ON | ON |
| **Quality** | Fastest | Medium | Ultra |
| **Memory Clean** | Every level | Every level | Every level |

---

## ⚠️ IMPORTANT NOTES

> [!NOTE]
> The `PerformanceBooster` GameObject uses `DontDestroyOnLoad` — it stays alive across ALL scenes. You only need to add it to your **first/main scene** once.

> [!TIP]
> In the Inspector, you can set `Auto Detect = OFF` and `Forced Tier = Low` to **test what the game looks like on a budget phone** without needing a real device.

> [!WARNING]
> Do NOT add this script to multiple scenes. Because it uses `DontDestroyOnLoad`, adding it to 2 scenes will create a duplicate and the second one will be destroyed automatically (handled by the singleton code).

> [!IMPORTANT]
> The `?.` null-conditional operator in `GamePerformanceBooster.Instance?.OnLevelLoad()` requires **C# 6 or higher**. Unity 2019.3+ supports this by default — you are fine.

---

## ✅ Final Checklist

- [ ] `GamePerformanceBooster.cs` is in `Assets/Script/`
- [ ] Empty GameObject named `PerformanceBooster` in the main scene
- [ ] Script attached to that GameObject
- [ ] `Auto Detect` is ON in Inspector
- [ ] `GamePerformanceBooster.Instance?.OnLevelLoad();` added at top of `LevelLoadSequence()` in `LevelManager.cs`

---

*Guide made by Antigravity AI — Claude Sonnet | Car-OUT Puzzle Game*
