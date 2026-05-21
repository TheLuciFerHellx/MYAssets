using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class Carout : MonoBehaviour
{
    public float rayDis = 5f;
    // public Transform Position;


    public float dashSpeed = 0.2f;
    public float returnSpeed = 0.5f;

    // public CarMover carMover;

    // public GameObject ColliderTrigger;
    public bool isBlocked { get; private set; }

    public Vector2Int currentGridIndex;

    [Header("Dynamic Tackle Settings")]
    [Tooltip("How far from the center of the blocking car we should stop to make visual contact.")]
    public float tackleCollisionOffset = 4.0f;
    private Vector2Int blockingGridIndex; // Caches the grid coordinate of the blocking obstacle

    // Update is called once per frame
    // void Update()
    // {
    //     Ray ray = new Ray(transform.position , transform.forward);
    //     RaycastHit hit;

    //     // UnityEngine.Debug.DrawRay(ray.origin, ray.direction * rayDis, Color.red);

    //     if(Physics.Raycast(ray, out hit, rayDis))
    //     {
    //         isBlocked = hit.collider.CompareTag("car");

    //     }       
    //     else
    //     {
    //         isBlocked = false;
    //     } 
    // }

    //  public bool CheckForBlockage()
    // {
    //     Ray ray = new Ray(transform.position, transform.forward);
    //     RaycastHit hit;

    //     if (Physics.Raycast(ray, out hit, rayDis))
    //     {
    //         isBlocked = hit.collider.CompareTag("car");
    //     }
    //     else
    //     {
    //         isBlocked = false;
    //     }

    //     return isBlocked;
    // }

    // void OnTriggerEnter(Collider other)
    // {
    //     if(other.gameObject.CompareTag("car"))
    //     {
    //         isBlocked = true;
    //     }
    //     else
    //     {
    //         isBlocked = false;
    //     }
    // }

    public bool CheckForBlockage()
    {
        if (SpawnCars.Instance == null) return false;

        // 1. Check for block using the method inside SpawnCars
        isBlocked = SpawnCars.Instance.CheckBlockageForCar(currentGridIndex, transform.eulerAngles, out Vector2Int targetGridIndex);

        // 2. If it is NOT blocked, make the slot isOccupied false simply
        if (!isBlocked)
        {
            SpawnCars.Instance.SetSlotOccupation(currentGridIndex.x, currentGridIndex.y, false);
            currentGridIndex = targetGridIndex; // Car tracks its new position
        }
        else
        {
            blockingGridIndex = targetGridIndex; // Store the blocking car's grid index
        }

        return isBlocked;
    }
    private void OnDrawGizmos() 
    {
        Gizmos.color = UnityEngine.Color.blue;
        // Draws a ray starting from the object's position forward
        Gizmos.DrawRay(transform.position, transform.forward * rayDis);
    }

    //public void DoTackle()
    //{
    //    Vector3 originalPosition = transform.position;
    //    Vector3 tacklePosition = transform.position + (transform.forward * 1f);

    //    // Create a sequence
    //    Sequence tackleSequence = DOTween.Sequence();

    //    // 1. Move forward fast
    //    tackleSequence.Append(transform.DOMove(tacklePosition, dashSpeed).SetEase(Ease.OutFlash));

    //    // 2. Small shake to add impact (optional)
    //    tackleSequence.Append(transform.DOShakeRotation(0.2f, 10f));

    //    // 3. Move back to original position
    //    tackleSequence.Append(transform.DOMove(originalPosition, returnSpeed).SetEase(Ease.InQuad));
    //}

    public void DoTackle()
    {
        Vector3 originalPosition = transform.position;
        Vector3 tacklePosition = originalPosition + (transform.forward * 1.5f); // Fallback nudge

        if (SpawnCars.Instance != null && isBlocked)
        {
            // Retrieve the slot data for the blocking car
            var blockingSlot = SpawnCars.Instance.GetSlotAt(blockingGridIndex.x, blockingGridIndex.y);
            if (blockingSlot != null)
            {
                Vector3 blockingWorldPos = blockingSlot.WorldPosition;
                Vector3 dir = (blockingWorldPos - originalPosition).normalized;

                // Distance in world units between the two cars
                float distance = Vector3.Distance(originalPosition, blockingWorldPos);

                // If they are far enough apart, slide right up to the obstacle with a collision offset
                if (distance > tackleCollisionOffset)
                {
                    tacklePosition = blockingWorldPos - dir * tackleCollisionOffset;
                }
                else
                {
                    // If they are already touching or extremely close, do a small proportional nudge
                    tacklePosition = originalPosition + (dir * (distance * 0.4f));
                }
            }
        }

        // Create the DOTween sequence
        Sequence tackleSequence = DOTween.Sequence();

        // 1. Move forward fast to the calculated tackle position
        tackleSequence.Append(transform.DOMove(tacklePosition, dashSpeed).SetEase(Ease.OutFlash));

        // 2. Small shake to add physical impact
        tackleSequence.Append(transform.DOShakeRotation(0.2f, 10f));

        // 3. Move back to original position
        tackleSequence.Append(transform.DOMove(originalPosition, returnSpeed).SetEase(Ease.InQuad));
    }
}
