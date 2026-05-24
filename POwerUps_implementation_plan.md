# Ultimate Unity Setup Guide & Complete C# Scripts

This document contains the **100% complete C# scripts** for your game's power-ups and level manager, followed by an **extremely detailed, step-by-step Unity Editor setup tutorial**. 

You can copy and paste these complete scripts directly into your files when you are ready.

---

## 1. Complete C# Script: `PowerUps.cs`

This is the entire `PowerUps.cs` file. We have integrated your 3 new premium power-ups, removed the Shuffle Cars feature as requested, and added robust protection against unassigned Inspector fields.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PowerUps : MonoBehaviour
{
    public static PowerUps Instance;
    public SpawnPassengers spawnPassengers;

    private CarMover lastMovedCar;
    private Vector3 lastPosition;

    [Header("Power Up 1: Fill Slot 0")]
    public int fillSlotCount = 3;
    public TextMeshProUGUI fillSlotCountTxt;
    public Button fillSlotButton;

    [Header("Power Up 2: Rearrange Queue")]
    public int rearrangeCount = 3;
    public TextMeshProUGUI rearrangeCountTxt;
    public Button rearrangeButton;

    [Header("Power Up 3: Helicopter VIP")]
    public int helicopterCount = 3;
    public TextMeshProUGUI helicopterCountTxt;
    public Button helicopterButton;
    public ParkingSlotManger vipSlot;
    public GameObject helicopterPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize UI texts gracefully on start if they are assigned
        if (fillSlotCountTxt != null) fillSlotCountTxt.text = fillSlotCount.ToString();
        if (rearrangeCountTxt != null) rearrangeCountTxt.text = rearrangeCount.ToString();
        if (helicopterCountTxt != null) helicopterCountTxt.text = helicopterCount.ToString();
    }

    #region Power Up 1: Fill Slot 0
    /// <summary>
    /// Instantly fills the car parked in the very first parking slot (index 0) 
    /// by drawing matching passengers from the waiting queue.
    /// Handles dynamic car capacities (3, 4, 5, 6, etc.) automatically.
    /// </summary>
    public void FillFirstParkingSlotCar()
    {
        if (fillSlotCount <= 0) return;

        if (ParkingGameManager.Instance == null || ParkingGameManager.Instance.AllParkingSlotManager.Count == 0)
        {
            Debug.LogWarning("AllParkingSlotManager is empty or not initialized!");
            return;
        }

        ParkingSlotManger firstSlot = ParkingGameManager.Instance.AllParkingSlotManager[0];
        if (!firstSlot.isOccupied || firstSlot.currentCar == null)
        {
            Debug.Log("First parking slot is empty! Cannot use Fill power-up.");
            return;
        }

        CarMover car = firstSlot.currentCar;
        ColorOfCarAndPassengers carColor = firstSlot.parkedCarType;
        int neededCapacity = car.CapacityOfPassengers; // Reads the exact current remaining capacity of the car

        if (neededCapacity <= 0) return;

        SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);

        // Find matching passengers of car's color in the waiting queue
        List<Passenger> matches = new List<Passenger>();
        List<Passenger> waitingList = ParkingGameManager.Instance.waitingPassengers;

        for (int i = 0; i < waitingList.Count; i++)
        {
            if (waitingList[i] != null && waitingList[i].passengertype == carColor)
            {
                matches.Add(waitingList[i]);
                if (matches.Count >= neededCapacity) break; // Stops searching once we meet the car's capacity
            }
        }

        if (matches.Count == 0)
        {
            Debug.LogWarning($"No matching passengers of color {carColor} in queue!");
            return;
        }

        // Deduct power-up count and update UI
        fillSlotCount--;
        if (fillSlotCountTxt != null) fillSlotCountTxt.text = fillSlotCount.ToString();
        if (fillSlotCount <= 0 && fillSlotButton != null) fillSlotButton.interactable = false;

        // Animate matches running to the car and deduct capacity
        StartCoroutine(ProcessFillCarRoutine(car, firstSlot, matches));
    }

    private IEnumerator ProcessFillCarRoutine(CarMover car, ParkingSlotManger slot, List<Passenger> matches)
    {
        List<Passenger> waitingList = ParkingGameManager.Instance.waitingPassengers;

        foreach (Passenger p in matches)
        {
            if (p == null) continue;

            waitingList.Remove(p);
            car.CapacityOfPassengers -= 1;
            car.GetComponent<CarMover>().totalPassengerTxt.text = car.CapacityOfPassengers.ToString();
            
            SoundManager.Instance.PlaySound(SoundManager.SoundName.PassengerAdd);
            StartCoroutine(MovePassengerToCar(p, car));
            
            yield return new WaitForSeconds(0.1f);
        }

        // Wait a brief moment for matching movements to complete
        yield return new WaitForSeconds(0.5f);

        if (car.CapacityOfPassengers <= 0)
        {
            slot.ClearSlot();
            car.carType = ColorOfCarAndPassengers.None;
            car.DriveAway();
        }
    }

    private IEnumerator MovePassengerToCar(Passenger p, CarMover car)
    {
        if (p == null || car == null) yield break;
        Vector3 targetPos = car.transform.position;

        while (p != null && Vector3.Distance(p.transform.position, targetPos) > 0.1f)
        {
            p.transform.position = Vector3.MoveTowards(p.transform.position, targetPos, Time.deltaTime * 100);
            yield return null;
        }

        if (p != null)
        {
            ObjectPool.Instance.AddToPool(p.gameObject);
        }
    }
    #endregion

    #region Power Up 2: Rearrange Queue
    /// <summary>
    /// Reorders the passenger waiting list to move passengers that match 
    /// any of the currently parked cars to the FRONT of the queue.
    /// </summary>
    public void RearrangeQueue()
    {
        if (rearrangeCount <= 0) return;

        if (ParkingGameManager.Instance == null) return;

        List<ParkingSlotManger> slots = ParkingGameManager.Instance.AllParkingSlotManager;
        HashSet<ColorOfCarAndPassengers> parkedColors = new HashSet<ColorOfCarAndPassengers>();

        // Collect all parked car colors that still need passengers
        foreach (var slot in slots)
        {
            if (slot != null && slot.isOccupied && slot.currentCar != null && slot.currentCar.CapacityOfPassengers > 0)
            {
                parkedColors.Add(slot.parkedCarType);
            }
        }

        if (parkedColors.Count == 0)
        {
            Debug.Log("No cars in parking slots to rearrange the queue for!");
            return;
        }

        SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);

        List<Passenger> waitingList = ParkingGameManager.Instance.waitingPassengers;
        List<Passenger> matches = new List<Passenger>();
        List<Passenger> nonMatches = new List<Passenger>();

        foreach (Passenger p in waitingList)
        {
            if (p != null)
            {
                if (parkedColors.Contains(p.passengertype))
                {
                    matches.Add(p);
                }
                else
                {
                    nonMatches.Add(p);
                }
            }
        }

        // Reassemble waiting list: matches first, then non-matches
        waitingList.Clear();
        waitingList.AddRange(matches);
        waitingList.AddRange(nonMatches);

        // Deduct power-up count and update UI
        rearrangeCount--;
        if (rearrangeCountTxt != null) rearrangeCountTxt.text = rearrangeCount.ToString();
        if (rearrangeCount <= 0 && rearrangeButton != null) rearrangeButton.interactable = false;

        Debug.Log($"Rearranged passenger queue. Moved {matches.Count} matching passengers to the front.");

        // Kickstart the central matching coroutine safely
        ParkingGameManager.Instance.OnCarParked(ColorOfCarAndPassengers.None);
    }
    #endregion

    #region Power Up 3: Helicopter VIP
    /// <summary>
    /// Opens a 4th VIP slot and flies the unparked grid car matching the first passenger's color 
    /// directly to that VIP slot using a helicopter or procedural lift animation.
    /// </summary>
    public void HelicopterVIPPowerUp()
    {
        if (helicopterCount <= 0) return;
        
        if (vipSlot == null)
        {
            Debug.LogError("VIP Parking Slot reference is not assigned in the Inspector!");
            return;
        }

        if (ParkingGameManager.Instance == null || ParkingGameManager.Instance.waitingPassengers.Count == 0)
        {
            Debug.Log("No passengers waiting in queue!");
            return;
        }

        ColorOfCarAndPassengers targetColor = ParkingGameManager.Instance.waitingPassengers[0].passengertype;

        // Find active, unparked car of the target color on the grid
        CarMover targetCar = spawnPassengers.TotalCarsSpawn.FirstOrDefault(car => 
            car != null && 
            car.gameObject.activeInHierarchy && 
            !car.isParked && 
            car.carType == targetColor
        );

        if (targetCar == null)
        {
            Debug.LogWarning($"No active, unparked car of color {targetColor} found on the grid!");
            return;
        }

        SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);

        // Deduct power-up count and update UI
        helicopterCount--;
        if (helicopterCountTxt != null) helicopterCountTxt.text = helicopterCount.ToString();
        if (helicopterCount <= 0 && helicopterButton != null) helicopterButton.interactable = false;

        StartCoroutine(HelicopterPowerUpRoutine(targetCar, vipSlot));
    }

    private IEnumerator HelicopterPowerUpRoutine(CarMover targetCar, ParkingSlotManger vipSlot)
    {
        // 1. Activate and register the VIP slot in the list of active parking slots
        vipSlot.gameObject.SetActive(true);
        if (!ParkingGameManager.Instance.AllParkingSlotManager.Contains(vipSlot))
        {
            ParkingGameManager.Instance.AllParkingSlotManager.Add(vipSlot);
        }
        
        vipSlot.isReserved = true;
        vipSlot.incomingCar = targetCar;
        targetCar.isParked = true;
        
        // Mark its grid slot on the board as vacant so other cars can pass through
        Carout carout = targetCar.GetComponent<Carout>();
        if (carout != null && SpawnCars.Instance != null)
        {
            SpawnCars.Instance.SetSlotOccupation(carout.currentGridIndex.x, carout.currentGridIndex.y, false);
        }

        Vector3 startCarPos = targetCar.transform.position;
        Vector3 targetSlotPos = vipSlot.transform.position;
        
        GameObject copter = null;
        if (helicopterPrefab != null)
        {
            // Spawn helicopter high up off-screen
            Vector3 spawnPos = startCarPos + new Vector3(-15f, 15f, -15f);
            copter = Instantiate(helicopterPrefab, spawnPos, Quaternion.identity);
            
            // Fly helicopter to directly above the car
            Vector3 aboveCar = startCarPos + new Vector3(0, 5f, 0);
            copter.transform.DOMove(aboveCar, 1f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(1.0f);
            
            // Lower helicopter to car level
            copter.transform.DOMove(startCarPos + new Vector3(0, 2f, 0), 0.5f);
            yield return new WaitForSeconds(0.5f);
            
            // Parent the car to the helicopter
            targetCar.transform.SetParent(copter.transform);
        }
        else
        {
            // Levitate car directly (premium procedural DOTween levitation effect!)
            targetCar.transform.DOMove(startCarPos + new Vector3(0, 5f, 0), 1f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(1f);
        }

        // Fly/Move above VIP slot position
        Vector3 aboveSlot = targetSlotPos + new Vector3(0, 5f, 0);
        if (copter != null)
        {
            copter.transform.DOMove(aboveSlot + new Vector3(0, 2f, 0), 1.5f).SetEase(Ease.InOutQuad);
            copter.transform.DOLookAt(aboveSlot, 0.5f);
        }
        else
        {
            targetCar.transform.DOMove(aboveSlot, 1.5f).SetEase(Ease.InOutQuad);
        }
        yield return new WaitForSeconds(1.5f);

        // Lower car onto the VIP slot
        if (copter != null)
        {
            targetCar.transform.SetParent(null); // Detach car
            targetCar.transform.DOMove(targetSlotPos, 0.5f).SetEase(Ease.InQuad);
            
            // Fly helicopter away off-screen and destroy it
            copter.transform.DOMove(aboveSlot + new Vector3(25f, 15f, 25f), 1.5f).SetEase(Ease.InQuad);
            Destroy(copter, 2f);
        }
        else
        {
            targetCar.transform.DOMove(targetSlotPos, 0.8f).SetEase(Ease.InQuad);
        }
        yield return new WaitForSeconds(0.8f);

        // Snap car perfectly into the VIP slot
        targetCar.transform.position = targetSlotPos;
        targetCar.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Manually trigger slot registration so everything is instantly occupied
        vipSlot.isOccupied = true;
        vipSlot.isReserved = false;
        vipSlot.currentCar = targetCar;
        vipSlot.parkedCarType = targetCar.carType;
        
        targetCar.totalPassengerTxt.enabled = true;
        targetCar.totalPassengerTxt.text = targetCar.CapacityOfPassengers.ToString();
        targetCar.arrow.enabled = false;

        // Start processing queue for the car we just landed
        ParkingGameManager.Instance.OnCarParked(targetCar.carType);
    }
    #endregion
}
```

---

## 2. Complete C# Script: `LevelManager.cs`

This is the entire `LevelManager.cs` file. We have modified `RestartLevel()` and `LevelLoadSequence()` to make sure the VIP Slot gets deactivated and cleaned up at the beginning of each level or restart.

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Video;
using Unity.VisualScripting;

public class LevelManager : MonoBehaviour 
{
    public static LevelManager Instance;
    public List<LevelData> allLevels; // Drag all your level SOs here

    public SpawnCars carSpawner;
    public SpawnPassengers spawnPassengers;
    private int currentLevelIndex = 0;

    public List<ParkingSlotManger> parkingSlotMangers;

    public ParkingGameManager parkingGameManager;

    public TextMeshProUGUI LevelTxt;
    public TextMeshProUGUI LevelTxtOnmainScreen;

    void Awake() 
    {
        // Setup Singleton
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadLevel();
    }

    public void CompleteLevel() 
    {
        currentLevelIndex++;
        LoadLevel();
    }

    #region ResetLevel
    public void RestartLevel()
    {
        // 1. Clear Cars (Loop backwards to avoid IndexOutOfRange)
        if (spawnPassengers.TotalCarsSpawn != null)
        {
            for (int i = spawnPassengers.TotalCarsSpawn.Count - 1; i >= 0; i--)
            {
                CarMover car = spawnPassengers.TotalCarsSpawn[i];
                if (car != null)
                {
                    ObjectPool.Instance.AddToPool(car.gameObject);
                }
            }
            spawnPassengers.TotalCarsSpawn.Clear();
        }

        // 2. Clear Waiting Passengers
        if (parkingGameManager.waitingPassengers != null)
        {
            for (int i = parkingGameManager.waitingPassengers.Count - 1; i >= 0; i--)
            {
                Passenger p = parkingGameManager.waitingPassengers[i];
                if (p != null)
                {
                    ObjectPool.Instance.AddToPool(p.gameObject);
                }
            }
            parkingGameManager.waitingPassengers.Clear();
        }

        // 3. Reset Slots
        foreach(var p in parkingSlotMangers)
        {
            p.isOccupied = false;
            p.isReserved = false;
            p.parkedCarType = ColorOfCarAndPassengers.None;
        }

        // 3.5 Reset VIP slot (Only open for the level it was activated in)
        if (PowerUps.Instance != null && PowerUps.Instance.vipSlot != null)
        {
            ParkingSlotManger vip = PowerUps.Instance.vipSlot;
            vip.gameObject.SetActive(false);
            if (ParkingGameManager.Instance != null && ParkingGameManager.Instance.AllParkingSlotManager.Contains(vip))
            {
                ParkingGameManager.Instance.AllParkingSlotManager.Remove(vip);
            }
            vip.ClearSlot();
        }
        
        // 4. Reload Level (Index remains the same)
        LoadLevel();
    }
    #endregion

    #region InfinityLevel
    public void LoadLevel()
    {
        LevelData currentLevel;

        // Use handmade levels first
        if (currentLevelIndex < allLevels.Count)
        {
            currentLevel = allLevels[currentLevelIndex];
        }
        else
        {
            // Generate random level
            currentLevel = CreateRandomLevel(currentLevelIndex + 1);
        }

        carSpawner.currentLevelData = currentLevel;

        Debug.Log("Now playing level: " + currentLevel.levelNumber);

        LevelTxt.text = "Level - " + currentLevel.levelNumber;
        LevelTxtOnmainScreen.text = "Level - " + currentLevel.levelNumber;

        ParkingGameManager.Instance.ResetForNewLevel();

        StartCoroutine(LevelLoadSequence());
    }

    LevelData CreateRandomLevel(int levelNumber)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();

        level.levelNumber = levelNumber;

        // Random cars between 3 and 9
        level.carsToSpawn = Random.Range(3, 10);

        return level;
    }   
    #endregion

    IEnumerator LevelLoadSequence()
    {
        // 1. IMPORTANT: Reset the game state BEFORE starting spawning
        spawnPassengers.TotalCarsSpawn.Clear();

        // Reset VIP slot so it is deactivated on new levels
        if (PowerUps.Instance != null && PowerUps.Instance.vipSlot != null)
        {
            ParkingSlotManger vip = PowerUps.Instance.vipSlot;
            vip.gameObject.SetActive(false);
            if (ParkingGameManager.Instance != null && ParkingGameManager.Instance.AllParkingSlotManager.Contains(vip))
            {
                ParkingGameManager.Instance.AllParkingSlotManager.Remove(vip);
            }
            vip.ClearSlot();
        }
        
        yield return new WaitForSeconds(0.5f);

        // Spawn Cars
        carSpawner.SpawnCarAsPerLevelNeed();
        
        yield return new WaitForSeconds(0.2f);

        // Spawn Passengers
        spawnPassengers.PassengerToSpawnBasedOnCarSpawn();

        // 2. IMPORTANT: Wait one more frame to ensure the list is actually filled
        yield return new WaitForEndOfFrame();

        // 3. Now let the game check for the win condition
        ParkingGameManager.Instance.isLevelLoading = false;
    }
}
```

---

## 3. Extremely Detailed Step-by-Step Unity Editor Setup Tutorial

Follow these clear, visual steps to set up the new power-ups and VIP slot inside your Unity project:

---

### Step 1: Duplicate and Configure the VIP Parking Slot

1. Open your Unity **Scene**.
2. Locate your existing parking slots in the **Hierarchy** (they hold the `ParkingSlotManger` script).
3. **Right-click** on one of the slots and select **Duplicate** (or press `Ctrl + D` / `Cmd + D`).
4. Rename this duplicate to **`VIP_ParkingSlot`** to make it easy to identify.
5. In the **Scene View**, slide this duplicate slightly to the side of the 3 normal slots (e.g. place it as a 4th slot).
6. **Deactivate the VIP slot**: In the Inspector for `VIP_ParkingSlot`, **uncheck** the little active box next to its name at the very top left. It should turn dark gray/inactive in your Hierarchy.
7. **WARNING**: Do **NOT** drag `VIP_ParkingSlot` into the `parkingSlotMangers` list of your `LevelManager` script. The VIP slot must remain hidden and completely separate.

---

### Step 2: Configure the `PowerUps` Inspector Links

1. Select the Game Object in your scene that holds the `PowerUps` script (usually named `PowerUps` or `GameManager`).
2. Looking at the **Inspector**, you will see the new fields for the 3 power-ups.
3. **Link the VIP Slot**:
   * Drag your inactive **`VIP_ParkingSlot`** Game Object from the Hierarchy and drop it into the **`Vip Slot`** field of the `PowerUps` script.
4. **Link the Helicopter Prefab (Optional)**:
   * If you have a helicopter 3D model/prefab, drag it from your Project view into the **`Helicopter Prefab`** field.
   * If you do not have one, leave it **empty**. The car will automatically use our custom gravity-beam DOTween levitation effect instead, which looks fantastic!

---

### Step 3: Create and Design the UI Buttons

1. In the Hierarchy, expand your **Canvas** and find where your current gameplay buttons are.
2. Create **3 new UI Buttons** (Right-click $\rightarrow$ **UI** $\rightarrow$ **Button - TextMeshPro**):
   * Rename Button 1 to **`Btn_FillCar`**
   * Rename Button 2 to **`Btn_Rearrange`**
   * Rename Button 3 to **`Btn_Helicopter`**
3. Inside each button, create a small **Text - TextMeshPro** element to show the remaining count (e.g., "3"):
   * Rename Text 1 to **`Txt_FillCount`**
   * Rename Text 2 to **`Txt_RearrangeCount`**
   * Rename Text 3 to **`Txt_HelicopterCount`**
4. Go back to your `PowerUps` Game Object, and in the Inspector, drag and drop these new UI elements into their respective fields:
   * Drag `Btn_FillCar` $\rightarrow$ **`Fill Slot Button`**
   * Drag `Txt_FillCount` $\rightarrow$ **`Fill Slot Count Txt`**
   * Drag `Btn_Rearrange` $\rightarrow$ **`Rearrange Button`**
   * Drag `Txt_RearrangeCount` $\rightarrow$ **`Rearrange Count Txt`**
   * Drag `Btn_Helicopter` $\rightarrow$ **`Helicopter Button`**
   * Drag `Txt_HelicopterCount` $\rightarrow$ **`Helicopter Count Txt`**

---

### Step 4: Wire up the Click Events (`OnClick`)

For each of the three buttons, we need to connect them to the script functions:

1. **Configure Fill Button**:
   * Select **`Btn_FillCar`** in the Hierarchy.
   * Scroll down in the Inspector to the **`On Click ()`** box and click the **`+`** icon to add a slot.
   * Drag your **`PowerUps` Holder** Game Object from the Hierarchy into the Object field.
   * Click the Function dropdown (currently says *No Function*), select **`PowerUps`** $\rightarrow$ **`FillFirstParkingSlotCar`**.

2. **Configure Rearrange Button**:
   * Select **`Btn_Rearrange`** in the Hierarchy.
   * Click **`+`** in the `On Click ()` box.
   * Drag your **`PowerUps` Holder** Game Object into the Object field.
   * Select the dropdown **`PowerUps`** $\rightarrow$ **`RearrangeQueue`**.

3. **Configure Helicopter Button**:
   * Select **`Btn_Helicopter`** in the Hierarchy.
   * Click **`+`** in the `On Click ()` box.
   * Drag your **`PowerUps` Holder** Game Object into the Object field.
   * Select the dropdown **`PowerUps`** $\rightarrow$ **`HelicopterVIPPowerUp`**.
