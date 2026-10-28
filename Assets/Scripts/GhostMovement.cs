using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    public enum GhostState
    {
        Normal,
        Scared,
        Dead
    }

    [SerializeField] private Tilemap tileMap;
    [SerializeField] private Transform pacStudent;
    [SerializeField] private Animator animator;

    [SerializeField, Range(1, 4)] private int ghostNumber = 1;

    [SerializeField] private float playerSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float normalSpeedMultiplier = 0.9f;
    [SerializeField, Range(0f, 1f)] private float scaredSpeedMultiplier = 0.5f;

    [SerializeField] private Transform spawnExit;
    [SerializeField] private Collider2D spawnArea;

    public GhostState State { get; private set; }

    private Vector3 spawnPosition;
    private Vector3Int currentGridPosition;
    private Vector3Int previousGridPosition;
    private Vector3Int currentDirection;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float lerpTime;
    private float lerpDuration;
    private bool isLerping;
    private bool isOutsideSpawnArea;

    private Vector3Int spawnGridPosition;
    private Vector3Int exitGridPosition;

    private bool okToUpdate = false;

    private static readonly Vector3Int[] Directions =
    {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left
    };

    private void StartAfterIntro()
    {
        spawnPosition = transform.position;
        currentGridPosition = tileMap.WorldToCell(transform.position);
        previousGridPosition = currentGridPosition;
        transform.position = tileMap.GetCellCenterWorld(currentGridPosition);

        State = GhostState.Normal;

        spawnGridPosition = tileMap.WorldToCell(spawnPosition);

        if (spawnExit != null)
            exitGridPosition = tileMap.WorldToCell(spawnExit.position);

        isOutsideSpawnArea = spawnArea == null ||
            !spawnArea.OverlapPoint(transform.position);


        if (State != GhostState.Dead)
            ChooseNextMove();
    }

    private float CurrentSpeed
    {
        get
        {
            float normalSpeed = playerSpeed * normalSpeedMultiplier;

            if (State == GhostState.Dead || State == GhostState.Scared)
            {
                return normalSpeed * scaredSpeedMultiplier;
            }

            return normalSpeed;
        }
    }

    private bool IsInsideSpawnArea(Vector3 worldPosition)
    {
        return spawnArea != null &&
                spawnArea.OverlapPoint(worldPosition);
    }

    private bool IsWalkable(Vector3Int cell)
    {
        if (!tileMap.HasTile(cell))
            return false;

        string tileType = tileMap.GetTile(cell).name;

        if (tileType == "Spritesheet_0" || tileType == "Spritesheet_1" || tileType == "Spritesheet_21" || tileType == "Spritesheet_19" || tileType == "Spritesheet_20")
            return false;


        if (isOutsideSpawnArea &&
            IsInsideSpawnArea(tileMap.GetCellCenterWorld(cell)))
            return false;

        return true;
    }

    private List<Vector3Int> GetValidDirections()
    {
        List<Vector3Int> valid = new List<Vector3Int>();

        foreach (Vector3Int direction in Directions)
        {
            Vector3Int nextCell = currentGridPosition + direction;

            if (IsWalkable(nextCell))
                valid.Add(direction);
        }

        // Do not reverse unless there is no other choice.
        if (valid.Count > 1)
            valid.Remove(-currentDirection);

        return valid;
    }

    private void ChooseNextMove()
    {
        if (State == GhostState.Dead)
        {
            MoveDeadGhost();
            return;
        }

        if (!isOutsideSpawnArea && spawnExit != null)
        {
            MoveTowardsExit();
            return;
        }

        List<Vector3Int> valid = GetValidDirections();

        if (valid.Count == 0)
        {
            foreach (Vector3Int direction in Directions)
            {
                if (IsWalkable(currentGridPosition + direction))
                    valid.Add(direction);
            }
        }

        //This should not occur.
        if (valid.Count == 0)
        {
            Debug.LogWarning("No valid moves for ghost " + ghostNumber, this);
            return;
        }

        Vector3Int chosenDirection;

        if (State == GhostState.Scared)
        {
            chosenDirection = ChooseGhostOneDirection(valid);
        }
        else
        {
            switch (ghostNumber)
            {
                case 1:
                    chosenDirection = ChooseGhostOneDirection(valid);
                    break;

                case 2:
                    chosenDirection = ChooseGhostTwoDirection(valid);
                    break;

                case 3:
                    chosenDirection = valid[Random.Range(0, valid.Count)];
                    break;

                case 4:
                    chosenDirection = ChooseClockwiseDirection(valid);
                    break;

                default:
                    chosenDirection = valid[Random.Range(0, valid.Count)];
                    break;
            }
        }

        StartMove(chosenDirection);
    }

    private Vector3Int ChooseGhostOneDirection(
        List<Vector3Int> valid)
    {
        float currentDistance = Vector2.Distance(
            transform.position, pacStudent.position);

        List<Vector3Int> acceptable = new List<Vector3Int>();

        foreach (Vector3Int direction in valid)
        {
            Vector3 candidate = tileMap.GetCellCenterWorld(
                currentGridPosition + direction);

            float distance = Vector2.Distance(candidate, pacStudent.position);

            if (distance >= currentDistance)
                acceptable.Add(direction);
        }

        // If no direction satisfies the rule, use the best available
        // direction rather than stopping.
        if (acceptable.Count > 0)
            return acceptable[Random.Range(0, acceptable.Count)];

        float greatestDistance = float.MinValue;
        foreach (Vector3Int direction in valid)
        {
            Vector3 candidate = tileMap.GetCellCenterWorld(
                currentGridPosition + direction);

            float distance = Vector2.Distance(candidate, pacStudent.position);

            if (distance > greatestDistance)
                greatestDistance = distance;
        }

        List<Vector3Int> best = new List<Vector3Int>();
        foreach (Vector3Int direction in valid)
        {
            Vector3 candidate = tileMap.GetCellCenterWorld(
                currentGridPosition + direction);

            float distance = Vector2.Distance(candidate, pacStudent.position);

            if (Mathf.Approximately(distance, greatestDistance))
                best.Add(direction);
        }

        return best[Random.Range(0, best.Count)];
    }

    private Vector3Int ChooseGhostTwoDirection(
        List<Vector3Int> valid)
    {
        float currentDistance = Vector2.Distance(
            transform.position, pacStudent.position);

        List<Vector3Int> acceptable = new List<Vector3Int>();

        foreach (Vector3Int direction in valid)
        {
            Vector3 candidate = tileMap.GetCellCenterWorld(
                currentGridPosition + direction);

            float distance = Vector2.Distance(candidate, pacStudent.position);

            if (distance <= currentDistance)
                acceptable.Add(direction);
        }

        if (acceptable.Count > 0)
            return acceptable[Random.Range(0, acceptable.Count)];

        // No move meets the distance condition; choose the closest.
        float smallestDistance = float.MaxValue;
        foreach (Vector3Int direction in valid)
        {
            Vector3 candidate = tileMap.GetCellCenterWorld(
                currentGridPosition + direction);

            float distance = Vector2.Distance(candidate, pacStudent.position);

            if (distance < smallestDistance)
                smallestDistance = distance;
        }

        List<Vector3Int> best = new List<Vector3Int>();
        foreach (Vector3Int direction in valid)
        {
            Vector3 candidate = tileMap.GetCellCenterWorld(
                currentGridPosition + direction);

            float distance = Vector2.Distance(candidate, pacStudent.position);

            if (Mathf.Approximately(distance, smallestDistance))
                best.Add(direction);
        }

        return best[Random.Range(0, best.Count)];
    }

    private Vector3Int ChooseClockwiseDirection(
    List<Vector3Int> valid)
    {
        // Follow the left wall to trace the inner perimeter.
        Vector3Int[] priority;

        if (currentDirection == Vector3Int.zero)
        {
            priority = new[]
            {
                Vector3Int.up,
                Vector3Int.right,
                Vector3Int.down,
                Vector3Int.left
            };
        }
        else
        {
            int index = System.Array.IndexOf(Directions, currentDirection);

            if (index < 0)
                index = 0;

            // Left turn, straight, right turn, reverse.
            priority = new[]
            {
                Directions[(index + 3) % 4],
                Directions[index],
                Directions[(index + 1) % 4],
                Directions[(index + 2) % 4]
            };
        }

        foreach (Vector3Int direction in priority)
        {
            if (valid.Contains(direction))
                return direction;
        }

        return valid[Random.Range(0, valid.Count)];
    }

    private void MoveTowardsExit()
    {
        if (spawnExit == null)
        {
            ChooseValidFallbackMove();
            return;
        }

        List<Vector3Int> valid = GetValidDirections();

        if (valid.Count == 0)
        {
            ChooseValidFallbackMove();
            return;
        }

        Vector3Int bestDirection = valid[0];
        float bestDistance = float.MaxValue;

        foreach (Vector3Int direction in valid)
        {
            Vector3 candidate = tileMap.GetCellCenterWorld(
                currentGridPosition + direction);

            float distance = Vector3.Distance(
                candidate, tileMap.GetCellCenterWorld(exitGridPosition));

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = direction;
            }
        }

        StartMove(bestDirection);
    }

    private void ChooseValidFallbackMove()
    {
        List<Vector3Int> valid = GetValidDirections();

        if (valid.Count > 0)
            StartMove(valid[Random.Range(0, valid.Count)]);
    }

    private void StartMove(Vector3Int direction)
    {
        if (direction == Vector3Int.zero)
            return;

        currentDirection = direction;
        previousGridPosition = currentGridPosition;

        startPosition = transform.position;
        targetPosition = tileMap.GetCellCenterWorld(
            currentGridPosition + direction);

        float distance = Vector3.Distance(startPosition, targetPosition);

        lerpDuration = distance / Mathf.Max(CurrentSpeed, 0.01f);
        lerpTime = 0f;
        isLerping = true;

        if (animator != null)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }
    }

    public void Kill()
    {
        State = GhostState.Dead;
        isLerping = false;
        currentDirection = Vector3Int.zero;
    }

    private void MoveDeadGhost()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            spawnPosition,
            CurrentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, spawnPosition)
            <= 0.001f)
        {
            currentGridPosition = spawnGridPosition;
            transform.position = spawnPosition;
            previousGridPosition = currentGridPosition;
            currentDirection = Vector3Int.zero;

            State = GhostState.Normal;

            if (spawnArea != null)
                isOutsideSpawnArea = false;

            ChooseNextMove();
        }
    }

    private void Update()
    {
        if (!okToUpdate)
            return;
        
        animator.SetInteger("Mode", (int)State);
        if (State == GhostState.Dead)
        {
            MoveDeadGhost();
            return;
        }

        if (!isLerping)
        {
            ChooseNextMove();
            return;
        }

        lerpTime += Time.deltaTime;
        float t = Mathf.Clamp01(lerpTime / lerpDuration);

        transform.position = Vector3.Lerp(startPosition, targetPosition, t);

        if (t >= 1f)
        {
            isLerping = false;
            currentGridPosition = tileMap.WorldToCell(transform.position);
            transform.position = tileMap.GetCellCenterWorld(
                currentGridPosition);

            if (!isOutsideSpawnArea &&
                spawnExit != null &&
                currentGridPosition == exitGridPosition)
            {
                isOutsideSpawnArea = true;
            }

            // Immediately select the next tile.
            ChooseNextMove();
        }
    }

    public void SetState(GhostState newState)
    {
        State = newState;

        if (State == GhostState.Dead)
            isLerping = false;
    }

    // Event subscription and handling for scene state changes
    private void Start()
    {
        if (SceneStateManager.Instance != null)
        {
            SceneStateManager.Instance.OnSceneStateChanged += HandleStateChange;
            HandleStateChange(SceneStateManager.Instance.CurrentState);
            SceneStateManager.Instance.ResetGhostChanged += ResetGhost;
        }
    }

    private void HandleStateChange(SceneState newState)
    {
        switch (newState)
        {
            case SceneState.Intro:
                break;
            case SceneState.Normal:
                StartAfterIntro();
                okToUpdate = true;
                break;
            case SceneState.Scared:
                SetState(GhostState.Scared);
                break;
            case SceneState.TemporaryDeath:
                okToUpdate = false;
                break;
            case SceneState.GameOver:
                okToUpdate = false;
                break;
            case SceneState.ScaredEnding:
                StartCoroutine(FlashRoutine());
                break;
        }
    }

    public void ResetGhost()
    {
        transform.position = tileMap.GetCellCenterWorld(spawnGridPosition);
        currentGridPosition = spawnGridPosition;
        previousGridPosition = spawnGridPosition;
        currentDirection = Vector3Int.zero;
        isLerping = false;

        isOutsideSpawnArea = false;
        State = GhostState.Normal;

        ChooseNextMove();
    }

    private IEnumerator FlashRoutine()
    {
        // Make the sprite flash until the ghost isn't scared
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        while (State == GhostState.Scared)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.2f);
        }
        spriteRenderer.enabled = true; // Ensure the sprite is visible at the end
    }
}