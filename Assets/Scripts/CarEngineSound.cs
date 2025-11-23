using UnityEngine;

public class CarEngineSound : MonoBehaviour
{
    private AudioSource audioSource;
    private Rigidbody rb;

    public float minPitch = 0.5f;
    public float maxPitch = 2.0f;
    public float maxSpeedForPitch = 50.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (rb == null)
        {
            Debug.LogError("CarEngineSound 스크립트에는 Rigidbody 컴포넌트가 필요합니다!");
            enabled = false;
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("CarEngineSound 스크립트에는 AudioSource 컴포넌트가 필요합니다!");
            enabled = false;
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeedForPitch);

        float newPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

        audioSource.pitch = newPitch;
    }
}