using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using Relunrel.Connections;
using UnityEngine;

public class VoiceChatReceiver : MonoBehaviour
{
    public VoiceChatSpeaker Speaker;
    
    private int OutputRate;
    private readonly int InputRate = 16000;
    Connection Connection;
    float[] Samples;
    private double SourcePosition = 0f;
    private float PreviousSample = 0f;
    private bool HasPreviousSample = false;

    private float[] Decoded = new float[MicrophoneSender.MaximumPayloadSamples];
    private float[] Output;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        OutputRate = AudioSettings.outputSampleRate;
        Samples = new float[Mathf.CeilToInt(MicrophoneSender.MaximumPayloadSamples * (float)OutputRate / InputRate) + 2];
        Connection = FindAnyObjectByType<ClientNetwork>().CurrentConnection;
    }

    // Update is called once per frame
    void Update()
    {
        while (Connection.UnreliableOrderedMessagesAvailable)
        {
            byte[] data = Connection.DequeueUnreliableOrderedMessage();
            Span<float> samples = ConvertShortBytesToFloatSamplesUpsampled(data);
            Speaker.EnqueueData(samples);
        }
    }

    Span<float> ConvertShortBytesToFloatSamples(Span<byte> shortBytes)
    {
        Span<float> samples = Samples.AsSpan(0);
        int sampleCount = shortBytes.Length / 2;
        while(shortBytes.Length >= 2)
        {
            short shortSample = BinaryPrimitives.ReadInt16BigEndian(shortBytes);
            float sample = shortSample / 32767.0f;
            samples[0] = sample;
            samples = samples[1..];
            shortBytes = shortBytes[2..];
        }
        return Samples.AsSpan(0, sampleCount);
    }

    Span<float> ConvertShortBytesToFloatSamplesUpsampled(ReadOnlySpan<byte> bytes)
    {
        int inputCount = bytes.Length / 2;

        // Decode PCM16 once.
        for (int i = 0; i < inputCount; i++)
        {
            short sample = BinaryPrimitives.ReadInt16BigEndian(bytes.Slice(i * 2, 2));
            Decoded[i] = sample / 32767f;
        }

        double step = (double)InputRate / OutputRate;

        int outputCount = 0;

        while (SourcePosition < inputCount)
        {
            int leftIndex = (int)SourcePosition;
            double frac = SourcePosition - leftIndex;

            float left;
            float right;

            if (leftIndex == 0)
            {
                // Interpolate between the previous packet and this packet.
                left = HasPreviousSample ? PreviousSample : Decoded[0];
                right = Decoded[0];
            }
            else
            {
                left = Decoded[leftIndex - 1];
                right = Decoded[leftIndex];
            }

            Samples[outputCount++] =
                Mathf.Lerp(left, right, (float)frac);

            SourcePosition += step;
        }

        // Carry the remaining fractional position into the next packet.
        SourcePosition -= inputCount;

        PreviousSample = Decoded[inputCount - 1];
        HasPreviousSample = true;

        return Samples.AsSpan(0, outputCount);
    }
}
