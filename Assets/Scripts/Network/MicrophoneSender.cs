using System;
using System.Buffers.Binary;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

public class MicrophoneSender : MonoBehaviour
{
    public const int MaximumPayloadSize =  512;
    public const int MaximumPayloadSamples = MaximumPayloadSize / sizeof(short);
    const int MaxChannels = 5;
    private string? CurrentDevice = null;
    private int MicrophoneClipLength = 10;
    private int TargetSampleFrequency = 16000;
    const int MaxSampleFactor = 5;
    private bool Recording = false;
    private AudioClip MicrophoneClip;
    private int NextMicrophoneSampleIndex = 0;
    private ClientNetwork Client;

    private byte[] Payload = new byte[MaximumPayloadSize];
    private float[] MonoSamples = new float[MaximumPayloadSamples];
    private float[] ChannelledSamples = new float[MaximumPayloadSamples * MaxChannels * MaxSampleFactor];

    void OnEnable()
    {
        Client = FindAnyObjectByType<ClientNetwork>();
        if(Microphone.devices.Length > 0)
        {
            // start recording
            Recording = true;




            // default recording device
            CurrentDevice = null;
            Microphone.GetDeviceCaps(CurrentDevice, out int minFreq, out int maxFreq);
            int frequency = math.clamp(TargetSampleFrequency, minFreq, maxFreq);
            if(frequency == 0)
            {
                frequency = TargetSampleFrequency;
            }
            // Start recording
            MicrophoneClip = Microphone.Start(CurrentDevice, true, MicrophoneClipLength, frequency);
            NextMicrophoneSampleIndex = 0;
        }
    }

    void OnDisable()
    {
        if (Recording)
        {
            Microphone.End(CurrentDevice);
            if(MicrophoneClip!= null)
            {
                Destroy(MicrophoneClip);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Recording)
        {
            // position in microphone clip
            int currentPosition = Microphone.GetPosition(CurrentDevice);
            int requiredInputSamples = Mathf.CeilToInt(MaximumPayloadSamples * (float)MicrophoneClip.frequency / TargetSampleFrequency);
            while(true){
                int newSampleCount = (currentPosition - NextMicrophoneSampleIndex + MicrophoneClip.samples) % MicrophoneClip.samples;
                Debug.Log($"Sample count: {newSampleCount}");
                Debug.Log($"Requried samples: {requiredInputSamples}");
                // only send samples if there are sufficient samples available
                if(newSampleCount >= requiredInputSamples)
                {
                    Span<float> channelledSamples = GenerateChannelledSamples(requiredInputSamples);
                    Span<float> monoSamples = AverageChannelledSamples(channelledSamples);
                    Span<byte> samples = ConvertFloatSamplesToShortBytes(monoSamples);
                    byte[] sampleArray = samples.ToArray();
                    SendAudioMessage message = new SendAudioMessage(sampleArray);

                    Client.SendMessage(message, DateTime.UtcNow);
                    
                }
                else
                {
                    break;
                }
            }
        }
    }

    Span<float> GenerateChannelledSamples(int targetSamples)
    {
        int channels = MicrophoneClip.channels;
        // next, need to copy the samples in from the microphone clip
        Span<float> channelledSamples = ChannelledSamples.AsSpan();
        int numSamples = math.min(MicrophoneClip.samples - NextMicrophoneSampleIndex, targetSamples);
        if(numSamples > 0)
        {
            // load this many samples into channelledSamples
            MicrophoneClip.GetData(channelledSamples, NextMicrophoneSampleIndex);
            // change channelled samples to only be the remaining samples left to write
            channelledSamples = channelledSamples[(channels * numSamples)..];
            NextMicrophoneSampleIndex += numSamples;
        }
        int samplesRemaining = targetSamples - numSamples;
        if(samplesRemaining > 0)
        {
            // load this many samples into channelledSamples
            MicrophoneClip.GetData(channelledSamples, 0);
            NextMicrophoneSampleIndex = samplesRemaining;
        }
        return ChannelledSamples.AsSpan(0, targetSamples * channels);
    }

    // Span<float> AverageChannelledSamples(Span<float> channelledSamples)
    // {
    //     int channels = MicrophoneClip.channels;
    //     int numSamples = channelledSamples.Length / channels;
    //     Span<float> monoSamples = MonoSamples[..numSamples];
    //     for(int i = 0; i < numSamples; i++)
    //     {
    //         float sum = 0;
    //         for(int c = 0; c < channels; c++)
    //         {
    //             sum += channelledSamples[i * channels + c];
    //         }
    //         monoSamples[i] = sum / channels;
    //     }
    //     return monoSamples;
    // }

    Span<float> AverageChannelledSamples(Span<float> channelledSamples)
    {
        int channels = MicrophoneClip.channels;

        int inputFrames = channelledSamples.Length / channels;

        int outputFrames = Mathf.CeilToInt(inputFrames * (float)TargetSampleFrequency / MicrophoneClip.frequency);

        Span<float> monoSamples = MonoSamples.AsSpan(0, outputFrames);

        float samplesPerOutput = (float)MicrophoneClip.frequency / TargetSampleFrequency;

        for (int outFrame = 0; outFrame < outputFrames; outFrame++)
        {
            float start = outFrame * samplesPerOutput;
            float end   = (outFrame + 1) * samplesPerOutput;

            int first = Mathf.FloorToInt(start);
            int last  = Mathf.CeilToInt(end);

            float sum = 0;
            int count = 0;

            for (int inFrame = first; inFrame < last && inFrame < inputFrames; inFrame++)
            {
                for (int c = 0; c < channels; c++)
                {
                    sum += channelledSamples[inFrame * channels + c];
                }
                count++;
            }
            sum = sum / channels;

            monoSamples[outFrame] = count > 0 ? sum / count : 0;
        }

        return monoSamples;
    }

    Span<byte> ConvertFloatSamplesToShortBytes(Span<float> floatSamples)
    {
        Span<byte> payload = Payload.AsSpan(0, floatSamples.Length * 2);

        for (int i = 0; i < floatSamples.Length; i++)
        {
            // Clamp the float to prevent accidental out-of-bounds artifacts
            //float clamped = Mathf.Clamp(floatSamples[i], -1.0f, 1.0f);
            
            // Scale and convert to 16-bit signed integer\
            short shortSample = (short)(floatSamples[i] * 32767.0f);
            BinaryPrimitives.WriteInt16BigEndian(payload, shortSample);
            payload = payload[2..];
        }

        return Payload.AsSpan(0, floatSamples.Length * 2);
    }
}
