
using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float moveDuration = 8f;
    [SerializeField] private float spawnPadding = 1f;
    [SerializeField] private BoxCollider2D levelBounds;
    [SerializeField] private Camera levelCamera;

    private GameObject currentCherry;

    private void Start()
    {
        StartCoroutine(BeginSpawn());
    }

    private IEnumerator BeginSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            currentCherry = SpawnCherry();

            if (currentCherry != null)
            {
                yield return MoveCherry(currentCherry);
                Destroy(currentCherry);
                currentCherry = null;
            }
        }
    }

    private GameObject SpawnCherry()
    {
        Bounds bounds = levelBounds.bounds;
        Vector2 center = bounds.center;

        float angle = Random.Range(0f, 360f);
        Vector2 direction = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;

        float levelDistance = DistanceToRectangleEdge(
            center, direction, bounds);

        Vector2 spawnPosition =
            center + direction * (levelDistance + spawnPadding);

        Bounds cameraBounds = GetCameraBounds();

        float cameraDistance = DistanceToRectangleEdge(
            center, -direction, cameraBounds);

        Vector2 targetPosition =
            center - direction * (cameraDistance + spawnPadding);

        GameObject cherry = Instantiate(
            cherryPrefab, spawnPosition, Quaternion.identity);

        CherryPath path = cherry.AddComponent<CherryPath>();
        path.Initialize(spawnPosition, targetPosition, moveDuration);

        return cherry;
    }

    private IEnumerator MoveCherry(GameObject cherry)
    {
        CherryPath path = cherry.GetComponent<CherryPath>();

        if (path != null)
        {
            yield return path.Move();
        }
    }

    private Bounds GetCameraBounds()
    {
        float distance = Mathf.Abs(levelCamera.transform.position.z);

        Vector3 bottomLeft = levelCamera.ViewportToWorldPoint(
            new Vector3(0f, 0f, distance));

        Vector3 topRight = levelCamera.ViewportToWorldPoint(
            new Vector3(1f, 1f, distance));

        Vector3 size = topRight - bottomLeft;

        return new Bounds((bottomLeft + topRight) / 2f, size);
    }

    private float DistanceToRectangleEdge(
        Vector2 point, Vector2 direction, Bounds bounds)
    {
        float distanceX = float.PositiveInfinity;
        float distanceY = float.PositiveInfinity;

        if (direction.x > 0.0001f)
            distanceX = (bounds.max.x - point.x) / direction.x;
        else if (direction.x < -0.0001f)
            distanceX = (bounds.min.x - point.x) / direction.x;

        if (direction.y > 0.0001f)
            distanceY = (bounds.max.y - point.y) / direction.y;
        else if (direction.y < -0.0001f)
            distanceY = (bounds.min.y - point.y) / direction.y;

        return Mathf.Min(Mathf.Abs(distanceX), Mathf.Abs(distanceY));
    }
}

public class CherryPath : MonoBehaviour
{
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float duration;

    public void Initialize(
        Vector2 start, Vector2 target, float moveDuration)
    {
        startPosition = start;
        targetPosition = target;
        duration = Mathf.Max(0.01f, moveDuration);
    }

    public IEnumerator Move()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);

            transform.position = Vector2.Lerp(
                startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
    }
}
