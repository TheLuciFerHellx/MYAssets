# ✅ Shop System — Final Fixed Version

> Scripts location: `Assets/Script/Shop/ShopManager.cs` and `Assets/Script/Shop/ShopItemCard.cs`
> Also updated: `Assets/Script/UIManager/ShopPopup.cs`

---

## 🐛 Bugs That Were Fixed

| Bug | Root Cause | Fix |
|---|---|---|
| **Double reward / wrong coin count** | Old code called `AddCurrency()` multiple times. Each call triggers `Save()` → `OnDataChanged` → `LoadAllData()` mid-purchase, corrupting the total | Now ALL changes are written directly to `CurrentSave` fields first, then `Save()` is called **exactly ONCE** at the end |
| **Powerups not updating** | Old code called `PowerUps.Instance.LoadFromSave()` but `PowerUps.Instance` only exists in the GameView scene. If shop is on Main Menu, it's null and silently does nothing | Powerup counts are written to `SaveData.CurrentSave` directly. `OnDataChanged` fires → `GameDataLoader.LoadAllData()` reads fresh values and updates HUD texts automatically — **no dependency on PowerUps.Instance** |
| **Card registration timing** | Cards calling `ShopManager.Instance.RegisterCard()` in `Awake()` — but ShopManager's `Awake()` runs the same frame, so `Instance` could be null | Registration moved to `Start()` and `OnEnable()`. By `Start()`, all `Awake()` calls are finished and `Instance` is guaranteed to exist |
| **Coins not showing in shop** | `shopCoinText` was not being refreshed when shop opened | `ShopPopup.ShowShopPopup()` now calls `ShopManager.Instance.OnShopOpened()` which calls `RefreshShopCoinDisplay()` + `RefreshAllCards()` immediately |

---

## 📁 Files Created / Modified inside the Project

| File Path | Status in Project | Description |
|---|---|---|
| [Assets/Script/Shop/ShopManager.cs](file:///c:/Users/ABHAYprajapati/Downloads/Car-OUT-jam-puzzle-Game-Grid/Car-OUT-jam-puzzle-Game-Grid/Assets/Script/Shop/ShopManager.cs) | ✅ Created & Added | Singleton controller. Handles purchases and atomic saves. |
| [Assets/Script/Shop/ShopItemCard.cs](file:///c:/Users/ABHAYprajapati/Downloads/Car-OUT-jam-puzzle-Game-Grid/Car-OUT-jam-puzzle-Game-Grid/Assets/Script/Shop/ShopItemCard.cs) | ✅ Created & Added | Attached to each card UI. Configures cost, rewards, and handles Buy click. |
| [Assets/Script/UIManager/ShopPopup.cs](file:///c:/Users/ABHAYprajapati/Downloads/Car-OUT-jam-puzzle-Game-Grid/Car-OUT-jam-puzzle-Game-Grid/Assets/Script/UIManager/ShopPopup.cs) | ✅ Updated & Added | Refreshes the shop display immediately upon opening. |

---

## ⚙️ Unity Setup — Step by Step

### Step 1 — Create the Shop folder & scripts

Both scripts are already fully created and placed inside your Unity project at `Assets/Script/Shop/`. When you open Unity, it will auto-detect and compile them!

---

### Step 2 — ShopManager GameObject

1. Inside your Shop popup hierarchy (in Canvas), create an **empty GameObject**
2. Name it `ShopManager`
3. Drag `ShopManager.cs` onto it
4. In the Inspector, assign:

| Inspector Field | What to drag |
|---|---|
| `Shop Coin Text` | The TMP text that shows player coins inside the shop |

---

### Step 3 — Create Card GameObjects

Each card is a UI GameObject. Typical layout:

```
CardGameObject  ← attach ShopItemCard.cs HERE
├── Background (Image)
├── ItemNameText (TMP)          → drag to: itemNameText
├── RewardDescriptionText (TMP) → drag to: rewardDescriptionText
├── CostText (TMP)              → drag to: costText
└── BuyButton (Button)          → drag to: buyButton
```

**Attach `ShopItemCard.cs` to the ROOT of each card.**

---

### Step 4 — Configure Each Card in Inspector

#### 🟡 Gold Pack — Coins Only

| Field | Value |
|---|---|
| Item Name | `Gold Pack` |
| Card Type | `CoinPack` |
| Coin Cost | `50` |
| Coins Reward | `200` |
| Powerup Rewards | *(leave list empty)* |

---

#### 💎 Jumbo Mega Pack — Coins + 2 Powerups

| Field | Value |
|---|---|
| Item Name | `Jumbo Mega Pack` |
| Card Type | `MegaPack` |
| Coin Cost | `150` |
| Coins Reward | `300` |
| Powerup Rewards | **Add 2 entries:** |

Powerup Rewards entries:

| # | Powerup Type | Amount |
|---|---|---|
| 0 | `FillSlot` | `2` |
| 1 | `Helicopter` | `1` |

---

#### 🔧 FillSlot Pack — Powerup Only

| Field | Value |
|---|---|
| Item Name | `Fill Slot x3` |
| Card Type | `PowerupOnly` |
| Coin Cost | `80` |
| Coins Reward | `0` |
| Powerup Rewards | 1 entry: `FillSlot`, Amount `3` |

---

### Step 5 — Place Cards in Scroll View

```
ShopPanel
└── ScrollView
    └── Viewport
        └── Content  ← Vertical Layout Group + Content Size Fitter
            ├── GoldPackCard       ← ShopItemCard.cs attached
            ├── JumboMegaPackCard  ← ShopItemCard.cs attached
            └── FillSlotPackCard   ← ShopItemCard.cs attached
```

---

### Step 6 — Wire the Open Button

On your open-shop button: **On Click ()** → drag the GameObject that has `ShopPopup.cs` → select `ShopPopup.ShowShopPopup()`

That's all. `ShowShopPopup()` now also calls `ShopManager.OnShopOpened()` automatically.

---

## 🔄 Complete Purchase Flow (No Bugs)

```
Player taps BUY
        ↓
ShopItemCard.OnBuyClicked()
        ↓
ShopManager.TryPurchase(card)
        ↓
playerCoins >= coinCost ?
  NO  → card.PlayInsufficientFeedback()  [shake, nothing saved]
  YES ↓
        ↓
CurrentSave.currency  -= cost           ← direct field write
CurrentSave.currency  += coinsReward    ← direct field write  
CurrentSave.fillSlotCount  += X         ← direct field write (if any)
CurrentSave.rearrangeCount += X         ← direct field write (if any)
CurrentSave.helicopterCount+= X         ← direct field write (if any)
        ↓
SaveData.Save()  ← called ONCE, writes JSON, fires OnDataChanged ONCE
        ↓
OnDataChanged fires:
  ├── GameDataLoader.LoadAllData()   → updates coin HUD, powerup count HUD texts
  ├── ShopManager.RefreshAllCards()  → updates all card buy buttons (enable/disable)
  └── ShopManager.RefreshShopCoinDisplay() → updates coin text inside shop
        ↓
PowerUps.Instance.LoadFromSave()   ← called if in GameView (safe null check)
```

---

## 📄 Complete Script Code

### 1. ShopManager.cs
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShopManager — Singleton. Central purchase controller for the shop.
///
/// HOW IT WORKS:
///   1. Player taps BUY on a ShopItemCard.
///   2. ShopItemCard calls ShopManager.Instance.TryPurchase(this).
///   3. ShopManager does ONE atomic operation: modifies CurrentSave directly, then calls Save() ONCE.
///   4. SaveData.Save() fires OnDataChanged ONCE.
///   5. OnDataChanged refreshes GameDataLoader (HUD coins + powerup texts) automatically.
///   6. PowerUps.LoadFromSave() is also called if the instance exists (GameView scene).
///   7. All shop card buttons refresh interactable state.
///
/// PLACE ON: An empty GameObject inside your ShopPopup (e.g. named "ShopManager").
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("─── Coin Display Inside Shop ───")]
    [Tooltip("TMP text that shows the player's current coin balance at the top of the shop")]
    public TextMeshProUGUI shopCoinText;

    // All cards in the scene register here so we can refresh them
    private readonly List<ShopItemCard> _registeredCards = new List<ShopItemCard>();

    // ─────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Auto-refresh coin display whenever ANY save happens (level complete, daily reward, etc.)
        SaveData.OnDataChanged += OnSaveDataChanged;
    }

    private void OnDisable()
    {
        SaveData.OnDataChanged -= OnSaveDataChanged;
    }

    private void Start()
    {
        RefreshShopCoinDisplay();
        RefreshAllCards();
    }

    // ─────────────────────────────────────────────────────────────────────
    // CARD REGISTRATION  (ShopItemCard calls these)
    // ─────────────────────────────────────────────────────────────────────

    public void RegisterCard(ShopItemCard card)
    {
        if (card != null && !_registeredCards.Contains(card))
            _registeredCards.Add(card);
    }

    public void UnregisterCard(ShopItemCard card)
    {
        _registeredCards.Remove(card);
    }

    // ─────────────────────────────────────────────────────────────────────
    // PURCHASE  — the ONLY place coins/powerups are modified
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by ShopItemCard when the player taps the buy button.
    /// Returns true if the purchase was successful.
    /// </summary>
    public bool TryPurchase(ShopItemCard card)
    {
        if (SaveData.Instance == null)
        {
            Debug.LogError("[ShopManager] SaveData.Instance is null! Cannot purchase.");
            return false;
        }

        int playerCoins = SaveData.Instance.CurrentSave.currency;
        int cost        = card.coinCost;

        // ── Not enough coins → shake the card and bail ──────────────────
        if (playerCoins < cost)
        {
            Debug.LogWarning($"[ShopManager] Need {cost} coins but player only has {playerCoins}.");
            card.PlayInsufficientFeedback();
            return false;
        }

        // ── ATOMIC: modify CurrentSave directly, call Save() ONCE at end ─
        // Step 1: deduct cost
        SaveData.Instance.CurrentSave.currency -= cost;

        // Step 2: add coin reward (if any)
        if (card.coinsReward > 0)
            SaveData.Instance.CurrentSave.currency += card.coinsReward;

        // Step 3: add powerup rewards (if any)
        foreach (var reward in card.powerupRewards)
        {
            switch (reward.powerupType)
            {
                case ShopItemCard.PowerupType.FillSlot:
                    SaveData.Instance.CurrentSave.fillSlotCount += reward.amount;
                    break;

                case ShopItemCard.PowerupType.Rearrange:
                    SaveData.Instance.CurrentSave.rearrangeCount += reward.amount;
                    break;

                case ShopItemCard.PowerupType.Helicopter:
                    SaveData.Instance.CurrentSave.helicopterCount += reward.amount;
                    break;
            }
        }

        // Step 4: ONE Save() call — this writes to disk AND fires OnDataChanged ONCE
        SaveData.Instance.Save();

        // Step 5: sync PowerUps in-game HUD (only exists in GameView scene, safe if null)
        if (PowerUps.Instance != null)
            PowerUps.Instance.LoadFromSave();

        Debug.Log($"[ShopManager] Purchase OK: {card.itemName} | Cost:{cost} | CoinsRewarded:{card.coinsReward}");
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────
    // CALLED BY SaveData.OnDataChanged
    // ─────────────────────────────────────────────────────────────────────

    private void OnSaveDataChanged()
    {
        RefreshShopCoinDisplay();
        RefreshAllCards();
    }

    // ─────────────────────────────────────────────────────────────────────
    // UI REFRESH
    // ─────────────────────────────────────────────────────────────────────

    public void RefreshShopCoinDisplay()
    {
        if (shopCoinText != null && SaveData.Instance != null)
            shopCoinText.text = SaveData.Instance.CurrentSave.currency.ToString();
    }

    public void RefreshAllCards()
    {
        for (int i = _registeredCards.Count - 1; i >= 0; i--)
        {
            if (_registeredCards[i] == null)
                _registeredCards.RemoveAt(i);  // clean up destroyed cards
            else
                _registeredCards[i].RefreshUI();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // OPEN / CLOSE  (called by your existing ShopPopup.cs buttons)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this from your open-shop button instead of or alongside ShopPopup.ShowShopPopup().
    /// Refreshes everything immediately when the shop opens.
    /// </summary>
    public void OnShopOpened()
    {
        RefreshShopCoinDisplay();
        RefreshAllCards();
    }
}
```

### 2. ShopItemCard.cs
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// ShopItemCard — Attach ONE of these to EVERY card prefab/GameObject in the shop.
///
/// CARD TYPES (set in Inspector):
///   CoinPack    → coinsReward > 0,  powerupRewards list is EMPTY
///   MegaPack    → coinsReward > 0,  powerupRewards list has entries
///   PowerupOnly → coinsReward = 0,  powerupRewards list has entries
///
/// IMPORTANT: Do NOT call SaveData methods yourself here.
///            Just configure the fields in the Inspector.
///            ShopManager.TryPurchase() handles ALL the save logic.
/// </summary>
public class ShopItemCard : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────
    // ENUMS
    // ─────────────────────────────────────────────────────────────────────

    public enum CardType
    {
        CoinPack,     // Gives only coins (no powerups)
        MegaPack,     // Gives coins + powerups
        PowerupOnly   // Gives only powerups (no coin reward)
    }

    public enum PowerupType
    {
        FillSlot,
        Rearrange,
        Helicopter
    }

    // ─────────────────────────────────────────────────────────────────────
    // POWERUP REWARD — one entry per powerup type you want in this pack
    // ─────────────────────────────────────────────────────────────────────

    [System.Serializable]
    public class PowerupReward
    {
        [Tooltip("Which powerup to grant")]
        public PowerupType powerupType;

        [Tooltip("How many of this powerup to add")]
        public int amount = 1;
    }

    // ─────────────────────────────────────────────────────────────────────
    // INSPECTOR FIELDS — configure each card in Unity Inspector
    // ─────────────────────────────────────────────────────────────────────

    [Header("─── Card Identity ───")]
    [Tooltip("Display name shown on the card  e.g. 'Gold Pack' or 'Jumbo Mega Pack'")]
    public string itemName = "Shop Item";

    [Tooltip("What kind of card is this?")]
    public CardType cardType = CardType.CoinPack;

    [Header("─── Cost (what player PAYS) ───")]
    [Tooltip("Coins the player must spend to buy this card")]
    public int coinCost = 100;

    [Header("─── Coin Reward (what player GETS) ───")]
    [Tooltip("Coins added to the player after purchase. Set 0 for PowerupOnly cards.")]
    public int coinsReward = 0;

    [Header("─── Powerup Rewards ───")]
    [Tooltip("Leave EMPTY for CoinPack. Add one entry per powerup type for MegaPack/PowerupOnly.")]
    public List<PowerupReward> powerupRewards = new List<PowerupReward>();

    [Header("─── UI References (assign in Inspector) ───")]
    [Tooltip("The BUY button on this card")]
    public Button buyButton;

    [Tooltip("TMP text that shows the cost  e.g. '100 Coins'")]
    public TextMeshProUGUI costText;

    [Tooltip("TMP text that auto-builds reward description  e.g. '+300 Coins\n+2 FillSlot'")]
    public TextMeshProUGUI rewardDescriptionText;

    [Tooltip("(Optional) TMP text showing the card's name")]
    public TextMeshProUGUI itemNameText;

    [Tooltip("(Optional) Icon image for this card")]
    public Image itemIconImage;

    // ─────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Wire the buy button immediately — safe to do in Awake
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void Start()
    {
        // Register with ShopManager (by Start(), ShopManager.Instance is definitely ready)
        TryRegister();
        RefreshUI();
    }

    private void OnEnable()
    {
        // Re-register every time the card/popup becomes active
        TryRegister();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.UnregisterCard(this);
    }

    private void TryRegister()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.RegisterCard(this);
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUY BUTTON
    // ─────────────────────────────────────────────────────────────────────

    private void OnBuyClicked()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.TryPurchase(this);
    }

    // ─────────────────────────────────────────────────────────────────────
    // UI REFRESH — called by ShopManager after every purchase / shop open
    // ─────────────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        // Item name label
        if (itemNameText != null)
            itemNameText.text = itemName;

        // Cost label
        if (costText != null)
            costText.text = coinCost + " Coins";

        // Reward description — auto-built from Inspector values
        if (rewardDescriptionText != null)
            rewardDescriptionText.text = BuildRewardDescription();

        // Buy button: interactable only if player has enough coins
        if (buyButton != null && SaveData.Instance != null)
        {
            int playerCoins = SaveData.Instance.CurrentSave.currency;
            buyButton.interactable = (playerCoins >= coinCost);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD REWARD TEXT — fully dynamic, driven by Inspector values
    // ─────────────────────────────────────────────────────────────────────

    private string BuildRewardDescription()
    {
        var sb = new System.Text.StringBuilder();

        // Coin reward line
        if (coinsReward > 0)
            sb.Append($"+{coinsReward} Coins");

        // Powerup reward lines
        foreach (var reward in powerupRewards)
        {
            if (sb.Length > 0) sb.Append("\n");

            switch (reward.powerupType)
            {
                case PowerupType.FillSlot:
                    sb.Append($"+{reward.amount} Fill Slot");
                    break;
                case PowerupType.Rearrange:
                    sb.Append($"+{reward.amount} Rearrange");
                    break;
                case PowerupType.Helicopter:
                    sb.Append($"+{reward.amount} Helicopter");
                    break;
            }
        }

        // Fallback if nothing is set
        if (sb.Length == 0)
            sb.Append("No Reward Set");

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────
    // INSUFFICIENT COINS FEEDBACK — shake animation
    // ─────────────────────────────────────────────────────────────────────

    public void PlayInsufficientFeedback()
    {
        // Shake only on X axis so it feels like a "no" shake
        transform.DOShakePosition(0.4f, new Vector3(12f, 0f, 0f), 20, 90f, false, true);
    }
}
```

### 3. ShopPopup.cs
```csharp
using System.Collections;
using UnityEngine;

/// <summary>
/// ShopPopup — handles the open/close animation of the Shop popup.
/// Also calls ShopManager.OnShopOpened() so all cards and coin text refresh instantly.
/// </summary>
public class ShopPopup : MonoBehaviour
{
    public UIPopupAnimator anim;

    public void ShowShopPopup()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);
        SoundManager.Instance.PlaySound(SoundManager.SoundName.PopupOpen);

        UIPopupManager.Instance.ShowPopup(UIPopupManager.UIPopupType.Shop);
        Time.timeScale = 0;

        if (anim != null)
            anim.Open();

        // Refresh coin display + all card buttons immediately when shop opens
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopOpened();
    }

    public void CloseShopPopup()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);
        SoundManager.Instance.PlaySound(SoundManager.SoundName.PopupClose);

        Time.timeScale = 1;

        if (anim != null)
            anim.Close();

        StartCoroutine(CloseAfterAnim());
    }

    private IEnumerator CloseAfterAnim()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        UIPopupManager.Instance.ClosePopup();
    }
}
```
