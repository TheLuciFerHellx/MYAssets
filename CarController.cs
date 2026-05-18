using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum ColorOfCarAndPassengers { Red, Blue, Green, white, oranage, None}

public class CarController : MonoBehaviour
{
    // public List<GameObject> parkingSpot;
    // private GameObject selectedCar;
    // public int speed = 20;
    public ParkingGameManager parkingGameManager;
    // private int currentSpotIndex = 0;

    // Update is called once per frame
    void Update()
    {

        if(Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) // toprevent click on ui
        {
            HandleCarSelection();
        }     

        // if (selectedCar != null) 
        // {
        //     Vector3 direction = (parkingSpot[0].transform.position - selectedCar.transform.position).normalized;
        //     direction.y = 0;
        //     if(direction != Vector3.zero)
        //     {
        //         Quaternion targetRotation = Quaternion.LookRotation(direction);
        //         selectedCar.transform.rotation = Quaternion.Slerp(selectedCar.transform.rotation , targetRotation, 5f * Time.deltaTime);
        //     }
        //     selectedCar.transform.position = Vector3.MoveTowards(selectedCar.transform.position, parkingSpot[0].transform.position, speed * Time.deltaTime );

        //     if (selectedCar.transform.position == parkingSpot[0].transform.position) 
        //     {
        //         selectedCar = null;
        //     }
        // }
    }

    // void HandleCarSelection()
    // {
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hit;
    //         if(Physics.Raycast(ray ,out hit))
    //         {
    //             if(hit.collider.CompareTag("car"))
    //             {

    //                 Carout obstacleDetector = hit.collider.GetComponent<Carout>();

    //                 // GameObject emptySpot = FindEmptyParkingSlot();

    //                 ParkingSlotManger emptySlot = FindEmptyParkingSlot();


    //                 if (obstacleDetector != null && obstacleDetector.isBlocked)
    //                 {
    //                     UnityEngine.Debug.Log("Cannot move: Car in front!");
    //                     obstacleDetector.DoTackle();
    //                     SoundManager.Instance.PlaySound(SoundManager.SoundName.CarDash);
    //                     return; // Stop here, don't move the car
    //                 }

                    

    //                 // CarMover mover = hit.collider.GetComponent<CarMover>();

    //                 // if (mover != null)
    //                 // {
    //                 //     mover.SetDestination(parkingSpot[currentSpotIndex].transform.position);
    //                 //     currentSpotIndex++;
    //                 // }

    //                 if (emptySlot != null)
    //                 {
    //                     CarMover mover = hit.collider.GetComponent<CarMover>();
    //                     if (mover != null)
    //                     {
    //                         emptySlot.isReserved = true; 
    //                         mover.isParked = true;
    //                         // 2. Send the car to that spot's position
    //                         mover.SetDestination(emptySlot.transform.position);
    //                         // SoundManager.Instance.PlaySound(SoundManager.SoundName.CarMove);
                            
    //                         // Note: isOccupied will be set to TRUE automatically 
    //                         // by your OnTriggerEnter once the car physically arrives.
    //                     }
    //                 }
    //                 else
    //                 {

    //                     UnityEngine.Debug.Log("No empty slots available right now!");
    //                 }

    //                 // if(emptySpot == null)
    //                 // {
    //                 //     GameOver();
    //                 // }

    //             }
    //         }
    // }

    void HandleCarSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("car"))
            {
                CarMover clickedMover = hit.collider.GetComponent<CarMover>();
                if (clickedMover != null && clickedMover.isParked) return; // FIX: Prevent clicking already moving cars!

                Carout obstacleDetector = hit.collider.GetComponent<Carout>();
                
                // NEW: Manually trigger the raycast check before deciding to move
                bool blocked = obstacleDetector != null && obstacleDetector.CheckForBlockage();

                if (blocked)
                {
                    UnityEngine.Debug.Log("Cannot move: Car in front!");
                    obstacleDetector.DoTackle();
                    SoundManager.Instance.PlaySound(SoundManager.SoundName.CarDash);
                    StartCoroutine(CrashJiggle());
                    return; 
                }

                // ParkingSlotManger emptySlot = FindEmptyParkingSlot();
                ParkingSlotManger emptySlot = parkingGameManager.FindEmptyParkingSlot();

                if (emptySlot != null)
                {
                    CarMover mover = hit.collider.GetComponent<CarMover>();
                    if (mover != null)
                    {
                        emptySlot.isReserved = true;
                        emptySlot.incomingCar = mover; // FIX: Link the car to the slot! 
                        mover.isParked = true;
                        
                        // This now starts the Coroutine we made earlier
                        mover.SetDestination(emptySlot.transform.position);
                        // SoundManager.Instance.PlaySound(SoundManager.SoundName.CarMove);
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("No empty slots available!");
                    // Here you might want to trigger a 'Parking Full' UI
                }
            }
        }
    }

    System.Collections.IEnumerator CrashJiggle()
    {
        Vector3 orig = Camera.main.transform.localPosition;
        for (int i = 0; i < 5; i++) // Jiggles 5 times quickly
        {
            Camera.main.transform.localPosition = orig + (Vector3)UnityEngine.Random.insideUnitCircle * 0.2f;
            yield return new WaitForSeconds(0.03f); // Delay between shakes
        }
        Camera.main.transform.localPosition = orig; // Stops cleanly at original spot
    }


    // ParkingSlotManger FindEmptyParkingSlot()
    // {
    //     foreach (GameObject slotObj in parkingSpot)
    //     {
    //         ParkingSlotManger slot = slotObj.GetComponent<ParkingSlotManger>();
    //         // if (slot != null && !slot.isOccupied && slot.isOccupied == false)
    //         // {
    //         //     return slotObj; // Return the first free slot we find
    //         // }
    //         if (slot != null && !slot.isOccupied && !slot.isReserved)
    //         {
    //             return slot; // Corrected: Return the component, not the GameObject
    //         }
    //     }
    //     return null; // All spots are full 
    // }
}
