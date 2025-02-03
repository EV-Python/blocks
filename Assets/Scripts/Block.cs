using UnityEngine;

public class Block : MonoBehaviour
{
    private BlockManager blockManager;
    private Vector2Int gridPosition;
    private bool isInitialized = false;

    private void Awake()
    {
        // Try to find BlockManager if we don't have one
        if (blockManager == null)
        {
            blockManager = Object.FindAnyObjectByType<BlockManager>();
        }
        SetupComponents();
    }

    private void SetupComponents()
    {
        // Ensure the block has a BoxCollider2D
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Make sure the collider size matches the sprite
        collider.size = Vector2.one;  // Set to 1x1 unit size

        // Ensure the block has a SpriteRenderer
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void Init(BlockManager manager, Vector2Int position)
    {
        blockManager = manager;
        gridPosition = position;
        isInitialized = true;
    }

    private void OnMouseDown()
    {
        // Try to find BlockManager if we don't have one
        if (blockManager == null)
        {
            blockManager = Object.FindAnyObjectByType<BlockManager>();
            if (blockManager == null)
            {
                Debug.LogError($"Block {gameObject.name} was clicked but no BlockManager exists in the scene!");
                return;
            }
        }

        if (!isInitialized)
        {
            Debug.LogError($"Block {gameObject.name} was clicked but hasn't been initialized with a grid position!");
            return;
        }
        
        blockManager.OnBlockClicked(gridPosition);
    }
} 