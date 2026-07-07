using UnityEngine;

public class OccludableAudio : MonoBehaviour
{
    public AudioSource Source;
    public AudioLowPassFilter Filter;

    private float MaxDistance;
    
    public AudioListener Listener;
    public float OccludedDistance = 20.0f;
    public float FadeSpeed = 50.0f;
    public LayerMask Mask;
    public float FrequencyChangeSpeed = 20000f;
    public float TargetLowPassFrequency = 1000;
    const float MaxLowPassFrequency = 22000;
    
    private void OnEnable()
    {
        MaxDistance = Source.maxDistance;
    }

    void Update()
    {
        float targetMaxDistance;
        float targetLowPassFrequency;
        if (Physics.Linecast(Listener.transform.position, transform.position, Mask))
        {
            targetMaxDistance = OccludedDistance;
            targetLowPassFrequency = TargetLowPassFrequency;
        }
        else
        {
            targetMaxDistance = MaxDistance;
            targetLowPassFrequency = MaxLowPassFrequency;
        }
        Source.maxDistance = Mathf.MoveTowards(Source.maxDistance, targetMaxDistance, Time.deltaTime * FadeSpeed);
        Filter.cutoffFrequency = Mathf.MoveTowards(Filter.cutoffFrequency, targetLowPassFrequency, Time.deltaTime * FrequencyChangeSpeed);
    }
}
