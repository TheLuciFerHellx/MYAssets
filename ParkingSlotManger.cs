// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ParkingSlotManger : MonoBehaviour
// {
//     public CarMover carMover;
//     public Passenger passenger;

//     public PassengerManger passengerManger;
//     // Start is called before the first frame update
//     void Start()
//     {
//         carMover = FindObjectOfType<CarMover>();
//         passenger = FindObjectOfType<Passenger>();
//         passengerManger = FindObjectOfType<PassengerManger>();
//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     void OnTriggerEnter(Collider other)
//     {
//         // if(other.TryGetComponent(out carMover.carColor))
//         // {

//         //     Debug.Log("its Working");
//         //     if(carMover.carColor == passenger.passengerColor)
//         //     {
//         //         // var first = passengerManger.passengers.Peek();
//         //         // if()
//         //         // {

//         //         // }
//         //         passengerManger.passengers.Dequeue();
//         //     }
//         // }
//         // Safety check: make sure the managers were actually found
//         if (carMover == null || passengerManger == null) return;

//         // TryGetComponent needs a type. I'm assuming 'CarColor' is the class name.
//         if(other.TryGetComponent(out CarMover detectedColor))
//         {
//             Debug.Log("Object with color detected!");

//             // if (detectedCar.carColor == passenger.passengerColor)
//             // {
//             //     passengerManger.passengers.Dequeue();
//             // }

//             // Check if the detected color matches the passenger color
//             if(detectedColor.carColor == passenger.passengerColor)
//             {
//                 // Safety check: Make sure there is actually someone in the queue to remove
//                 if (passengerManger.passengers.Count > 0)
//                 {
//                     // passengerManger.passengers.Dequeue();
//                     Passenger p = passengerManger.passengers[0];
//                     passengerManger.passengers.RemoveAt(0);
//                     Debug.Log("Passenger Dequeued!");
//                 }
//             }
//         }
//     }
// }
using System.Collections;
using UnityEngine;

public class ParkingSlotManger : MonoBehaviour
{
    // NO NEED for FindObjectOfType anymore

    public bool isOccupied = false;
    public bool isReserved = false;
    public ColorOfCarAndPassengers parkedCarType; 
    public CarMover currentCar;
    public CarMover incomingCar; // FIX: Tells the slot which car is expected

    // public bool IsAvailable => !isOccupied && !isReserved;


    void OnTriggerEnter(Collider other)
    {
        if (!isOccupied && other.TryGetComponent(out CarMover detectedCar))
        {
            // FIX: Ignore passing cars! Only accept the car that this slot is reserved for.
            if (isReserved && incomingCar != null && detectedCar != incomingCar) 
            {
                return;
            }

            currentCar = detectedCar;
            currentCar.totalPassengerTxt.enabled = true;
            currentCar.totalPassengerTxt.text = currentCar.thisCarCapacity.ToString();
            currentCar.arrow.enabled = false;
            isOccupied = true;
            isReserved = false;
            incomingCar = null; // Clear the reservation
            parkedCarType = detectedCar.carType;

            Debug.Log("Car entered: " + gameObject.name);
            
            // Notify central manager
            // ParkingGameManager.Instance.OnCarParked(detectedCar.carType);
            StartCoroutine(DelayOnDetetct());
            Debug.Log("Car Color :"+detectedCar.carType);
        }
    }

     void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CarMover detectedCar))
        {
            // isOccupied = false;
            // currentCar = null;
            // parkedCarType = ColorOfCarAndPassengers.None; //|================================
            // Debug.Log("Car left: " + gameObject.name);
            if(currentCar == detectedCar)
            {
                StartCoroutine(DelayExitCheck(detectedCar));
            }
        }
    }
    IEnumerator DelayExitCheck(CarMover car)
    {
        yield return new WaitForSeconds(0.01f);

        if(currentCar == car)
        {
            isOccupied = false;
            currentCar = null;
            parkedCarType = ColorOfCarAndPassengers.None; //|================================
            Debug.Log("Car left truly: " + gameObject.name);
        }
    }
    // to make delay in detect so car park perfectly.
    IEnumerator DelayOnDetetct()
    {
        if(currentCar != null)
        {
            
            yield return new WaitForSeconds(0.5f);
        }

        // ParkingGameManager.Instance.OnCarParked(currentCar.carType);
        // Check if the instance and the car still exist after the delay
        if (ParkingGameManager.Instance != null && currentCar != null)
        {
            ParkingGameManager.Instance.OnCarParked(currentCar.carType);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.Log("delay!");
        }
    }
}
