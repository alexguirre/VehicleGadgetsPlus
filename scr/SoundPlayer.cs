namespace VehicleGadgetsPlus
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using SlimDX;
    using SlimDX.Multimedia;
    using SlimDX.DirectSound;
    
    internal class SoundPlayer : IDisposable
    {
        private DirectSound dSound;
        private Dictionary<string, SecondarySoundBuffer> cache;

        public bool IsDisposed { get; private set; }

        public int CacheCount { get { return cache.Count; } }

        public SoundPlayer()
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
                using (WaveStream wave = new WaveStream(getWaveStream()))
                {
                    SoundBufferDescription description = new SoundBufferDescription
                    {
                        SizeInBytes = (int)wave.Length,
                        Flags = BufferFlags.ControlVolume,
                        Format = wave.Format,
                    };

                    buffer = new SecondarySoundBuffer(dSound, description);
                    byte[] data = new byte[description.SizeInBytes];
                    wave.Read(data, 0, description.SizeInBytes);
                    buffer.Write(data, 0, LockFlags.None);
                    cache[id] = buffer;
                }
            }

            Play(buffer, loop, volume);
        }

        private void Play(SecondarySoundBuffer buffer, bool loop, float volume)
        {
            Rage.Game.LogTrivial($"Volume: {volume} | {(int)((1.0f - volume) * -4000.0f)}");
            buffer.Volume = (int)((1.0f - volume) * -4000.0f);
            buffer.Play(0, loop ? PlayFlags.Looping : PlayFlags.None);
        }

        public void Stop(string id)
        {
            if (cache.TryGetValue(id, out SecondarySoundBuffer buffer))
            {
                buffer.Stop();
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                }

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
