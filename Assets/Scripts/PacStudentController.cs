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

    [SerializeField] private GameObject scoreControllerObject;
    [SerializeField] private AudioClip budCollectSound;
    [SerializeField] private AudioClip wallHitSound;
    [SerializeField] private AudioSource oneShotAudioSource;

    [SerializeField] private GameObject teleporter1;
    [SerializeField] private GameObject teleporter2;

    [SerializeField] private TileBase grassTile;
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
            //Play audio for hitting a wall
            footstepAudioSource.PlayOneShot(wallHitSound);
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
            // Check for teleporters
            if (currentGridPosition == tileMap.WorldToCell(teleporter1.transform.position))
            {
                currentGridPosition = tileMap.WorldToCell(teleporter2.transform.position);
                transform.position = tileMap.GetCellCenterWorld(currentGridPosition);
            }
            else if (currentGridPosition == tileMap.WorldToCell(teleporter2.transform.position))
            {
                currentGridPosition = tileMap.WorldToCell(teleporter1.transform.position);
                transform.position = tileMap.GetCellCenterWorld(currentGridPosition);
            }

            // Check the tile type at the new pos and replace/ add score if needed
            string tileType = tileMap.GetTile(currentGridPosition).name;


            // Tile is a uneaten pellet, replac and add score 39->59

            if (tileType == "Spritesheet_39")
            {
                tileMap.SetTile(currentGridPosition, grassTile);
                scoreControllerObject.GetComponent<ScoreController>().UpdateScore(10);
            }

            if (tileType == "New Animated Tile")
            {
                tileMap.SetTile(currentGridPosition, grassTile);
                scoreControllerObject.GetComponent<ScoreController>().UpdateScore(50);

                // Go into powerup mode for 10 seconds
            }
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bud"))
        {
            oneShotAudioSource.PlayOneShot(budCollectSound);
            scoreControllerObject.GetComponent<ScoreController>().UpdateScore(100);
            Destroy(other.gameObject);
        }
    }
}
