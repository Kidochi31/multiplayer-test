using System;
using System.Linq;
using UnityEngine;

public class VoiceChatSpeaker : MonoBehaviour
{
    public AudioSource Source;
    private AudioClip Clip;
    private int SampleFrequency = 16000;
    private float[] SampleBuffer;
    private int BufferStart;
    private int BufferNext;
    private int Count => (BufferNext - BufferStart + SampleBuffer.Length) % SampleBuffer.Length;


    void OnEnable()
    {
        SampleBuffer = new float[SampleFrequency];
        BufferStart = 0;
        BufferNext = 0;
        Clip = AudioClip.Create("streaming clip", SampleFrequency / 25, 1, SampleFrequency, true, OnAudioRead);
        Source.clip = Clip;
        Source.loop = true;
        Source.Play();
    }

    void OnAudioRead(float[] data)
    {
        lock(SampleBuffer){
            Span<float> target = data.AsSpan();
            while(target.Length > 0)
            {
                if(Count > 0)
                {
                    Span<float> source = SampleBuffer.AsSpan(BufferStart);
                    Span<float> writeSource = source.Length > target.Length ? source[0..target.Length] : source;
                    if(writeSource.Length > Count)
                    {
                        writeSource = writeSource[0..Count];
                    }
                    writeSource.CopyTo(target);
                    BufferStart = (BufferStart + writeSource.Length) % SampleBuffer.Length;
                    target = target[writeSource.Length..];
                }
                else
                {
                    target.Fill(0f);
                    target = Span<float>.Empty;
                }
            }
        }
    }

    public void EnqueueData(Span<float> data)
    {
        lock(SampleBuffer)
        {
            int dataLength = data.Length;
            Span<float> target = SampleBuffer.AsSpan(BufferNext);
            while(data.Length > 0)
            {
                Span<float> writeData = data.Length > target.Length ? data[0..target.Length] : data;
                writeData.CopyTo(target);
                data = data[writeData.Length..];
                target = SampleBuffer.AsSpan();
            }
            BufferNext = (BufferNext + dataLength) % SampleBuffer.Length;
        }
    }

    void OnDisable()
    {
        Destroy(Source);
        Destroy(Clip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
