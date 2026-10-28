using UnityEngine;
using System.Collections;

public class SproutMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float movespeed = 2f;
    [SerializeField] private Animator animator;

    private int currentWaypointIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(MoveSprout());
    }

    private IEnumerator MoveSprout()
    {
        while (true) {
            Vector3 startPos = transform.position;
            Vector3 targetPos = waypoints[currentWaypointIndex].position;
            animator.SetInteger("Currentwaypoint", currentWaypointIndex);
            float distance = Vector3.Distance(startPos, targetPos);
            float timeToMove = distance / movespeed;
            
            float elapsedTime = 0f;
            while (elapsedTime < timeToMove) {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / timeToMove);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}
