using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class VoiceChatSpeaker : MonoBehaviour
{
    public AudioSource Source;
    private AudioClip Clip;
    private int SampleFrequency = 16000;
    private JitterBuffer Buffer;

    private float previousSample;
    private float nextSample;

    private float phase;
    private int OutputSampleRate;

    void OnEnable()
    {
        OutputSampleRate = AudioSettings.outputSampleRate;
        int capacity = OutputSampleRate;
        int targetLatency = OutputSampleRate * 20 / 1000; // 40 ms delay
        Buffer = new JitterBuffer(capacity, targetLatency);
        Source.loop = true;
        Source.Play();

        
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        Buffer.ReadInterleaved(data, channels);
    }

    // void OnAudioRead(float[] data)
    // {
    //     Debug.Log($"Played chunk at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture)}");
    //     Debug.Log($"buffer before read: {Buffer.BufferedSamples}");
    //     Buffer.Read(data);
    //     Debug.Log($"requested data: {data.Length}");
    //     Debug.Log($"buffer after read: {Buffer.BufferedSamples}");
    // }

    public void EnqueueData(Span<float> data)
    {
        Buffer.Enqueue(data);
    }

    void OnDisable()
    {
        Source.Stop();
        Destroy(Clip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
