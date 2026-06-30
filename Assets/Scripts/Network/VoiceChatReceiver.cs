using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using Relunrel.Connections;
using UnityEngine;

public class VoiceChatReceiver : MonoBehaviour
{
    public VoiceChatSpeaker Speaker;
    
    Connection Connection;
    float[] Samples = new float[MicrophoneSender.MaximumPayloadSamples];


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Connection = FindAnyObjectByType<ClientNetwork>().CurrentConnection;
    }

    // Update is called once per frame
    void Update()
    {
        while (Connection.UnreliableOrderedMessagesAvailable)
        {
            byte[] data = Connection.DequeueUnreliableOrderedMessage();
            Span<float> samples = ConvertShortBytesToFloatSamples(data);
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
}
