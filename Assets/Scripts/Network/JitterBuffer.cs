using System;

public sealed class JitterBuffer
{
    private readonly float[] buffer;

    private int readIndex;
    private int writeIndex;
    private int count;

    // Don't begin playback until this many samples have accumulated.
    private readonly int targetLatencySamples;

    private bool started;

    public int BufferedSamples => count;

    public JitterBuffer(int capacity, int targetLatencySamples)
    {
        if (targetLatencySamples >= capacity)
            throw new ArgumentException();

        buffer = new float[capacity];
        this.targetLatencySamples = targetLatencySamples;
    }

    public void Enqueue(ReadOnlySpan<float> samples)
    {
        lock (buffer)
        {
            foreach (float sample in samples)
            {
                // Buffer full?
                if (count == buffer.Length)
                {
                    // Drop the oldest sample.
                    readIndex++;
                    if (readIndex == buffer.Length)
                        readIndex = 0;

                    count--;
                }

                buffer[writeIndex] = sample;

                writeIndex++;
                if (writeIndex == buffer.Length)
                    writeIndex = 0;

                count++;
            }

            if (!started && count >= targetLatencySamples)
                started = true;
        }
    }

    public float ReadSample()
    {
        lock (buffer)
        {
            if (!started)
                return 0;

            if (count == 0)
            {
                started = false;
                return 0;
            }

            float sample = buffer[readIndex];

            readIndex++;
            if (readIndex == buffer.Length)
                readIndex = 0;

            count--;

            return sample;
        }
    }

    public void Read(Span<float> output)
    {
        lock (buffer)
        {
            if (!started)
            {
                output.Clear();
                return;
            }

            for (int i = 0; i < output.Length; i++)
            {
                if (count == 0)
                {
                    output[i] = 0f;
                    continue;
                }

                output[i] = buffer[readIndex];

                readIndex++;
                if (readIndex == buffer.Length)
                    readIndex = 0;

                count--;
            }

            // If we completely underrun, wait until we've buffered
            // enough audio again.
            if (count == 0)
                started = false;
        }
    }
}