using UnityEngine;

public class BudMarker : MonoBehaviour
{
    public Vector2 startPosition;
    public Vector2 targetPosition;
    public float duration;

    public void Initialize(
        Vector2 start, Vector2 target, float moveDuration)
    {
        startPosition = start;
        targetPosition = target;
        duration = Mathf.Max(0.01f, moveDuration);
    }
}