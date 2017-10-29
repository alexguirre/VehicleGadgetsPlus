namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    internal enum SoundEffectState
    {
        None,
        Beginning,
        Looping,
        Ending,
    }

    internal sealed class SoundEffect : IDisposable
    {
        private SoundEffectState state;

        public SoundEffectState State
        {
            get => state;
            private set
            {
                if (value == state)
                    return;

                SoundEffectState old = state;
                state = value;
                OnStateChanged(old, state);
            }
        }
        public Sound Begin { get; }
        public Sound Loop { get; }
        public Sound End { get; }

        public SoundEffect(Sound begin, Sound loop, Sound end)
        {
            Begin = begin;
            Loop = loop;
            End = end;
        }

        public void Play()
        {
            if (State == SoundEffectState.None || State == SoundEffectState.Ending)
            {
                State = SoundEffectState.Beginning;
            }
        }

        public void Stop()
        {
            if (State != SoundEffectState.None && State != SoundEffectState.Ending)
            {
                State = SoundEffectState.Ending;
            }
        }

        public void ImmediateStop()
        {
            if (State != SoundEffectState.None)
            {
                State = SoundEffectState.None;
            }
        }

        public void Update()
        {
            switch (State)
            {
                case SoundEffectState.Beginning:
                    if (Begin == null || !Begin.IsPlaying)
                    {
                        State = SoundEffectState.Looping;
                    }
                    break;
                case SoundEffectState.Looping:
                    if(Loop == null)
                    {
                        State = SoundEffectState.Ending;
                    }
                    break;
                case SoundEffectState.Ending:
                    if (End == null || !End.IsPlaying)
                    {
                        State = SoundEffectState.None;
                    }
                    break;
            }
        }
        
        private void OnStateChanged(SoundEffectState oldState, SoundEffectState newState)
        {
            Sound s = GetSoundForState(oldState);
            if (s != null && s.IsPlaying)
            {
                s.Stop();
            }

            switch (newState)
            {
                case SoundEffectState.Beginning:
                    Begin?.Play();
                    break;
                case SoundEffectState.Looping:
                    Loop?.Play();
                    break;
                case SoundEffectState.Ending:
                    End?.Play();
                    break;
            }
        }

        private Sound GetSoundForState(SoundEffectState state)
        {
            switch (state)
            {
                default:
                case SoundEffectState.None: return null;
                case SoundEffectState.Beginning: return Begin;
                case SoundEffectState.Looping: return Loop;
                case SoundEffectState.Ending: return End;
            }
        }

        public void Dispose()
        {
            ImmediateStop();
            Begin?.Dispose();
            Loop?.Dispose();
            End?.Dispose();
        }
    }
}
