using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using NAudio.Wave;

namespace Simple_Platformer.Game.MainGame.Other
{ 

public class Mp3Player : IDisposable
    {
        private WaveOutEvent waveOut;
        private Mp3FileReader mp3Reader;
        private BeatTrackingSampleProvider beatTracker;

        public event EventHandler<BeatEventArgs> BeatDetected;

        public float BeatSensitivity
        {
            get => beatTracker.Sensitivity;
            set => beatTracker.Sensitivity = value;
        }

        public float EnergyDecayRate
        {
            get => beatTracker.EnergyDecayRate;
            set => beatTracker.EnergyDecayRate = value;
        }

        public Mp3Player(string filePath)
        {
            mp3Reader = new Mp3FileReader(filePath);
            beatTracker = new BeatTrackingSampleProvider(mp3Reader.ToSampleProvider());
            beatTracker.BeatDetected += (sender, args) => BeatDetected?.Invoke(this, args);
            waveOut = new WaveOutEvent();
            waveOut.Init(beatTracker);
        }

        public void Start() => waveOut.Play();

        public void Stop() => waveOut.Stop();

        public TimeSpan CurrentTime
        {
            get => mp3Reader.CurrentTime;
            set => mp3Reader.CurrentTime = value;
        }

        public void Dispose()
        {
            waveOut?.Dispose();
            mp3Reader?.Dispose();
        }
    }

    public class BeatTrackingSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;
        private float movingAverageEnergy;
        private readonly object syncLock = new object();

        public float Sensitivity { get; set; } = 1.3f;
        public float EnergyDecayRate { get; set; } = 0.95f;
        public event EventHandler<BeatEventArgs> BeatDetected;

        public BeatTrackingSampleProvider(ISampleProvider sourceProvider)
        {
            this.sourceProvider = sourceProvider;
        }

        public WaveFormat WaveFormat => sourceProvider.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = sourceProvider.Read(buffer, offset, count);
            DetectBeat(buffer, offset, samplesRead);
            return samplesRead;
        }

        private void DetectBeat(float[] buffer, int offset, int count)
        {
            float sum = 0;
            for (int i = 0; i < count; i++)
            {
                float sample = buffer[offset + i];
                sum += sample * sample;
            }

            float instantEnergy = sum / count;
            UpdateEnergyThreshold(instantEnergy);

            if (instantEnergy > movingAverageEnergy * Sensitivity)
            {
                BeatDetected?.Invoke(this, new BeatEventArgs { BeatTime = DateTime.Now });
            }
        }

        private void UpdateEnergyThreshold(float instantEnergy)
        {
            lock (syncLock)
            {
                if (movingAverageEnergy == 0)
                {
                    movingAverageEnergy = instantEnergy;
                }
                else
                {
                    movingAverageEnergy = EnergyDecayRate * movingAverageEnergy +
                                       (1 - EnergyDecayRate) * instantEnergy;
                }
            }
        }
    }

    public class BeatEventArgs : EventArgs
    {
        public DateTime BeatTime { get; set; }
    }
}
