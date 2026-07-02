using UnityEngine;

public class MicrophoneLatencyCheck : MonoBehaviour
{
    public AudioSource source;

    void Start()
    {
        source.clip = Microphone.Start(null, true, 10, 16000);

        while (Microphone.GetPosition(null) <= 0)
        {
        }

        source.loop = true;
        source.Play();
    }
}
