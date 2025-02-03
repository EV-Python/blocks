using UnityEngine;

public class Spawner : MonoBehaviour
{
    private BlockManager blockManager;
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;

    [Header("Spawner Settings")]
    public Color spawnerColor = new Color(1f, 0f, 0f, 0.8f);
    public Vector3 spawnerScale = new Vector3(1.2f, 1.2f, 1f);
    
    void Start()
    {
        // Find the BlockManager in the scene
        blockManager = FindObjectOfType<BlockManager>();
        if (blockManager == null)
        {
            Debug.LogError("No BlockManager found in the scene!");
            return;
        }

        // Register with BlockManager
        Vector2Int position = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        blockManager.RegisterSpawner(this, position);

        // Setup appearance
        SetupSpawnerAppearance();
    }

    private void SetupSpawnerAppearance()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = spawnerColor;
            transform.localScale = spawnerScale;
        }

        // Add pulsing effect
        var pulse = GetComponent<SpawnerPulse>();
        if (pulse == null)
        {
            pulse = gameObject.AddComponent<SpawnerPulse>();
        }
        pulse.pulseScale = spawnerScale;
    }

    public void UpdateGridPosition()
    {
        gridPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        blockManager.UpdateSpawnerPosition(this, gridPosition);
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
} 