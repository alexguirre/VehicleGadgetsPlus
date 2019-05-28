namespace VehicleGadgetsPlus
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using SlimDX;
    using SlimDX.Multimedia;
    using SlimDX.DirectSound;

    internal class Sound : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public string Id { get; } = Guid.NewGuid().ToString();
        public bool Looped { get; }
        public float Volume { get; }
        public bool IsPlaying => SoundPlayer.Instance.IsPlaying(Id);

        private readonly Func<string> filePathGetter;
        private readonly Func<Stream> streamGetter;

        public Sound(bool looped, float volume, Func<string> waveFilePathGetter)
        {
            Looped = looped;
            Volume = volume;
            filePathGetter = waveFilePathGetter;
        }

        public Sound(bool looped, float volume, Func<Stream> waveStreamGetter)
        {
            Looped = looped;
            Volume = volume;
            streamGetter = waveStreamGetter;
        }

        ~Sound()
        {
            Dispose(false);
        }

        public void Play()
        {
            if (IsDisposed)
                throw new ObjectDisposedException($"Sound [{Id}]");

            if (filePathGetter != null)
            {
                SoundPlayer.Instance.Play(Id, Looped, Volume, filePathGetter);
            }
            else
            {
                SoundPlayer.Instance.Play(Id, Looped, Volume, streamGetter);
            }
        }

        public void Stop()
        {
            if (IsDisposed)
                throw new ObjectDisposedException($"Sound [{Id}]");

            SoundPlayer.Instance.Stop(Id);
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                SoundPlayer.Instance.Clean(Id);

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    internal class SoundPlayer : IDisposable
    {
        private static SoundPlayer soundPlayer;
        public static SoundPlayer Instance => soundPlayer ?? (soundPlayer = new SoundPlayer());
        public static bool IsInitialized => soundPlayer != null;

        private DirectSound dSound;
        private Dictionary<string, SecondarySoundBuffer> cache;

        public bool IsDisposed { get; private set; }
        
        private SoundPlayer()
        {
            dSound = new DirectSound();
            dSound.SetCooperativeLevel(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, CooperativeLevel.Normal);
            dSound.IsDefaultPool = false;

            cache = new Dictionary<string, SecondarySoundBuffer>();
        }

        ~SoundPlayer()
        {
            Dispose(false);
        }

        public bool IsPlaying(string id)
        {
            if (cache.TryGetValue(id, out SecondarySoundBuffer buffer))
            {
                return (buffer.Status & BufferStatus.Playing) == BufferStatus.Playing;
            }

            return false;
        }

        public void Clean(string id)
        {
            if (cache.TryGetValue(id, out SecondarySoundBuffer buffer))
            {
                buffer.Dispose();
                cache.Remove(id);
            }
        }

        public void Play(string id, bool loop, float volume, Func<Stream> getWaveStream)
        {
            if (!cache.TryGetValue(id, out SecondarySoundBuffer buffer))
            {
                using (Stream stream = getWaveStream())
                using (WaveStream wave = new WaveStream(stream))
                {
                    buffer = CreateBuffer(id, wave);
                }
            }

            Play(buffer, loop, volume);
        }

        public void Play(string id, bool loop, float volume, Func<string> getWaveFilePath)
        {
            if (!cache.TryGetValue(id, out SecondarySoundBuffer buffer))
            {
                string path = getWaveFilePath();
                if (File.Exists(path))
                {
                    using (WaveStream wave = new WaveStream(path))
                    {
                        buffer = CreateBuffer(id, wave);
                    }
                }
                else
                {
                    return;
                }
            }

            Play(buffer, loop, volume);
        }

        private void Play(SecondarySoundBuffer buffer, bool loop, float volume)
        {
            buffer.Volume = (int)((1.0f - volume) * -4000.0f);
            buffer.Play(0, loop ? PlayFlags.Looping : PlayFlags.None);
        }

        private SecondarySoundBuffer CreateBuffer(string id, WaveStream wave)
        {
            SoundBufferDescription description = new SoundBufferDescription
            {
                SizeInBytes = (int)wave.Length,
                Flags = BufferFlags.ControlVolume,
                Format = wave.Format,
            };

            SecondarySoundBuffer buffer = new SecondarySoundBuffer(dSound, description);
            byte[] data = new byte[description.SizeInBytes];
            wave.Read(data, 0, description.SizeInBytes);
            buffer.Write(data, 0, LockFlags.None);
            cache[id] = buffer;
            return buffer;
        }

        public void Stop(string id)
        {
            if (cache.TryGetValue(id, out SecondarySoundBuffer buffer))
            {
                buffer.Stop();
                buffer.CurrentPlayPosition = 0;
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                foreach (SecondarySoundBuffer b in cache.Values)
                {
                    if (b != null)
                    {
                        b.Stop();
                        b.Dispose();
                    }
                }
                cache = null;

                dSound?.Dispose();
                dSound = null;

                IsDisposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
