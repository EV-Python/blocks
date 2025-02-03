using UnityEngine;

public class SpawnerPulse : MonoBehaviour
{
    public Vector3 pulseScale = Vector3.one;
    private float pulseSpeed = 2f;
    private float pulseAmount = 0.1f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = pulseScale;
    }

    void Update()
    {
        // Create a pulsing effect using a sine wave
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
} 