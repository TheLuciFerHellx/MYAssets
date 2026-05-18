using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CarMover : MonoBehaviour 
{
    public ColorOfCarAndPassengers carType;

    public ColorOfCarAndPassengers defaultCarType;
    private Vector3 targetPosition;
    public float speed = 20f;
    private bool isMoving = false;

    public int CapacityOfPassengers = 4;
    public int thisCarCapacity ;
    public TextMeshProUGUI totalPassengerTxt;
    public Image arrow;
    public bool isParked = false;

    // int moveStep = 0;
    // float distanceMoved = 0f;


    public void SetDestination(Vector3 target) 
    {
        targetPosition = target;
        isMoving = true;
        StartCoroutine(MoveRoutine());
    }

    public void ResetCapacity() 
    {
        CapacityOfPassengers = thisCarCapacity;
        isParked = false; // FIX: Reset this so pooled cars can be clicked again!
        // Reset any other visuals or logic here, like clearing passenger lists
    }

    public void ResetEnum() 
    {
        carType = defaultCarType;
        Debug.Log("Car enum reset to: " + carType);
    }

    // private IEnumerator MoveRoutine() 
    //{
    //    // While we haven't reached the target
    //    while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
    //    {
    //        // 1. Handle Rotation
    //        Vector3 direction = (targetPosition - transform.position).normalized;
    //        direction.y = 0;

    //        if (direction != Vector3.zero) 
    //        {
    //            Quaternion targetRot = Quaternion.LookRotation(direction);
    //            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
    //        }

    //        // 2. Handle Position
    //        transform.position = Vector3.MoveTowards(transform.position, targetPosition, (speed * 6) * Time.deltaTime);

    //        // Wait for next frame
    //        yield return null;
    //    }

    //    // 3. Final snap and reset
    //    transform.position = targetPosition;
    //    transform.rotation = Quaternion.Euler(0, 0, 0);
    //    // moveCoroutine = null;
    //}

    private IEnumerator MoveRoutine()
    {
        // PART 1: Drive around the outer walls
        while (true)
        {
            // Always move forward
            transform.Translate(Vector3.forward * (speed * 6) * Time.deltaTime);

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            // Only detect walls that are close (1.5 units away)
            if (Physics.Raycast(ray, out hit, 5f))
            {
                if (hit.collider.CompareTag("rightWall") || hit.collider.CompareTag("leftWall"))
                {
                    // Set rotation to face the Front (North)
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (hit.collider.CompareTag("backWall"))
                {
                    // If we hit the back, turn Right to drive towards the rightWall
                    transform.rotation = Quaternion.Euler(0, 90f, 0);
                }
                else if (hit.collider.CompareTag("frontWall"))
                {
                    // We hit the front! Break out of this while loop to go to the slot
                    break;
                }
            }

            // Wait for the next frame before looping again
            yield return null;
        }

        // PART 2: We hit the front wall, now drive to the parking slot!

        // Smoothly turn to face the target slot
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Keep moving until we reach the parking slot
        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, (speed * 6) * Time.deltaTime);
            yield return null;
        }

        // Safely snap into the final position
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(0, 0, 0); // Face straight in the slot
        isMoving = false;
    }


    // void Update() 
    // {
    //     if (!isMoving) return;

    //     // Move and Rotate
    //     Vector3 direction = (targetPosition - transform.position).normalized;
    //     direction.y = 0;
    //     if (direction != Vector3.zero) 
    //     {
    //         transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 5f * Time.deltaTime);
    //     }

    //     transform.position = Vector3.MoveTowards(transform.position,targetPosition, (speed * 6) * Time.deltaTime);
    //     // transform.position = Vector3.Slerp(transform.position, targetPosition, 2 * Time.deltaTime); 

    //     if (transform.position == targetPosition)
    //     {
    //         transform.rotation = Quaternion.Euler(0, 0 ,0);
    //         isMoving = false;
    //     }
    //     //=======================for move in steps=============================================
    //     // if (moveStep == 0) 
    //     // {
    //     //     // 1. Move Forward (5 units total)
    //     //     float moveAmount = speed * Time.deltaTime;
    //     //     transform.Translate(Vector3.forward * moveAmount);
    //     //     distanceMoved += moveAmount;

    //     //     if (distanceMoved >= 5f) moveStep = 1;
    //     // }
    //     // else if (moveStep == 1) 
    //     // {
    //     //     // 2. Rotate to face target
    //     //     Vector3 dir = (targetPosition - transform.position).normalized;
    //     //     dir.y = 0;
    //     //     Quaternion targetRot = Quaternion.LookRotation(dir);
    //     //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);

    //     //     if (Quaternion.Angle(transform.rotation, targetRot) < 1f) moveStep = 2;
    //     // }
    //     // else if (moveStep == 2) 
    //     // {
    //     //     // 3. Move Towards final target
    //     //     transform.position = Vector3.MoveTowards(transform.position, targetPosition, (speed * 4) * Time.deltaTime);

    //     //     if (Vector3.Distance(transform.position, targetPosition) < 0.05f) 
    //     //     {
    //     //         isMoving = false;
    //     //         moveStep = 0; // Reset
    //     //         distanceMoved = 0f;
    //     //         transform.rotation = Quaternion.Euler(0, 0 ,0);
    //     //     }
    //     // }

    // }


    public void DriveAway()
    {
        isMoving = false;   
        totalPassengerTxt.enabled = false;
        arrow.enabled = false;
        DG.Tweening.Sequence s = DOTween.Sequence();

        // 1. Move Back (Relative to current position)
        s.Append(transform.DOMove(-transform.forward * 7f, 0.01f).SetRelative());   

        // 2. Wait
        s.AppendInterval(0.1f);

        // 3. Turn Right (90 degrees)
        s.Append(transform.DORotate(new Vector3(0, 90, 0), 0.2f));
        

        // 4. Move Straight (Relative to the NEW facing direction)
        // By using SetRelative(true), it calculates the direction AFTER the turn finishes
        s.Append(transform.DOMove(transform.right * 80f, 0.5f).SetRelative());
        // SoundManager.Instance.PlaySound(SoundManager.SoundName.CarMove);
        
        s.OnComplete(() => 
        {
            ObjectPool.Instance.AddToPool(gameObject); // pooling car as well
            isParked = false;
        });
  
    }
    
}
