using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private AudioClip budCollectSound;
    [SerializeField] private AudioClip wallHitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip killSound;
    [SerializeField] private AudioClip powerupSound;
    [SerializeField] private AudioSource oneShotAudioSource;

    [SerializeField] private GameObject teleporter1;
    [SerializeField] private GameObject teleporter2;

    [SerializeField] private TileBase grassTile;

    private int lives = 3; // Starting number of lives
    [SerializeField] private LivesBarScript livesBarScript; // Reference to the LivesBarScript

    private Vector3Int startingGridPosition; // Store the starting grid position for respawn

    private Coroutine powerUpCoroutine; // Reference to the power-up coroutine
    [SerializeField] private TMP_Text scaredtext;
    [SerializeField] private GameObject BackgroundMusicPlayerObject; // Reference to the BackgroundMusicPlayer GameObject

    [SerializeField] private SillySpeedUp Silly;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentGridPosition = tileMap.WorldToCell(transform.position);
        transform.position = tileMap.GetCellCenterWorld(currentGridPosition);
        startingGridPosition = currentGridPosition;
        livesBarScript.InitializeLives(lives);
        animator.SetBool("IsSproutDead", false); // Initialize the lives bar with the starting number of lives
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
        if (tileType == "Spritesheet_0" || tileType == "Spritesheet_1" || tileType == "Spritesheet_2" || tileType == "Spritesheet_19" || tileType == "Spritesheet_20"|| tileType == "Spritesheet_21")
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
                oneShotAudioSource.PlayOneShot(powerupSound); // Play power-up sound
                // Go into powerup mode for 10 seconds
                if (powerUpCoroutine != null)
                {
                    StopCoroutine(powerUpCoroutine);
                }
                powerUpCoroutine = StartCoroutine(PowerUpMode());
            }

            // check if any spritesheet_39 or newanimatedtiles are left if not end the game
            BoundsInt bounds = tileMap.cellBounds;
            bool found_it = false;
            string tileType2;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                TileBase tile = tileMap.GetTile(pos);
                if (tile == null) {
                    continue;
                }
                tileType2 = tile.name;
                if (tileType2 == "Spritesheet_39" || tileType2 == "New Animated Tile")
                {
                    found_it = true;
                }
            }
            if (!found_it) {
                SceneStateManager.Instance.SetState(SceneState.GameOver);
                //Check current scene to choose which score to submit
                string levelKey;

                if (SceneManager.GetActiveScene().name == "Level1")
                {
                    levelKey = "Level1";
                }
                else
                {
                    levelKey = "Level2";
                }

                SaveHighScore(levelKey);
            }
        }
    }

    private IEnumerator PowerUpMode()
    {
        scaredtext.gameObject.SetActive(true);
        scaredtext.text = "10";
        // Set the game state to power-up mode
        SceneStateManager.Instance.SetState(SceneState.Scared);
        for (int i = 10; i > 3; i--)
        {
            scaredtext.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        SceneStateManager.Instance.SetState(SceneState.ScaredEnding);

        for (int i = 3; i > 0; i--)
        {
            scaredtext.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        // Revert the game state back to normal
        SceneStateManager.Instance.SetState(SceneState.Normal);
        scaredtext.gameObject.SetActive(false);
    }

    void Update()
    {
        if (SceneStateManager.Instance.CurrentState == SceneState.Intro || SceneStateManager.Instance.CurrentState == SceneState.TemporaryDeath || SceneStateManager.Instance.CurrentState == SceneState.GameOver)
        {
            return; // Skip input handling if in Intro, TemporaryDeath, or GameOver state
        }
        if (Silly != null) {
            lerpDuration = 0.5f/Silly.speedup;
            Debug.Log(lerpDuration);
        }
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
        Debug.Log($"Collided with: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        if (other.CompareTag("Bud"))
        {
            oneShotAudioSource.PlayOneShot(budCollectSound);
            scoreControllerObject.GetComponent<ScoreController>().UpdateScore(100);
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Robot"))
        {
            // Handle collision with robot
            Debug.Log("Collided with Robot!");
            if (SceneStateManager.Instance.CurrentState == SceneState.Scared || SceneStateManager.Instance.CurrentState == SceneState.ScaredEnding)
            {
                oneShotAudioSource.PlayOneShot(killSound); // Play kill sound
                // If in scared mode, destroy the ghost and add score
                scoreControllerObject.GetComponent<ScoreController>().UpdateScore(300);
                BackgroundMusicPlayerObject.GetComponent<BackgroundMusicPlayer>().PlayDeadMusic(); // Play the dead music
                other.gameObject.GetComponent<GhostController>().Kill(); // kill the ghost temp
            }
            else
            {
                SceneStateManager.Instance.SetState(SceneState.TemporaryDeath);
                lives--; // Decrease lives by 1
                livesBarScript.UpdateLives(lives); // Update the lives bar
                oneShotAudioSource.PlayOneShot(deathSound); 
                animator.SetBool("IsSproutDead", true); // Trigger death animation
                Invoke(nameof(DeathContinue), 1.0f); // Wait for 1 second before continuing
                
            }
        }
    }

    private void DeathContinue()
    {
        if (lives <= 0)
        {
            Debug.Log("Game Over!");
            SceneStateManager.Instance.SetState(SceneState.GameOver);
            //Check current scene to choose which score to submit
            string levelKey;

            if (SceneManager.GetActiveScene().name == "Level1")
            {
                levelKey = "Level1";
            }
            else
            {
                levelKey = "Level2";
            }

            SaveHighScore(levelKey);
        }
        else
        {
            // Play death sound
            // Reset PacStudent's position to the starting point or a safe location
            currentGridPosition = startingGridPosition;
            transform.position = tileMap.GetCellCenterWorld(currentGridPosition);
            SceneStateManager.Instance.ResetGhosts();
            animator.SetBool("IsSproutDead", false); // Reset ghosts to their initial positions
            SceneStateManager.Instance.SetState(SceneState.Normal); // Resume the game
        }
                
    }


    private void SaveHighScore(string levelKey)
    {
        int currentScore = int.Parse(scoreText.text);

        int currentTime = TimeToCentiseconds(timeText.text);

        string scoreKey = levelKey + "_HighScore";
        string timeKey = levelKey + "_BestTime";

        int previousScore = PlayerPrefs.GetInt(scoreKey, -1);
        int previousTime = PlayerPrefs.GetInt(timeKey, int.MaxValue);

        if (currentScore > previousScore ||
            (currentScore == previousScore &&
            currentTime < previousTime))
        {
            PlayerPrefs.SetInt(scoreKey, currentScore);
            PlayerPrefs.SetInt(timeKey, currentTime);
            PlayerPrefs.Save();

            Debug.Log("New record saved for " + levelKey);
        }
    }

    private int TimeToCentiseconds(string timeString)
    {
        string[] parts = timeString.Split(':');

        if (parts.Length != 3)
        {
            Debug.LogError("Invalid time format: " + timeString);
            return int.MaxValue;
        }

        int minutes = int.Parse(parts[0]);
        int seconds = int.Parse(parts[1]);
        int centiseconds = int.Parse(parts[2]);

        return (minutes * 60 * 100)
            + (seconds * 100)
            + centiseconds;
    }

}
