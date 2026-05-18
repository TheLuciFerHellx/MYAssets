using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ParkingGameManager : MonoBehaviour
{
    public static ParkingGameManager Instance;
    
    // List of all passengers waiting
    public List<Passenger> waitingPassengers = new List<Passenger>();
    public List<ParkingSlotManger> AllParkingSlotManager = new List<ParkingSlotManger>();
    
    public Vector3 lineStart = new Vector3(-2, 1, 38);
    public float stepSize = 1.5f; // Distance between passengers
    public int walkSpeedOfPassengers = 1;

    private bool gameOver = false;

    public bool isLevelLoading = true;

    public TextMeshProUGUI TotalPassengerCount;

    private bool isProcessingQueue = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Update()
    {
        //TotalPassengerCount.text = waitingPassengers.Count.ToString();
        if (TotalPassengerCount != null)
        {
            TotalPassengerCount.text =  waitingPassengers.Count.ToString();
        }
        for (int i = 0; i < waitingPassengers.Count; i++)  // to update passenger line 
        {
            Vector3 nextPos = lineStart + (Vector3.right * (i * stepSize));
        
        // Use MoveTowards or Lerp for smooth movement instead of "snapping"
            waitingPassengers[i].transform.position = Vector3.MoveTowards(waitingPassengers[i].transform.position, nextPos, Time.deltaTime * walkSpeedOfPassengers );
        }
    
        if (gameOver || isLevelLoading) return;

        // Optimization: Win condition check (can also be moved to an event-based check)
        if (waitingPassengers.Count == 0) 
        {
            gameOver = true;
            isLevelLoading = true;
            UnityEngine.Debug.Log("Level Complete - All passengers cleared!");
            SoundManager.Instance.PlaySound(SoundManager.SoundName.LevelComplete);
            // SoundManager.Instance.PlaySound(SoundManager.SoundName.PopupOpen);
            StartCoroutine(showLevelCompletePopup());
            // Time.timeScale=0;
            // LevelManager.Instance.CompleteLevel(); //Loding Next Level On Complete
            return;
        }
    }

    IEnumerator showLevelCompletePopup()
    {
        yield return new WaitForSeconds(1f);
        Time.timeScale=0;
        UIPopupManager.Instance.ShowPopup(UIPopupManager.UIPopupType.LevelComplete);
    }
    public void ResetForNewLevel()
    {
        gameOver = false;
        isLevelLoading = true; // Lock the check until spawning is done
        waitingPassengers.Clear(); // Clear any leftovers from the previous level
        // Add any other resets needed (e.g., clearing cars from slots)
    }
    public void OnCarParked(ColorOfCarAndPassengers type)
    {
        //  while (waitingPassengers.Count > 0)
        // {
        //     Passenger p = waitingPassengers[0];
        //     ParkingSlotManger matchingSlot = null;

        //     foreach (var slot in AllParkingSlotManager)
        //     {
        //         if (slot.isOccupied && slot.parkedCarType == p.passengertype)
        //         {
        //             matchingSlot = slot;
        //             break; // Found a match in slots, no need to check other slots
        //         }
        //     }

        //     if (matchingSlot != null)
        //     {
        //         GameObject carGO = matchingSlot.currentCar.gameObject;
        //         CarMover count = matchingSlot.currentCar.GetComponent<CarMover>(); 

        //         matchingSlot.currentCar.CapacityOfPassengers -= 1; 
        //         count.totalPassengerTxt.text = matchingSlot.currentCar.CapacityOfPassengers.ToString();
        //         Debug.Log("Deducted seat from: " + carGO.name);

        //         waitingPassengers.RemoveAt(0);
        //         ObjectPool.Instance.AddToPool(p.gameObject); // add to pool
        //         // Destroy(p.gameObject, 0.1f); //insted of destroy

        //         //==============================================================================
        //         // for (int i = 0; i < waitingPassengers.Count; i++) 
        //         // {
        //         //     // Move object at index i to the 3D position corresponding to that index
        //         //     waitingPassengers[i].transform.position = anchorPoints[i].position;

        //         //     Vector3 nextPos = lineStart + (Vector3.back * (i * stepSize));
        //         //     waitingPassengers[i].transform.position = nextPos;
        //         // }
        //         //==============================================================================

        //         if (matchingSlot.currentCar.CapacityOfPassengers <= 0)
        //         {
        //             // 1. Get the CarMover component
        //             var car = matchingSlot.currentCar;

        //             // 2. Fix: Use assignment '=' to change the type
        //             car.carType = ColorOfCarAndPassengers.None; //|================================

        //             // carGO.gameObject.SetActive(false);
        //             Debug.Log("Car is full! Drive away.");
        //             matchingSlot.currentCar.DriveAway(); // move full car

        //             // 3. IMPORTANT: Reset the slot so it's not "Occupied" forever
        //             matchingSlot.isOccupied = false;
        //             matchingSlot.currentCar = null;
        //             matchingSlot.parkedCarType = ColorOfCarAndPassengers.None; //|================================

        //         }
        //     }
        //     else
        //     {
        //         GameOver();
        //         // If the person at index 0 has NO match in any slot, STOP the whole process
        //         break; 
        //     }
        // }
        //StartCoroutine(ProcessQueue());
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    // IEnumerator ProcessQueue()
    // {
    //     while (waitingPassengers.Count > 0)
    //     {
    //         Passenger p = waitingPassengers[0];
    //         ParkingSlotManger matchingSlot = null;

    //         foreach (var slot in AllParkingSlotManager)
    //         {
    //             if (slot.isOccupied && slot.parkedCarType == p.passengertype)
    //             {
    //                 matchingSlot = slot;
    //                 break; // Found a match in slots, no need to check other slots
    //             }
    //         }

    //         if (matchingSlot != null)
    //         {
    //             GameObject carGO = matchingSlot.currentCar.gameObject;
    //             CarMover count = matchingSlot.currentCar.GetComponent<CarMover>(); 

    //             matchingSlot.currentCar.CapacityOfPassengers -= 1; 
    //             SoundManager.Instance.PlaySound(SoundManager.SoundName.PassengerAdd);
    //             count.totalPassengerTxt.text = matchingSlot.currentCar.CapacityOfPassengers.ToString();
    //             Debug.Log("Deducted seat from: " + carGO.name);

    //             waitingPassengers.RemoveAt(0);
                
    //             while (Vector3.Distance(p.transform.position, carGO.transform.position) > 0.1f && matchingSlot.currentCar.CapacityOfPassengers != 0)
    //             {
    //                 p.transform.position = Vector3.MoveTowards(p.transform.position, carGO.transform.position, Time.deltaTime * 100);
    //                 yield return null; 
    //             }
    //             yield return new WaitForSeconds(0.08f); 
    //             ObjectPool.Instance.AddToPool(p.gameObject); // add to pool
    //             // Destroy(p.gameObject, 0.1f); //insted of destroy
                
    //             if (matchingSlot.currentCar != null && matchingSlot.currentCar.CapacityOfPassengers <= 0)
    //             {
    //                 // 1. Get the CarMover component
    //                 var car = matchingSlot.currentCar;

    //                 // 2. Fix: Use assignment '=' to change the type
    //                 car.carType = ColorOfCarAndPassengers.None; //|================================

    //                 // carGO.gameObject.SetActive(false);
    //                 Debug.Log("Car is full! Drive away.");
    //                 matchingSlot.currentCar.DriveAway(); // move full car

    //                 // 3. IMPORTANT: Reset the slot so it's not "Occupied" forever
    //                 matchingSlot.isOccupied = false;
    //                 matchingSlot.currentCar = null;
    //                 matchingSlot.parkedCarType = ColorOfCarAndPassengers.None; //|================================

    //             }
    //         }
    //         else
    //         {
    //             GameOver();
    //             // If the person at index 0 has NO match in any slot, STOP the whole process
    //             break; 
    //         }
    //     }
    // }

    IEnumerator ProcessQueue() 
    {
        isProcessingQueue = true;
        // Use a while loop but ensure we yield to prevent freezing the game
        while (waitingPassengers.Count > 0) {
            Passenger p = waitingPassengers[0];
            ParkingSlotManger matchingSlot = null;

            // Find match
            foreach (var slot in AllParkingSlotManager) {
                if (slot.isOccupied && slot.currentCar != null && slot.parkedCarType == p.passengertype) {
                    // Check if car still has room (safety check)
                    if (slot.currentCar.CapacityOfPassengers > 0) {
                        matchingSlot = slot;
                        break;
                    }
                }
            }

            if (matchingSlot != null) {
                // 1. Claim the seat immediately so other passengers don't "overfill" it
                var car = matchingSlot.currentCar;
                car.CapacityOfPassengers -= 1;
                //car.GetComponent<CarMover>().totalPassengerTxt.text = car.CapacityOfPassengers.ToString();
                car.totalPassengerTxt.text = car.CapacityOfPassengers.ToString();
                
                // Remove passenger from list so the next one in queue can start thinking
                waitingPassengers.RemoveAt(0);

                // 2. Move Passenger
                SoundManager.Instance.PlaySound(SoundManager.SoundName.PassengerAdd);
                Vector3 targetPos = car.transform.position;
                
                while (p != null && Vector3.Distance(p.transform.position, targetPos) > 0.1f) {
                    p.transform.position = Vector3.MoveTowards(p.transform.position, targetPos, Time.deltaTime * 100);
                    yield return null;
                }

                // 3. Cleanup Passenger
                if(p != null) ObjectPool.Instance.AddToPool(p.gameObject);
                yield return new WaitForSeconds(0.05f);

                // 4. Handle Full Car
                if (car.CapacityOfPassengers <= 0) {
                    matchingSlot.ClearSlot(); 
                    car.carType = ColorOfCarAndPassengers.None;
                    car.DriveAway();

                    
                    //yield return new WaitForSeconds(1f); 
                    // Clear the slot
                    // matchingSlot.isOccupied = false;
                    // matchingSlot.currentCar = null;
                    // matchingSlot.parkedCarType = ColorOfCarAndPassengers.None;
                }
            } 
            else {
                // No match for the front of the line
                isProcessingQueue = false;
                GameOver(); 
                yield break; // Exit coroutine entirely 
            }
            
            // Small gap between processing the next passenger in queue
            yield return new WaitForSeconds(0.1f);
        }
        isProcessingQueue = false;
    }

    public ParkingSlotManger FindEmptyParkingSlot()
    {
        foreach (var slotObj in AllParkingSlotManager)
        {
            ParkingSlotManger slot = slotObj.GetComponent<ParkingSlotManger>();
            // if (slot != null && !slot.isOccupied && slot.isOccupied == false)
            // {
            //     return slotObj; // Return the first free slot we find
            // }
            if (slot != null && !slot.isOccupied && !slot.isReserved)
            {
                return slot; // Corrected: Return the component, not the GameObject
            }
        }
        return null; // All spots are full 
    }

    public void GameOver()
    {
        // if (gameOver) return;

        // 1. Check if all spots are occupied
        bool allSpotsFull = true;
        foreach (var slotObj in AllParkingSlotManager)
        {
            // if (!slotObj.GetComponent<ParkingSlotManger>().isOccupied)
            // {
            //     allSpotsFull = false;
            //     break;
            // }
             if (!slotObj.isOccupied || slotObj.currentCar == null)
            {
                allSpotsFull = false;
                break;
            }
        }

        // 2. Win Condition: No passengers left (Always a win)
        if (waitingPassengers.Count == 0)
        {
            gameOver = true;
            isLevelLoading = true;
            UnityEngine.Debug.Log("Level Complete - All passengers cleared!");
            return; // Exit so we don't trigger Game Over logic
        }

        // 3. Lose Condition: Board full AND passengers still waiting
        if (allSpotsFull && waitingPassengers.Count > 0)
        {
            gameOver = true;
            isLevelLoading = true;
            SoundManager.Instance.PlaySound(SoundManager.SoundName.GameOver);
            // SoundManager.Instance.PlaySound(SoundManager.SoundName.PopupOpen);
            UIPopupManager.Instance.ShowPopup(UIPopupManager.UIPopupType.GameOver);
            Time.timeScale=0;
            UnityEngine.Debug.Log("GAME OVER - No spots left and passengers waiting!");
        }
    }

    // public void OnCarParked(CarType type)
    // {
    //     // if (waitingPassengers.Count > 0 && waitingPassengers[0].passengerColor == carColor)
    //     // {
    //     //     Debug.Log("Passenger " + waitingPassengers[0].name + " matched!");
    //     //     Destroy(waitingPassengers[0].gameObject);
    //     //     waitingPassengers.RemoveAt(0);
    //     //     // Handle next passenger in queue...
    //     // }

    //     // while (waitingPassengers.Count > 0)
    //     // {
    //     // // Compare color of the first passenger in line
    //     //     if (waitingPassengers[0].passengertype == type)
    //     //     {
    //     //         Debug.Log("Match found!");
                
    //     //         // 1. Get reference to the passenger
    //     //         Passenger p = waitingPassengers[0];

    //     //         // 2. Remove from list FIRST
    //     //         waitingPassengers.RemoveAt(0);

    //     //         // 3. Destroy the GameObject at the end of the frame
    //     //         // This prevents the Inspector from looking for a dead object
    //     //         Destroy(p.gameObject, 0.1f); 
    //     //     }
    //     // }

    //     // for (int i = 0; i <= waitingPassengers.Count; i++) 
    //     // {
    //     //     if (waitingPassengers[0].passengertype == type) 
    //     //     {
    //     //         Passenger p = waitingPassengers[0];
    //     //         waitingPassengers.RemoveAt(0);
    //     //         Destroy(p.gameObject, 0.1f);
    //     //     }
    //     // }

    //     // while (waitingPassengers.Count > 0 && waitingPassengers[0].passengertype == type) 
    //     // {
    //     //     Passenger p = waitingPassengers[0];
            
    //     //     // Remove from the list immediately so the NEXT passenger becomes index [0]
    //     //     waitingPassengers.RemoveAt(0);
            
    //     //     // Destroy the physical object
    //     //     Destroy(p.gameObject, 0.1f);
    //     // }

    //     // for(int i=0; i<AllParkingSlotManager.Count; i++)
    //     // {
    //     //     if(waitingPassengers.Count > 0 && waitingPassengers[0].passengertype == type)
    //     //     {
    //     //         Passenger p = waitingPassengers[0];
            
    //     //         // Remove from the list immediately so the NEXT passenger becomes index [0]
    //     //         waitingPassengers.RemoveAt(0);
                
    //     //         // Destroy the physical object
    //     //         Destroy(p.gameObject, 0.1f);
    //     //     }
    //     // }

    //     // Keep running as long as there is someone in line
    //     while (waitingPassengers.Count > 0)
    //     {
    //         Passenger p = waitingPassengers[0];
    //         // bool matchFound = false;
    //         ParkingSlotManger matchingSlot = null;

    //         // Look through all slots to see if ANY slot matches this specific passenger
    //         foreach (var slot in AllParkingSlotManager)
    //         {
    //             if (slot.isOccupied && slot.parkedCarType == p.passengertype)
    //             {
    //                 // matchFound = true;
    //                 matchingSlot = slot;
    //                 break; // Found a match in slots, no need to check other slots
    //             }
    //         }

    //         // if (matchFound)
    //         // {
    //         //     // Remove the current index 0
    //         //     waitingPassengers.RemoveAt(0);
    //         //     Destroy(p.gameObject, 0.1f);


    //         // }
    //         if (matchingSlot != null)
    //         {
    //             // 1. GET THE CAR GAME OBJECT
    //             GameObject carGO = matchingSlot.currentCar.gameObject;
                
    //             // 2. ACCESS VALUES TO DEDUCT
    //             // Assuming your CarMover has a 'seats' variable
    //             matchingSlot.currentCar.CapacityOfPassengers -= 1; 
    //             Debug.Log("Deducted seat from: " + carGO.name);

    //             // 3. REMOVE PASSENGER
    //             waitingPassengers.RemoveAt(0);
    //             Destroy(p.gameObject, 0.1f);

    //             // 4. (Optional) If car is full, you could move it away
    //             if (matchingSlot.currentCar.CapacityOfPassengers <= 0)
    //             {
    //                 carGO.gameObject.SetActive(false);
    //                 Debug.Log("Car is full! Drive away.");
    //                 // matchingSlot.currentCar.DriveAway();
    //             }
    //         }
    //         else
    //         {
    //             // If the person at index 0 has NO match in any slot, STOP the whole process
    //             break; 
    //         }
    //     }
    // }
}
