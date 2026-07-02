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
    private float OutputSampleRate;

    void OnEnable()
    {
        Debug.Log(AudioSettings.outputSampleRate);
        OutputSampleRate = AudioSettings.outputSampleRate;
        int capacity = SampleFrequency;
        int targetLatency = SampleFrequency * 20 / 1000; // 40 ms delay
        Buffer = new JitterBuffer(capacity, targetLatency);
        phase = 0f;
        previousSample = 0;
        nextSample = 0;
        //Clip = AudioClip.Create("streaming clip", MicrophoneSender.MaximumPayloadSamples * 4, 1, SampleFrequency, true, OnAudioRead);
        //Source.clip = Clip;
        Source.loop = true;
        Source.Play();

        
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float step = (float)SampleFrequency / OutputSampleRate;

        for (int frame = 0; frame < data.Length / channels; frame++)
        {
            while (phase >= 1f)
            {
                previousSample = nextSample;
                nextSample = Buffer.ReadSample();
                phase -= 1f;
            }

            float sample = Mathf.Lerp(previousSample, nextSample, phase);

            phase += step;

            int index = frame * channels;

            for (int c = 0; c < channels; c++)
                data[index + c] = sample;
        }
    }

    void OnAudioRead(float[] data)
    {
        Debug.Log($"Played chunk at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture)}");
        Debug.Log($"buffer before read: {Buffer.BufferedSamples}");
        Buffer.Read(data);
        Debug.Log($"requested data: {data.Length}");
        Debug.Log($"buffer after read: {Buffer.BufferedSamples}");
    }

    public void EnqueueData(Span<float> data)
    {
        Buffer.Enqueue(data);
        Debug.Log($"jitter buffer samples: {Buffer.BufferedSamples}");
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
