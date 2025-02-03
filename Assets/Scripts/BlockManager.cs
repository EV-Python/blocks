using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlockManager : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;
    public int mapSize = 8; // Smaller map size for better visibility
    [SerializeField] private float cameraSize = 5f; // Smaller camera size to match map
    [SerializeField] private Color borderColor = Color.black;
    [SerializeField] private float borderWidth = 0.2f; // Thicker border
    
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, Spawner> spawners = new Dictionary<Vector2Int, Spawner>();
    private Camera mainCamera;
    private LineRenderer borderRenderer;

    private void OnValidate()
    {
        // This runs in edit mode when values are changed in the inspector
        UpdateBorderAndCamera();
    }

    private void Start()
    {
        UpdateBorderAndCamera();
        SetupGame();
    }

    private void UpdateBorderAndCamera()
    {
        SetupCamera();
        CreateMapBorder();
    }

    private void CreateMapBorder()
    {
        // Find or create the border object
        Transform borderTransform = transform.Find("MapBorder");
        GameObject borderObject;

        if (borderTransform != null)
        {
            borderObject = borderTransform.gameObject;
            borderRenderer = borderObject.GetComponent<LineRenderer>();
        }
        else
        {
            borderObject = new GameObject("MapBorder");
            borderObject.transform.parent = this.transform;
            borderRenderer = borderObject.AddComponent<LineRenderer>();
        }
        
        // Configure line renderer
        borderRenderer.positionCount = 5;
        borderRenderer.startWidth = borderWidth;
        borderRenderer.endWidth = borderWidth;
        
        // Create or update material
        if (borderRenderer.material == null || borderRenderer.material.shader != Shader.Find("Sprites/Default"))
        {
            borderRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        borderRenderer.startColor = borderColor;
        borderRenderer.endColor = borderColor;
        
        // Set border positions
        borderRenderer.SetPositions(new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(mapSize, 0, 0),
            new Vector3(mapSize, mapSize, 0),
            new Vector3(0, mapSize, 0),
            new Vector3(0, 0, 0)
        });

        // Keep border slightly behind blocks
        borderObject.transform.localPosition = new Vector3(0, 0, 1);
    }

    private void SetupCamera()
    {
        // Find the main camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = cameraSize;
            mainCamera.transform.position = new Vector3(mapSize / 2f, mapSize / 2f, -10f);
        }
    }

    private void SetupGame()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("Block Prefab is not assigned in BlockManager!");
            return;
        }

        // Verify block prefab has required components
        if (blockPrefab.GetComponent<Block>() == null)
        {
            Debug.LogError("Block Prefab is missing the Block script!");
            return;
        }

        // Create initial block at the center of the map
        Vector2Int centerPosition = new Vector2Int(mapSize / 2, mapSize / 2);
        SpawnBlock(centerPosition);
        Debug.Log("Created initial block at center position: " + centerPosition);
    }

    public void RegisterSpawner(Spawner spawner, Vector2Int position)
    {
        if (IsWithinMapBounds(position))
        {
            spawners[position] = spawner;
            occupiedPositions.Add(position);
            Debug.Log($"Registered spawner at position: {position}");
        }
        else
        {
            Debug.LogError($"Attempted to place spawner outside map bounds at position: {position}");
        }
    }

    public void UpdateSpawnerPosition(Spawner spawner, Vector2Int newPosition)
    {
        // Remove old position
        Vector2Int oldPosition = spawners.FirstOrDefault(x => x.Value == spawner).Key;
        if (spawners.ContainsKey(oldPosition))
        {
            spawners.Remove(oldPosition);
            occupiedPositions.Remove(oldPosition);
        }

        // Add new position
        if (IsWithinMapBounds(newPosition))
        {
            spawners[newPosition] = spawner;
            occupiedPositions.Add(newPosition);
        }
    }

    void SpawnBlock(Vector2Int position)
    {
        if (blockPrefab == null)
        {
            Debug.LogError("Cannot spawn block - Block Prefab is not assigned!");
            return;
        }

        if (!IsWithinMapBounds(position))
        {
            Debug.LogWarning($"Attempted to spawn block outside bounds at {position}");
            return;
        }

        if (occupiedPositions.Contains(position))
        {
            Debug.LogWarning($"Position {position} is already occupied");
            return;
        }

        GameObject newBlock = Instantiate(blockPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        Block blockComponent = newBlock.GetComponent<Block>();
        
        if (blockComponent == null)
        {
            Debug.LogError("Spawned block is missing Block component!");
            Destroy(newBlock);
            return;
        }

        blockComponent.Init(this, position);
        Debug.Log($"Initialized block at position: {position}");

        // Set normal block color
        SpriteRenderer renderer = newBlock.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = new Color(1f, 1f, 1f, 0.9f);
        }

        occupiedPositions.Add(position);
        Debug.Log($"Successfully spawned block at position {position}");
    }

    public void OnBlockClicked(Vector2Int position)
    {
        // Check if clicked block is a spawner
        if (spawners.ContainsKey(position))
        {
            // Spawner behavior: Try to place block in available adjacent position
            Vector2Int[] spawnerDirections = new Vector2Int[]
            {
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down
            };

            foreach (Vector2Int dir in spawnerDirections)
            {
                Vector2Int newPos = position + dir;
                if (!occupiedPositions.Contains(newPos) && IsWithinMapBounds(newPos))
                {
                    SpawnBlock(newPos);
                    break;
                }
            }
            return;
        }

        // Normal block behavior
        Vector2Int newBlockPos = FindEmptyAdjacent(position);
        if (newBlockPos != position && IsWithinMapBounds(newBlockPos))
        {
            SpawnBlock(newBlockPos);
        }
    }

    bool IsWithinMapBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < mapSize &&
               position.y >= 0 && position.y < mapSize;
    }

    Vector2Int FindEmptyAdjacent(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = position + dir;
            if (IsWithinMapBounds(newPos) && !occupiedPositions.Contains(newPos))
            {
                return newPos;
            }
        }

        return FindEmptySpotExpanding(position);
    }

    Vector2Int FindEmptySpotExpanding(Vector2Int position)
    {
        int range = 1;
        while (range < Mathf.Min(mapSize, mapSize))
        {
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector2Int newPos = position + new Vector2Int(x, y);
                    if (IsWithinMapBounds(newPos) && !occupiedPositions.Contains(newPos))
                    {
                        return newPos;
                    }
                }
            }
            range++;
        }
        return position;
    }

#if UNITY_EDITOR
    // This ensures the border updates in the scene view
    private void OnDrawGizmos()
    {
        // Update border if it exists
        if (borderRenderer != null)
        {
            borderRenderer.startColor = borderColor;
            borderRenderer.endColor = borderColor;
            borderRenderer.startWidth = borderWidth;
            borderRenderer.endWidth = borderWidth;
        }
    }
#endif
}
