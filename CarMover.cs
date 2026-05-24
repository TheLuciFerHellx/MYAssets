//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.Video;
//using DG.Tweening;
//using TMPro;
//using UnityEngine.UI;
//using System.Collections;

//public class CarMover : MonoBehaviour 
//{
//    public ColorOfCarAndPassengers carType;

//    public ColorOfCarAndPassengers defaultCarType;
//    private Vector3 targetPosition;
//    public float speed = 20f;
//    private bool isMoving = false;

//    public int CapacityOfPassengers = 4;
//    public int thisCarCapacity ;
//    public TextMeshProUGUI totalPassengerTxt;
//    public Image arrow;
//    public bool isParked = false;

//    // int moveStep = 0;
//    // float distanceMoved = 0f;


//    public void SetDestination(Vector3 target) 
//    {
//        targetPosition = target;
//        isMoving = true;
//        StartCoroutine(MoveRoutine());
//    }

//    public void ResetCapacity() 
//    {
//        CapacityOfPassengers = thisCarCapacity;
//        isParked = false;
//        // Reset any other visuals or logic here, like clearing passenger lists
//    }

//    public void ResetEnum() 
//    {
//        carType = defaultCarType;
//        Debug.Log("Car enum reset to: " + carType);
//    }

//    //  private IEnumerator MoveRoutine() 
//    // {
//    //     // While we haven't reached the target
//    //     while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
//    //     {
//    //         // 1. Handle Rotation
//    //         Vector3 direction = (targetPosition - transform.position).normalized;
//    //         direction.y = 0;

//    //         if (direction != Vector3.zero) 
//    //         {
//    //             Quaternion targetRot = Quaternion.LookRotation(direction);
//    //             transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
//    //         }

//    //         // 2. Handle Position
//    //         transform.position = Vector3.MoveTowards(transform.position, targetPosition, (speed * 6) * Time.deltaTime);

//    //         // Wait for next frame
//    //         yield return null;
//    //     }

//    //     // 3. Final snap and reset
//    //     transform.position = targetPosition;
//    //     transform.rotation = Quaternion.Euler(0, 0, 0);
//    //     // moveCoroutine = null;
//    // }

//    private IEnumerator MoveRoutine()
//    {
//        // PART 1: Drive around the outer walls
//        while (true)
//        {
//            // Always move forward
//            transform.Translate(Vector3.forward * (speed * 6) * Time.deltaTime);

//            Ray ray = new Ray(transform.position, transform.forward);
//            RaycastHit hit;

//            // Only detect walls that are close (1.5 units away)
//            if (Physics.Raycast(ray, out hit, 5f))
//            {
//                if (hit.collider.CompareTag("rightwall") || hit.collider.CompareTag("leftwall"))
//                {
//                    // Set rotation to face the Front (North)
//                    transform.rotation = Quaternion.Euler(0, 0, 0);
//                }
//                else if (hit.collider.CompareTag("backwall"))
//                {
//                    // If we hit the back, turn Right to drive towards the rightWall
//                    transform.rotation = Quaternion.Euler(0, 90f, 0);
//                }
//                else if (hit.collider.CompareTag("frontwall"))
//                {
//                    // We hit the front! Break out of this while loop to go to the slot
//                    break;
//                }
//            }

//            // Wait for the next frame before looping again
//            yield return null;
//        }

//        // PART 2: We hit the front wall, now drive to the parking slot!

//        // Smoothly turn to face the target slot
//        Vector3 direction = (targetPosition - transform.position).normalized;
//        direction.y = 0;
//        if (direction != Vector3.zero)
//        {
//            transform.rotation = Quaternion.LookRotation(direction);
//        }

//        // Keep moving until we reach the parking slot
//        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
//        {
//            transform.position = Vector3.MoveTowards(transform.position, targetPosition, (speed * 6) * Time.deltaTime);
//            yield return null;
//        }

//        // Safely snap into the final position
//        transform.position = targetPosition;
//        transform.rotation = Quaternion.Euler(0, 0, 0); // Face straight in the slot
//        isMoving = false;
//    }


//    // void Update() 
//    // {
//    //     if (!isMoving) return;

//    //     // Move and Rotate
//    //     Vector3 direction = (targetPosition - transform.position).normalized;
//    //     direction.y = 0;
//    //     if (direction != Vector3.zero) 
//    //     {
//    //         transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 5f * Time.deltaTime);
//    //     }

//    //     transform.position = Vector3.MoveTowards(transform.position,targetPosition, (speed * 6) * Time.deltaTime);
//    //     // transform.position = Vector3.Slerp(transform.position, targetPosition, 2 * Time.deltaTime); 

//    //     if (transform.position == targetPosition)
//    //     {
//    //         transform.rotation = Quaternion.Euler(0, 0 ,0);
//    //         isMoving = false;
//    //     }
//    //     //=======================for move in steps=============================================
//    //     // if (moveStep == 0) 
//    //     // {
//    //     //     // 1. Move Forward (5 units total)
//    //     //     float moveAmount = speed * Time.deltaTime;
//    //     //     transform.Translate(Vector3.forward * moveAmount);
//    //     //     distanceMoved += moveAmount;

//    //     //     if (distanceMoved >= 5f) moveStep = 1;
//    //     // }
//    //     // else if (moveStep == 1) 
//    //     // {
//    //     //     // 2. Rotate to face target
//    //     //     Vector3 dir = (targetPosition - transform.position).normalized;
//    //     //     dir.y = 0;
//    //     //     Quaternion targetRot = Quaternion.LookRotation(dir);
//    //     //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);

//    //     //     if (Quaternion.Angle(transform.rotation, targetRot) < 1f) moveStep = 2;
//    //     // }
//    //     // else if (moveStep == 2) 
//    //     // {
//    //     //     // 3. Move Towards final target
//    //     //     transform.position = Vector3.MoveTowards(transform.position, targetPosition, (speed * 4) * Time.deltaTime);

//    //     //     if (Vector3.Distance(transform.position, targetPosition) < 0.05f) 
//    //     //     {
//    //     //         isMoving = false;
//    //     //         moveStep = 0; // Reset
//    //     //         distanceMoved = 0f;
//    //     //         transform.rotation = Quaternion.Euler(0, 0 ,0);
//    //     //     }
//    //     // }

//    // }


//    public void DriveAway()
//    {
//        isMoving = false;   
//        totalPassengerTxt.enabled = false;
//        arrow.enabled = false;
//        DG.Tweening.Sequence s = DOTween.Sequence();

//        // 1. Move Back (Relative to current position)
//        s.Append(transform.DOMove(-transform.forward * 7f, 0.001f).SetRelative());   

//        // 2. Wait
//        s.AppendInterval(0.1f);

//        // 3. Turn Right (90 degrees)
//        s.Append(transform.DORotate(new Vector3(0, 90, 0), 0.2f));


//        // 4. Move Straight (Relative to the NEW facing direction)
//        // By using SetRelative(true), it calculates the direction AFTER the turn finishes
//        s.Append(transform.DOMove(transform.right * 80f, 0.5f).SetRelative());
//        // SoundManager.Instance.PlaySound(SoundManager.SoundName.CarMove);

//        s.OnComplete(() => 
//        {
//            ObjectPool.Instance.AddToPool(gameObject); // pooling car as well
//            isParked = false;
//        });

//    }

//}

//NOTE make sure you make gridldot list public on spawnCars
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CarMover : MonoBehaviour
{
    public ColorOfCarAndPassengers carType;
    public ColorOfCarAndPassengers defaultCarType;

    private Vector3 targetPosition;

    public float speed = 35f;

    private bool isMoving = false;

    public int CapacityOfPassengers = 4;
    public int thisCarCapacity;

    public TextMeshProUGUI totalPassengerTxt;
    public Image arrow;

    public bool isParked = false;

    [Header("Traffic")]
    public float safetyDistance = 4f;

    // =========================================================
    // SET DESTINATION
    // =========================================================

    public void SetDestination(Vector3 target)
    {
        if (isMoving) return;

        targetPosition = target;

        isMoving = true;

        StartCoroutine(MoveRoutine());
    }

    // =========================================================
    // RESETS
    // =========================================================

    public void ResetCapacity()
    {
        CapacityOfPassengers = thisCarCapacity;

        isParked = false;
    }

    public void ResetEnum()
    {
        carType = defaultCarType;

        Debug.Log("Car enum reset to: " + carType);
    }

    // =========================================================
    // MAIN MOVE
    // =========================================================

    private IEnumerator MoveRoutine()
    {
        Carout carout = GetComponent<Carout>();

        if (carout == null)
        {
            yield break;
        }

        Vector2Int currentGrid =
            carout.currentGridIndex;

        // =====================================================
        // SNAP TO CURRENT GRID CENTER
        // =====================================================

        var currentSlot =
            SpawnCars.Instance.GetSlotAt(
                currentGrid.x,
                currentGrid.y);

        if (currentSlot != null)
        {
            transform.position =
                currentSlot.WorldPosition;
        }

        // =====================================================
        // GET DIRECTION
        // =====================================================

        Vector2Int dir =
            GetDirectionFromAngle(
                transform.eulerAngles.y);

        // =====================================================
        // GENERATE PATH
        // =====================================================

        List<Vector3> waypoints =
            GeneratePath(currentGrid, dir);

        // Final parking slot
        waypoints.Add(targetPosition);

        // =====================================================
        // INITIAL ROTATION
        // =====================================================

        if (waypoints.Count > 0)
        {
            Vector3 firstDir =
                (waypoints[0] - transform.position).normalized;

            firstDir.y = 0;

            if (firstDir != Vector3.zero)
            {
                transform.rotation =
                    Quaternion.LookRotation(firstDir);
            }
        }

        // =====================================================
        // FOLLOW WAYPOINTS
        // =====================================================

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 wp = waypoints[i];

            // =================================================
            // MOVE FIRST
            // =================================================

            while (Vector3.Distance(transform.position, wp) > 0.05f)
            {
                // Traffic stop
                if (IsMovingCarAhead())
                {
                    yield return null;
                    continue;
                }

                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        wp,
                        speed * Time.deltaTime);

                yield return null;
            }

            // PERFECT SNAP
            transform.position = wp;

            // =================================================
            // THEN ROTATE
            // =================================================

            if (i < waypoints.Count - 1)
            {
                Vector3 nextDir =
                    (waypoints[i + 1] - transform.position).normalized;

                nextDir.y = 0;

                if (nextDir != Vector3.zero)
                {
                    transform.rotation =
                        Quaternion.LookRotation(nextDir);
                }
            }
        }

        // =====================================================
        // FINAL PARK SNAP
        // =====================================================

        transform.position = targetPosition;

        transform.rotation =
            Quaternion.Euler(0, 0, 0);

        isMoving = false;

        isParked = true;
    }

    // =========================================================
    // GENERATE GRID PATH
    // =========================================================

    private List<Vector3> GeneratePath(
        Vector2Int startGrid,
        Vector2Int dir)
    {
        List<Vector3> path =
            new List<Vector3>();

        int maxCols =
            SpawnCars.Instance.maxColumns;

        int maxRows =
            SpawnCars.Instance.maxRows;

        Vector2Int current =
            startGrid;

        // =====================================================
        // STEP 1
        // MOVE TILL BORDER
        // =====================================================

        while (true)
        {
            Vector2Int next =
                current + dir;

            bool outside =
                next.x < 0 ||
                next.x >= maxCols ||
                next.y < 0 ||
                next.y >= maxRows;

            if (outside)
            {
                break;
            }

            current = next;

            var slot =
                SpawnCars.Instance.GetSlotAt(
                    current.x,
                    current.y);

            if (slot != null)
            {
                path.Add(slot.WorldPosition);
            }
        }

        // =====================================================
        // STEP 2
        // BORDER ROAD SYSTEM
        // =====================================================

        // LEFT BORDER
        if (current.x == 0)
        {
            for (int y = current.y + 1; y < maxRows; y++)
            {
                var slot =
                    SpawnCars.Instance.GetSlotAt(
                        0,
                        y);

                if (slot != null)
                {
                    path.Add(slot.WorldPosition);
                }
            }
        }

        // RIGHT BORDER
        else if (current.x == maxCols - 1)
        {
            for (int y = current.y + 1; y < maxRows; y++)
            {
                var slot =
                    SpawnCars.Instance.GetSlotAt(
                        maxCols - 1,
                        y);

                if (slot != null)
                {
                    path.Add(slot.WorldPosition);
                }
            }
        }

        // BOTTOM BORDER
        else if (current.y == 0)
        {
            // Move RIGHT on bottom road
            for (int x = current.x + 1; x < maxCols; x++)
            {
                var slot =
                    SpawnCars.Instance.GetSlotAt(
                        x,
                        0);

                if (slot != null)
                {
                    path.Add(slot.WorldPosition);
                }
            }

            // Then UP on right road
            for (int y = 1; y < maxRows; y++)
            {
                var slot =
                    SpawnCars.Instance.GetSlotAt(
                        maxCols - 1,
                        y);

                if (slot != null)
                {
                    path.Add(slot.WorldPosition);
                }
            }
        }

        return path;
    }

    // =========================================================
    // DIRECTION FROM ANGLE
    // =========================================================

    private Vector2Int GetDirectionFromAngle(float angle)
    {
        angle =
            Mathf.Repeat(
                Mathf.Round(angle / 45f) * 45f,
                360f);

        switch ((int)angle)
        {
            case 0:
                return new Vector2Int(0, 1);

            case 45:
                return new Vector2Int(1, 1);

            case 90:
                return new Vector2Int(1, 0);

            case 135:
                return new Vector2Int(1, -1);

            case 180:
                return new Vector2Int(0, -1);

            case 225:
                return new Vector2Int(-1, -1);

            case 270:
                return new Vector2Int(-1, 0);

            case 315:
                return new Vector2Int(-1, 1);
        }

        return Vector2Int.zero;
    }

    // =========================================================
    // TRAFFIC CHECK
    // =========================================================

    private bool IsMovingCarAhead()
    {
        RaycastHit hit;

        if (Physics.Raycast(
            transform.position + Vector3.up * 0.5f,
            transform.forward,
            out hit,
            safetyDistance))
        {
            if (hit.collider.CompareTag("car"))
            {
                if (hit.collider.gameObject == gameObject)
                    return false;

                CarMover other =
                    hit.collider.GetComponent<CarMover>();

                // Ignore parked cars
                if (other != null && !other.isParked)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // =========================================================
    // DRIVE AWAY
    // =========================================================

    public void DriveAway()
    {
        isMoving = false;

        totalPassengerTxt.enabled = false;

        arrow.enabled = false;

        //Sequence s = DOTween.Sequence();

        DG.Tweening.Sequence s = DOTween.Sequence();
        // Move back
        s.Append(
            transform.DOMove(
                -transform.forward * 7f,
                0.001f).SetRelative());

        // Wait
        s.AppendInterval(0.1f);

        // Turn right
        s.Append(
            transform.DORotate(
                new Vector3(0, 90, 0),
                0.2f));

        // Drive away
        s.Append(
            transform.DOMove(
                transform.right * 80f,
                0.5f).SetRelative());

        s.OnComplete(() =>
        {
            ObjectPool.Instance.AddToPool(gameObject);

            isParked = false;
        });
    }
}
