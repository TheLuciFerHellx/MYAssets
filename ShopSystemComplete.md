# 🛒 Complete Shop System Guide — Car OUT Jam Puzzle Game

> **Written by:** Pro Unity Developer (20+ years)  
> **Date:** June 2026  
> **DO NOT modify existing scripts.** All new scripts are fully compatible with your save system, PowerUps.cs, and GameDataLoader.cs.

---

## 📦 What This System Gives You

| Feature | Description |
|---|---|
| **Coin Packs** | Cards to buy coins (e.g. "Gold Pack = 500 coins") |
| **Mega Packs** | Cards that give coins + powerups together (e.g. "Jumbo Pack = 300 coins + 2 FillSlot + 1 Heli") |
| **Full Save Integration** | Purchases write directly to your existing `SaveData.cs` / `GameSaveData` |
| **Dynamic Cards** | Each card is a prefab with a `ShopItemCard.cs` script. You set what it gives in the Inspector |
| **ShopManager** | Central manager; cards register themselves — no hardcoding |
| **PowerUps.cs Sync** | After purchase, `PowerUps.Instance.LoadFromSave()` is called automatically |
| **GameDataLoader Sync** | `SaveData.OnDataChanged` fires automatically; all HUD texts update |

---

## 🗂️ Files To Create

```
Assets/Script/Shop/
    ShopManager.cs        ← Central shop controller (Singleton)
    ShopItemCard.cs       ← Attach to every card prefab in Inspector
```

> That's it! Only 2 new scripts. Everything else wires in through your existing save/powerup system.

---

## 📄 SCRIPT 1: `ShopManager.cs`

**Path:** `Assets/Script/Shop/ShopManager.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central Shop Manager — Singleton.
/// Handles opening/closing the shop popup and coin display.
/// Each ShopItemCard registers itself; ShopManager just orchestrates.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Shop UI References")]
    [Tooltip("Assign the root ShopPanel GameObject (the one you show/hide)")]
    public GameObject shopPanel;

    [Tooltip("TMP text that shows the player's current coin count inside the shop")]
    public TextMeshProUGUI coinCountText;

    [Tooltip("(Optional) UIPopupAnimator for open/close animation — same as your other popups")]
    public UIPopupAnimator anim;

    // All cards in the scene register themselves here automatically
    private List<ShopItemCard> registeredCards = new List<ShopItemCard>();

    // ──────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ──────────────────────────────────────────────────────────────

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
        SaveData.OnDataChanged += RefreshCoinDisplay;
    }

    private void OnDisable()
    {
        SaveData.OnDataChanged -= RefreshCoinDisplay;
    }

    private void Start()
    {
        RefreshCoinDisplay();
    }

    // ──────────────────────────────────────────────────────────────
    // CARD REGISTRATION  (cards call this themselves in their Awake)
    // ──────────────────────────────────────────────────────────────

    public void RegisterCard(ShopItemCard card)
    {
        if (!registeredCards.Contains(card))
            registeredCards.Add(card);
    }

    public void UnregisterCard(ShopItemCard card)
    {
        registeredCards.Remove(card);
    }

    // ──────────────────────────────────────────────────────────────
    // OPEN / CLOSE
    // ──────────────────────────────────────────────────────────────

    public void OpenShop()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);
            SoundManager.Instance.PlaySound(SoundManager.SoundName.PopupOpen);
        }

        UIPopupManager.Instance.ShowPopup(UIPopupManager.UIPopupType.Shop);
        Time.timeScale = 0f;

        if (anim != null)
            anim.Open();

        RefreshCoinDisplay();
        RefreshAllCards();
    }

    public void CloseShop()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);
            SoundManager.Instance.PlaySound(SoundManager.SoundName.PopupClose);
        }

        Time.timeScale = 1f;

        if (anim != null)
            anim.Close();

        StartCoroutine(CloseAfterAnim());
    }

    private IEnumerator CloseAfterAnim()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        UIPopupManager.Instance.ClosePopup();
    }

    // ──────────────────────────────────────────────────────────────
    // PURCHASE HANDLER  (called by ShopItemCard when player taps Buy)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to process a purchase for the given card.
    /// Returns TRUE if purchase was successful.
    /// </summary>
    public bool TryPurchase(ShopItemCard card)
    {
        if (SaveData.Instance == null) return false;

        int playerCoins = SaveData.Instance.CurrentSave.currency;
        int cost        = card.coinCost;

        // ── Not enough coins ──
        if (playerCoins < cost)
        {
            Debug.LogWarning($"[ShopManager] Not enough coins! Have {playerCoins}, need {cost}");
            card.PlayInsufficientFeedback(); // shake animation on the card
            return false;
        }

        // ── Deduct cost ──
        SaveData.Instance.AddCurrency(-cost);

        // ── Add coins reward (Coin Pack / part of Mega Pack) ──
        if (card.coinsReward > 0)
            SaveData.Instance.AddCurrency(card.coinsReward);

        // ── Add powerup rewards ──
        foreach (var reward in card.powerupRewards)
        {
            switch (reward.powerupType)
            {
                case ShopItemCard.PowerupType.FillSlot:
                    int newFill = SaveData.Instance.CurrentSave.fillSlotCount + reward.amount;
                    SaveData.Instance.SetFillSlotCount(newFill);
                    break;

                case ShopItemCard.PowerupType.Rearrange:
                    int newRearrange = SaveData.Instance.CurrentSave.rearrangeCount + reward.amount;
                    SaveData.Instance.SetRearrangeCount(newRearrange);
                    break;

                case ShopItemCard.PowerupType.Helicopter:
                    int newHeli = SaveData.Instance.CurrentSave.helicopterCount + reward.amount;
                    SaveData.Instance.SetHelicopterCount(newHeli);
                    break;
            }
        }

        // ── Persist everything ──
        SaveData.Instance.Save();

        // ── Sync the in-game PowerUps HUD ──
        if (PowerUps.Instance != null)
            PowerUps.Instance.LoadFromSave();

        // ── Refresh all shop cards ──
        RefreshAllCards();
        RefreshCoinDisplay();

        Debug.Log($"[ShopManager] Purchased: {card.itemName}");
        return true;
    }

    // ──────────────────────────────────────────────────────────────
    // UI REFRESH
    // ──────────────────────────────────────────────────────────────

    public void RefreshCoinDisplay()
    {
        if (coinCountText != null && SaveData.Instance != null)
            coinCountText.text = SaveData.Instance.CurrentSave.currency.ToString();
    }

    public void RefreshAllCards()
    {
        foreach (var card in registeredCards)
        {
            if (card != null)
                card.RefreshUI();
        }
    }
}
```

---

## 📄 SCRIPT 2: `ShopItemCard.cs`

**Path:** `Assets/Script/Shop/ShopItemCard.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Attach this to every Shop Card prefab/GameObject in your Shop popup.
///
/// CARD TYPES:
///   CoinPack  — gives only coins (coinsReward > 0, no powerupRewards)
///   MegaPack  — gives coins + powerups (coinsReward > 0, powerupRewards has entries)
///   PowerupOnly — gives only powerups (coinsReward = 0, powerupRewards has entries)
///
/// All configuration is done in the Inspector — fully dynamic!
/// </summary>
public class ShopItemCard : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    // ENUMS
    // ──────────────────────────────────────────────────────────────

    public enum CardType
    {
        CoinPack,    // Only coins
        MegaPack,    // Coins + Powerups
        PowerupOnly  // Only powerups (no coin reward)
    }

    public enum PowerupType
    {
        FillSlot,
        Rearrange,
        Helicopter
    }

    // ──────────────────────────────────────────────────────────────
    // POWERUP REWARD ENTRY  (one entry per powerup type in the pack)
    // ──────────────────────────────────────────────────────────────

    [System.Serializable]
    public class PowerupReward
    {
        [Tooltip("Which powerup to add")]
        public PowerupType powerupType;

        [Tooltip("How many of this powerup to add on purchase")]
        public int amount = 1;
    }

    // ──────────────────────────────────────────────────────────────
    // INSPECTOR FIELDS
    // ──────────────────────────────────────────────────────────────

    [Header("─── Card Identity ───")]
    [Tooltip("Display name shown on the card (e.g. 'Gold Pack', 'Jumbo Mega Pack')")]
    public string itemName = "Shop Item";

    [Tooltip("What kind of card is this?")]
    public CardType cardType = CardType.CoinPack;

    [Header("─── Cost ───")]
    [Tooltip("How many coins the player must spend to buy this")]
    public int coinCost = 100;

    [Header("─── Coin Reward ───")]
    [Tooltip("How many coins this card gives. Set 0 for PowerupOnly cards.")]
    public int coinsReward = 0;

    [Header("─── Powerup Rewards ───")]
    [Tooltip("Leave empty for CoinPack. Add entries for MegaPack or PowerupOnly.")]
    public List<PowerupReward> powerupRewards = new List<PowerupReward>();

    [Header("─── UI References ───")]
    [Tooltip("The main buy button on this card")]
    public Button buyButton;

    [Tooltip("Shows the cost (e.g. '100 Coins')")]
    public TextMeshProUGUI costText;

    [Tooltip("Shows what the player gets (e.g. '+500 Coins' or '+300 Coins +2 FillSlot')")]
    public TextMeshProUGUI rewardDescriptionText;

    [Tooltip("(Optional) Item name label")]
    public TextMeshProUGUI itemNameText;

    [Tooltip("(Optional) Icon for this card")]
    public Image itemIcon;

    // ──────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Register with ShopManager
        if (ShopManager.Instance != null)
            ShopManager.Instance.RegisterCard(this);
    }

    private void OnEnable()
    {
        // Re-register if ShopManager was loaded after this card
        if (ShopManager.Instance != null)
            ShopManager.Instance.RegisterCard(this);

        RefreshUI();
    }

    private void OnDisable()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.UnregisterCard(this);
    }

    private void Start()
    {
        // Wire up the buy button
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        RefreshUI();
    }

    // ──────────────────────────────────────────────────────────────
    // BUY BUTTON HANDLER
    // ──────────────────────────────────────────────────────────────

    private void OnBuyClicked()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.TryPurchase(this);
    }

    // ──────────────────────────────────────────────────────────────
    // UI REFRESH  (called by ShopManager whenever data changes)
    // ──────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        // Item name
        if (itemNameText != null)
            itemNameText.text = itemName;

        // Cost
        if (costText != null)
            costText.text = coinCost + " Coins";

        // Reward description — build dynamically
        if (rewardDescriptionText != null)
            rewardDescriptionText.text = BuildRewardDescription();

        // Buy button interactable check
        if (buyButton != null && SaveData.Instance != null)
        {
            int playerCoins = SaveData.Instance.CurrentSave.currency;
            buyButton.interactable = playerCoins >= coinCost;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // BUILD REWARD TEXT  (auto-generates based on Inspector settings)
    // ──────────────────────────────────────────────────────────────

    private string BuildRewardDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Coins reward
        if (coinsReward > 0)
            sb.Append($"+{coinsReward} Coins");

        // Powerup rewards
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

        return sb.ToString();
    }

    // ──────────────────────────────────────────────────────────────
    // FEEDBACK ANIMATION  (shake card if not enough coins)
    // ──────────────────────────────────────────────────────────────

    public void PlayInsufficientFeedback()
    {
        transform.DOShakePosition(0.4f, new Vector3(10f, 0f, 0f), 20, 90f);
    }
}
```

---

## ⚙️ Unity Setup Instructions

### Step 1 — Create the Scripts

1. In Unity, go to `Assets/Script/`
2. Create a new folder: **`Shop`**
3. Inside it, create two C# scripts:
   - `ShopManager.cs` → paste Script 1 above
   - `ShopItemCard.cs` → paste Script 2 above

---

### Step 2 — ShopManager GameObject Setup

1. In your Shop popup hierarchy (inside Canvas), create an **empty GameObject** named `ShopManager`
2. Drag **`ShopManager.cs`** onto it
3. In the Inspector, fill in:

| Field | What to assign |
|---|---|
| `Shop Panel` | The root GameObject of your shop popup |
| `Coin Count Text` | TMP text showing coin count inside the shop |
| `Anim` | (Optional) The `UIPopupAnimator` component on the shop panel |

> The `ShopPopup.cs` you already have can call `ShopManager.Instance.OpenShop()` and `ShopManager.Instance.CloseShop()` — or just keep using it as-is and let it control the UIPopupManager; both work together.

---

### Step 3 — Create Shop Card Prefabs

For each item you want to sell, create a UI card (Image + children). A typical card layout:

```
ShopCard (GameObject)
├── Background (Image)
├── ItemIcon (Image)                ← drag into ShopItemCard.itemIcon
├── ItemNameText (TMP)              ← drag into ShopItemCard.itemNameText
├── RewardDescriptionText (TMP)     ← drag into ShopItemCard.rewardDescriptionText
├── CostText (TMP)                  ← drag into ShopItemCard.costText
└── BuyButton (Button)              ← drag into ShopItemCard.buyButton
```

Attach `ShopItemCard.cs` to the root `ShopCard` GameObject.

---

### Step 4 — Configure Each Card in the Inspector

#### 🟡 Gold Pack (Coins Only)

| Field | Value |
|---|---|
| Item Name | `Gold Pack` |
| Card Type | `CoinPack` |
| Coin Cost | `50` |
| Coins Reward | `200` |
| Powerup Rewards | *(empty list)* |

---

#### 💎 Jumbo Mega Pack (Coins + 2 Powerups)

| Field | Value |
|---|---|
| Item Name | `Jumbo Mega Pack` |
| Card Type | `MegaPack` |
| Coin Cost | `150` |
| Coins Reward | `300` |
| Powerup Rewards | Add **2 entries**: |

**Powerup Rewards List for Jumbo Mega Pack:**

| Index | Powerup Type | Amount |
|---|---|---|
| [0] | `FillSlot` | `2` |
| [1] | `Helicopter` | `1` |

---

#### ⭐ Diamond Mega Pack (Coins + Different Powerups)

| Field | Value |
|---|---|
| Item Name | `Diamond Mega Pack` |
| Card Type | `MegaPack` |
| Coin Cost | `250` |
| Coins Reward | `500` |
| Powerup Rewards | Add entries as you like |

---

#### 🔧 FillSlot Pack (Powerup Only)

| Field | Value |
|---|---|
| Item Name | `Fill Slot x3` |
| Card Type | `PowerupOnly` |
| Coin Cost | `80` |
| Coins Reward | `0` |
| Powerup Rewards | 1 entry: `FillSlot`, Amount `3` |

---

### Step 5 — Place Cards in the Shop Panel

Put all your card GameObjects inside the Shop popup's **Content** object (inside a Scroll View with Vertical Layout Group). Unity Inspector example:

```
ShopPopup (GameObject)
└── ShopPanel
    ├── Header ("SHOP")
    ├── CoinDisplay (TMP: shows player coins)
    ├── CloseButton
    └── ScrollView
        └── Viewport
            └── Content (Vertical Layout Group + Content Size Fitter)
                ├── GoldPackCard       ← ShopItemCard attached
                ├── JumboMegaPackCard  ← ShopItemCard attached
                ├── DiamondMegaCard    ← ShopItemCard attached
                └── FillSlotPackCard   ← ShopItemCard attached
```

---

### Step 6 — Wire the Open Button

On any button in your MainMenu or HUD that should open the shop:

- **On Click ()** → drag `ShopManager` GameObject → select `ShopManager.OpenShop()`

OR, if you are using `ShopPopup.cs` already, replace the contents of `ShowShopPopup()` with:

```csharp
public void ShowShopPopup()
{
    SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);
    SoundManager.Instance.PlaySound(SoundManager.SoundName.PopupOpen);
    UIPopupManager.Instance.ShowPopup(UIPopupManager.UIPopupType.Shop);
    Time.timeScale = 0;
    if (anim != null) anim.Open();
    ShopManager.Instance.RefreshAllCards();
    ShopManager.Instance.RefreshCoinDisplay();
}
```

---

## 🔄 How Data Flows (Full Purchase Flow)

```
Player taps BUY on a card
        ↓
ShopItemCard.OnBuyClicked()
        ↓
ShopManager.TryPurchase(card)
        ↓
Check: playerCoins >= card.coinCost?
   ├─ NO  → card.PlayInsufficientFeedback() (shake)
   └─ YES ↓
        ↓
SaveData.AddCurrency(-cost)          ← deduct cost
SaveData.AddCurrency(+coinsReward)   ← add coin reward (if any)
SaveData.SetFillSlotCount(...)       ← add powerup rewards (if any)
SaveData.SetRearrangeCount(...)
SaveData.SetHelicopterCount(...)
SaveData.Save()                      ← write to JSON on disk
        ↓
SaveData.OnDataChanged fires
        ↓
GameDataLoader.LoadAllData()         ← HUD coin text auto-updates
PowerUps.Instance.LoadFromSave()     ← PowerUp HUD counts update
ShopManager.RefreshAllCards()        ← All card buy buttons enable/disable
ShopManager.RefreshCoinDisplay()     ← Shop coin text updates
```

---

## 📋 Quick Reference: Example Pack Setups

| Pack Name | Cost | Coins Given | FillSlot | Rearrange | Helicopter |
|---|---|---|---|---|---|
| Starter Gold | 30 | 100 | 0 | 0 | 0 |
| Gold Pack | 50 | 200 | 0 | 0 | 0 |
| Mega Pack | 100 | 100 | 1 | 1 | 0 |
| Jumbo Mega Pack | 150 | 300 | 2 | 0 | 1 |
| Diamond Mega Pack | 250 | 500 | 2 | 2 | 1 |
| FillSlot x3 | 80 | 0 | 3 | 0 | 0 |
| Rearrange x3 | 80 | 0 | 0 | 3 | 0 |
| Helicopter x2 | 120 | 0 | 0 | 0 | 2 |

All of these are set purely in the Inspector — **no code changes needed.**

---

## ✅ Checklist Before Testing

- [ ] Created `Assets/Script/Shop/ShopManager.cs` and pasted Script 1
- [ ] Created `Assets/Script/Shop/ShopItemCard.cs` and pasted Script 2
- [ ] `ShopManager` GameObject exists inside Canvas/ShopPopup
- [ ] `coinCountText` reference is assigned in `ShopManager` Inspector
- [ ] Each card has `ShopItemCard.cs` attached
- [ ] Each card's `buyButton`, `costText`, `rewardDescriptionText` are assigned in Inspector
- [ ] Each card has `coinCost`, `coinsReward`, `powerupRewards` set in Inspector
- [ ] A button calls `ShopManager.Instance.OpenShop()` or `ShopPopup.ShowShopPopup()`
- [ ] `UIPopupManager` has `Shop` type registered with your ShopPopup UIBase

---

## 🧩 Compatibility Notes

| Your Script | Compatible? | Notes |
|---|---|---|
| `SaveData.cs` | ✅ 100% | Uses `AddCurrency`, `SetFillSlotCount`, `SetRearrangeCount`, `SetHelicopterCount`, `Save()` — all existing methods |
| `GameDataLoader.cs` | ✅ Auto | Listens to `SaveData.OnDataChanged` already — will auto-refresh |
| `PowerUps.cs` | ✅ Auto | `LoadFromSave()` called after every purchase |
| `ShopPopup.cs` | ✅ Works alongside | Keep existing open/close or call `ShopManager` methods |
| `UIPopupManager.cs` | ✅ | Shop type already registered in your enum |
| `DG.Tweening` | ✅ | Used for shake feedback — already in your project |
| `SoundManager.cs` | ✅ | Click/PopupOpen/PopupClose sounds used |

---

## 💡 Pro Tips

1. **Want to add a new pack?** Just drop a new card GameObject into the Content, attach `ShopItemCard.cs`, fill the Inspector. Done. No ShopManager code changes ever needed.

2. **Want to disable a card temporarily?** Uncheck `GameObject.SetActive(false)` — cards auto-unregister from ShopManager on `OnDisable`.

3. **Want to show "SOLD OUT" or limit purchases?** Add a `public int maxPurchaseCount = -1;` field to `ShopItemCard` and a `private int purchasesDone` counter that checks against it in `OnBuyClicked`. Optionally save that count per item to `SaveData`.

4. **Want animated coin fly-out?** In `ShopManager.TryPurchase()`, after `SaveData.Instance.Save()`, start a coroutine that spawns coin UI objects flying toward the coin counter — purely cosmetic.

5. **Want sale prices?** Add `public int salePrice = -1;` to `ShopItemCard` and in `RefreshUI()` use `salePrice >= 0 ? salePrice : coinCost` for the effective cost.
