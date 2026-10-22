using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private int[,] levelMap =
    {
    {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
    {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
    {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
    {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
    {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
    {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
    {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
    {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
    {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
    {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
    {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
    {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
    {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
    {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
    {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject[] tilePrefabs; // Array of tile prefabs corresponding to tile types
    [SerializeField] private GameObject oldTileMap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);


        Destroy(oldTileMap);

        //duplicate quadrants to make a bigger map but ignore the bottom row to avoid duplicate tiles
        int[,] bigLevelMap = new int[rows * 2-1, cols * 2];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                bigLevelMap[i, j] = levelMap[i, j];
                bigLevelMap[i + rows - 1, j] = levelMap[rows - 1 - i, j];
                bigLevelMap[i, j + cols] = levelMap[i, cols - 1 - j];
                bigLevelMap[i + rows - 1, j + cols] = levelMap[rows - 1 - i, cols - 1 - j];
            }
        }
        mainCamera.orthographicSize = Mathf.Max(bigLevelMap.GetLength(1) /2f/mainCamera.aspect, bigLevelMap.GetLength(0) / 2f);

        // Generate level on bigmap
        for (int i = 0; i < bigLevelMap.GetLength(0); i++)
        {
            for (int j = 0; j < bigLevelMap.GetLength(1); j++)
            {
                int tileType = bigLevelMap[i, j];

                GameObject tilePrefab = Instantiate(tilePrefabs[tileType], new Vector3(j, -i, 0), Quaternion.identity, parent: transform);
                if (tileType == 1 || tileType == 3)
                {
                    int up = SafeArrayAccess(bigLevelMap, i - 1, j);
                    int down = SafeArrayAccess(bigLevelMap, i + 1, j);
                    int left = SafeArrayAccess(bigLevelMap, i, j - 1);
                    int right = SafeArrayAccess(bigLevelMap, i, j + 1);
                    
                    if ((up == 1 || up == 2) && (left == 1 || left == 2))
                    {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 180);
                    }
                    else if ((up == 2 || up == 1) && (right == 2 || right == 1))
                    {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 90);
                    }
                    else if ((down == 2 || down == 1) && (left == 2 || left == 1))
                    {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, -90);
                    }


                    // desperation
                    if ((down == 2 || down == 1) && (left == 2 || left == 1) &&(up == 2 || up == 1) && (right == 2 || right == 1)) {
                        int upright = SafeArrayAccess(bigLevelMap, i - 1, j + 1);
                        int upleft = SafeArrayAccess(bigLevelMap, i - 1, j - 1);
                        int downright = SafeArrayAccess(bigLevelMap, i + 1, j + 1);
                        int downleft = SafeArrayAccess(bigLevelMap, i + 1, j - 1);
                        if (upright == 0) {
                            tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 90);
                        }
                        else if (upleft == 0) {
                            tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 180);
                        }
                        else if (downright == 0) {
                            tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
                        }
                        else if (downleft == 0) {
                            tilePrefab.transform.rotation = Quaternion.Euler(0, 0, -90);
                        }
                    }



                }
                if (tileType == 2 || tileType == 4 || tileType == 8)
                {
                    if (SafeArrayAccess(bigLevelMap, i, j-1) == 0 || SafeArrayAccess(bigLevelMap, i, j+1) == 0)
                    {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 90);
                    }

                    // desperation part 2
                    
                    int up = SafeArrayAccess(bigLevelMap, i - 1, j);
                    int down = SafeArrayAccess(bigLevelMap, i + 1, j);
                    int left = SafeArrayAccess(bigLevelMap, i, j - 1);
                    int right = SafeArrayAccess(bigLevelMap, i, j + 1);
                    if (up+down+left+right < 2) {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                }
                if (tileType == 7) {
                    int down = SafeArrayAccess(bigLevelMap, i + 1, j);
                    int left = SafeArrayAccess(bigLevelMap, i, j - 1);
                    int right = SafeArrayAccess(bigLevelMap, i, j + 1);
                    
                    if (down != 2 && down != 1) {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 180);
                    }
                    else if (left != 2 && left != 1) {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, 90);
                    }
                    else if (right != 2 && right != 1) {
                        tilePrefab.transform.rotation = Quaternion.Euler(0, 0, -90);
                    }
                }
            }
        }

        //move camera to center of map
        mainCamera.transform.position = new Vector3(bigLevelMap.GetLength(1) / 2f - 0.5f, -bigLevelMap.GetLength(0) / 2f + 0.5f, -10);
    }

    public int SafeArrayAccess(int[,] array, int i, int j)
    {
        if (i < 0 || i >= array.GetLength(0) || j < 0 || j >= array.GetLength(1))
        {
            return 0;
        }
        if (array[i, j] == 0 || array[i, j] == 5 || array[i, j] == 6)
        {
            return 0;
        }
        if (array[i, j] == 2 || array[i, j] == 4 || array[i, j] == 8)
        {
            return 1;
        }
        if (array[i, j] == 1 || array[i, j] == 3 || array[i, j] == 7)
        {
            return 2;
        }
        return array[i, j];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
