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

    [Header("ShuffleButton")]
    public int ShuffleCount =3 ;
    public TextMeshProUGUI shuffleCountTxt;
    public Button shuffleButton;

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


    // void Update()
    // {
    //     shuffleCountTxt.text = ShuffleCount.ToString();
    //     if(ShuffleCount <= 0)
    //     {   
    //         shuffleButton.enabled = false;
    //     }   
    // }

    #region ShuffleCars
    // public void ShuffleCars()
    // {
    //     List<Vector3> positions = new List<Vector3>();

    //     foreach (CarMover car in spawnPassengers.TotalCarsSpawn)
    //     {
    //         if (car != null)
    //         {
    //             positions.Add(car.transform.position);
    //         }
    //     }

    //     foreach (CarMover car in spawnPassengers.TotalCarsSpawn)
    //     {
    //         if (car == null) continue;

    //         int randomIndex = Random.Range(0, positions.Count);

    //         car.transform.position = positions[randomIndex];

    //         positions.RemoveAt(randomIndex);
    //     }

    //     Debug.Log("Cars Shuffled");
    // }
    public void ShuffleCars() 
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundName.Click);

        List<CarMover> activeCars = new List<CarMover>();
        foreach (CarMover car in spawnPassengers.TotalCarsSpawn)
        {
            if (car != null && car.gameObject.activeInHierarchy && !car.isParked)
            {
                activeCars.Add(car);
            }
        }

        if (activeCars.Count <= 1) return;

        List<Vector3> positions = new List<Vector3>();
        foreach (CarMover car in activeCars) {
            positions.Add(car.transform.position);
        }

        foreach (CarMover car in activeCars) {
            int randomIndex = Random.Range(0, positions.Count);
            car.transform.position = positions[randomIndex];
            
            positions[randomIndex] = positions[positions.Count - 1];
            positions.RemoveAt(positions.Count - 1);
        }
        ShuffleCount--;

        Debug.Log($"Shuffled {activeCars.Count} active cars. Pooled and parked cars ignored.");

        shuffleCountTxt.text = ShuffleCount.ToString();
        if(ShuffleCount <= 0)
        {   
            shuffleButton.enabled = false;
        }   

    }

    #endregion

    #region UndoOneMove
        // Call this BEFORE moving any car
    // public void SaveMove(CarMover car)
    // {
    //     lastMovedCar = car;
    //     lastPosition = car.transform.position;
    // }


    // // Undo Button
    // public void UndoMove()
    // {
    //     if (lastMovedCar == null) return;

    //     lastMovedCar.transform.position = lastPosition;

    //     Debug.Log("Move Undone");
    // }

    #endregion

}
