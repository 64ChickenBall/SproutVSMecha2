using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    [SerializeField] private Tilemap tileMap;

    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;

    private bool hasInput = false;
    private bool isLerping = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float lerpTime = 0f;
    private float lerpDuration = 0.2f; // Duration of the lerp in seconds

    private Vector3Int currentGridPosition;

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource footstepAudioSource;

    [SerializeField] private ParticleSystem particleSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentGridPosition = tileMap.WorldToCell(transform.position);
        transform.position = tileMap.GetCellCenterWorld(currentGridPosition);
    }

    void SetInput(Vector2Int input)
    {
        if (input != Vector2Int.zero)
        {
            lastInput = input;
            hasInput = true;
        }
    }

    private bool IsWalkable(Vector3Int gridPosition)
    {
        if (!tileMap.HasTile(gridPosition))
        {
            return false;
        }
        string tileType = tileMap.GetTile(gridPosition).name;
        if (tileType == "Spritesheet_0" || tileType == "Spritesheet_1" || tileType == "Spritesheet_2" || tileType == "Spritesheet_19" || tileType == "Spritesheet_20"|| tileType == "Spritesheet_20")
        {
            return false;
        }
        return true;
    }

    private bool AttemptMove(Vector2Int direction)
    {
        
        if (direction == Vector2Int.zero)
        {
            return false;
        }
        Vector3Int newGridPosition = currentGridPosition + new Vector3Int(direction.x, direction.y, 0);
        if (!IsWalkable(newGridPosition))
        {
            UpdateAnimationAndAudio(Vector2Int.zero);
            return false;
        }
        currentInput = direction;
        startPosition = transform.position;
        targetPosition = tileMap.GetCellCenterWorld(newGridPosition);
        lerpTime = 0f;
        isLerping = true;
        UpdateAnimationAndAudio(direction);
        return true;
    }

    private void UpdateAnimationAndAudio(Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", 0);
            footstepAudioSource.Stop();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            return;
        }

        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
        if (!footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Play();
            if (!particleSystem.isPlaying)
            {
                particleSystem.Play();
                Debug.Log("Particle system started.");
            }
        }
    }

    private void MoveTowardsTarget()
    {
        lerpTime += Time.deltaTime;
        float t = Mathf.Clamp01(lerpTime / lerpDuration);
        transform.position = Vector3.Lerp(startPosition, targetPosition, t);

        if (t >= 1f)
        {
            isLerping = false;
            currentGridPosition += new Vector3Int(currentInput.x, currentInput.y, 0);
            transform.position = tileMap.GetCellCenterWorld(currentGridPosition);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            SetInput(Vector2Int.up);
        }
        if (Input.GetKey(KeyCode.S))
        {
            SetInput(Vector2Int.down);
        }
        if (Input.GetKey(KeyCode.A))
        {
            SetInput(Vector2Int.left);
        }
        if (Input.GetKey(KeyCode.D))
        {
            SetInput(Vector2Int.right);
        }

        if (isLerping)
        {
            MoveTowardsTarget();
            return;
        }
        if (!hasInput)
        {
            return;
        }
        if (AttemptMove(lastInput))
        {
            return;
        }
        AttemptMove(currentInput);
    }
}
