using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class AudioMenu : MonoBehaviour
{
    public GameObject RecordButton;
    public GameObject PlayButton;
    public TMP_Text RecordButtonText;
    public TMP_Text PlayButtonText;
    bool Recording = false;
    bool Playing = false;
    public string StartRecordText = "Start Recording";
    public string StopRecordText = "Stop Recording";
    public string StartPlayText = "Play Recording";
    public string PausePlayText = "Pause Recording";
    public int MicrophoneClipLength = 10;
    public int TargetSampleFrequency = 16000;
    private int CurrentFrequency = 16000;
    private string? CurrentDevice = null;
    private AudioClip? CurrentMicrophoneClip = null;
    private AudioClip? CurrentSavedClip = null;
    private int NextMicrophoneSampleIndex = 0;
    public int SaveSamplePeriodMs = 30;
    private int SavedSamples = 0;
    private DateTime LastSampleSaved = DateTime.UnixEpoch;
    private TimeSpan SaveSamplePeriod => TimeSpan.FromMilliseconds(SaveSamplePeriodMs);

    public AudioSource AudioSource;

    void OnEnable()
    {
        Recording = false;
        Playing = false;
        RecordButtonText.text = StartRecordText;
        PlayButtonText.text = StartPlayText;
        PlayButton.SetActive(false);
        RecordButton.SetActive(true);
    }

    public void ToggleRecording()
    {
        if (Recording)
        {
            Recording = false;
            // stop recording
            SaveMicrophoneData();
            Microphone.End(CurrentDevice);
            // truncate the saved recording to fit the number of samples exactly
            // need to take all the data out of the old clip
            if(SavedSamples != 0)
            {
                float[] samples = new float[SavedSamples * CurrentSavedClip.channels];
                CurrentSavedClip.GetData(samples, 0);
                
                // copy the data into a new clip with exactly enough space
                CurrentSavedClip = AudioClip.Create("saved clip", SavedSamples, CurrentSavedClip.channels, CurrentSavedClip.frequency, false);
                CurrentSavedClip.SetData(samples, 0);

                // set play button to be active
                PlayButton.SetActive(true);
            }
            else
            {
                // set play button to be inactive
                PlayButton.SetActive(false);
            }
            
            RecordButton.SetActive(true);
            // set texts
            PlayButtonText.text = StartPlayText;
            RecordButtonText.text = StartRecordText;
        }
        else
        {
            if(Microphone.devices.Length > 0)
            {
                // start recording
                Recording = true;

                // default recording device
                CurrentDevice = null;
                Microphone.GetDeviceCaps(CurrentDevice, out int minFreq, out int maxFreq);
                CurrentFrequency = math.clamp(TargetSampleFrequency, minFreq, maxFreq);
                if(CurrentFrequency == 0)
                {
                    CurrentFrequency = TargetSampleFrequency;
                }
                // Start recording
                CurrentMicrophoneClip = Microphone.Start(CurrentDevice, true, MicrophoneClipLength, CurrentFrequency);
                NextMicrophoneSampleIndex = 0;
                SavedSamples = 0;
                CurrentSavedClip = AudioClip.Create("saved clip",CurrentFrequency,CurrentMicrophoneClip.channels,CurrentFrequency,false);

                // set only record button to be active
                PlayButton.SetActive(false);
                RecordButton.SetActive(true);
                // set texts
                PlayButtonText.text = StartPlayText;
                RecordButtonText.text = StopRecordText;
            }
        }
    }

    void Update()
    {
        if (Recording)
        {
            // if currently recording -> need to copy samples from microphone clip into the saved clip
            // only save every so often
            if(DateTime.UtcNow >= LastSampleSaved + SaveSamplePeriod)
            {
                LastSampleSaved = DateTime.UtcNow;
                SaveMicrophoneData();
            }
        }
    }

    void SaveMicrophoneData()
    {
        // position in microphone clip
        int currentPosition = Microphone.GetPosition(CurrentDevice);
        int newSamples = (currentPosition - NextMicrophoneSampleIndex + CurrentMicrophoneClip.samples) % CurrentMicrophoneClip.samples;
        int totalSamples = SavedSamples + newSamples;
        // not enough space -> create new clip
        if(CurrentSavedClip.samples < totalSamples)
        {
            // need to take all the data out of the old clip
            float[] samples = new float[CurrentSavedClip.samples * CurrentSavedClip.channels];
            CurrentSavedClip.GetData(samples, 0);

            // copy the data into a new clip with double space
            CurrentSavedClip = AudioClip.Create("saved clip", CurrentSavedClip.samples * 2, CurrentSavedClip.channels, CurrentSavedClip.frequency, false);
            CurrentSavedClip.SetData(samples, 0);
        }

        // next, need to copy the samples in from the microphone clip
        // if overflow -> need to do two copies -> otherwise one copy
        if(currentPosition < NextMicrophoneSampleIndex)
        {
            // overflow -> two copies
            // first copy is from NextMicrophoneSampleIndex to the end
            {
                // copy from NextMicrophoneSampleIndex to the end
                int numSamples = CurrentMicrophoneClip.samples - NextMicrophoneSampleIndex;
                if(numSamples != 0)
                {
                    float[] samples = new float[numSamples * CurrentSavedClip.channels];
                    CurrentMicrophoneClip.GetData(samples.AsSpan(),NextMicrophoneSampleIndex);

                    // copy samples into saved clip
                    CurrentSavedClip.SetData(samples.AsSpan(), SavedSamples);
                    // increment the saved samples
                    SavedSamples += numSamples;
                }
            }
            // second copy is from 0 to the currentPosition
            {
                // copy from 0 to the currentPosition
                int numSamples = currentPosition;
                if(numSamples != 0)
                {
                    float[] samples = new float[numSamples * CurrentSavedClip.channels];
                    CurrentMicrophoneClip.GetData(samples.AsSpan(),0);

                    // copy samples into saved clip
                    CurrentSavedClip.SetData(samples.AsSpan(), SavedSamples);
                    // increment the saved samples
                    SavedSamples += numSamples;
                }
            }
            NextMicrophoneSampleIndex = currentPosition;
        }
        else
        {
            // no overflow -> single copy
            // get samples from microphone
            int numSamples = currentPosition - NextMicrophoneSampleIndex;
            if(numSamples != 0){
                float[] samples = new float[numSamples * CurrentSavedClip.channels];
                CurrentMicrophoneClip.GetData(samples.AsSpan(),NextMicrophoneSampleIndex);

                // copy samples into saved clip
                CurrentSavedClip.SetData(samples.AsSpan(), SavedSamples);
                // increment the saved samples and change the sample index
                NextMicrophoneSampleIndex = currentPosition;
                SavedSamples += numSamples;
            }
        }
    }

    public void TogglePlaying()
    {
        if (Playing)
        {
            Playing = false;
            // pause playing
            AudioSource.Pause();

            // set both buttons to be active
            PlayButton.SetActive(true);
            RecordButton.SetActive(true);
            // set texts
            PlayButtonText.text = StartPlayText;
            RecordButtonText.text = StartRecordText;
        }
        else
        {
            Playing = true;
            AudioSource.clip = CurrentSavedClip;
            AudioSource.loop = true;
            AudioSource.Play();
            // set only play buttons to be active
            PlayButton.SetActive(true);
            RecordButton.SetActive(false);
            // set texts
            PlayButtonText.text = PausePlayText;
            RecordButtonText.text = StartRecordText;
        }
    }
}
