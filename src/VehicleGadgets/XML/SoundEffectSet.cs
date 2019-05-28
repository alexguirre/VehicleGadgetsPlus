namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    using Rage;

    public sealed class SoundEffectSet
    {
        public static readonly string Default = "default";

        // 0 - 100
        public int Volume { get; set; }
        // "default" or filename with extension from "Vehicle Gadgets+\Sounds\"
        [XmlElement(IsNullable = true)] public string Begin { get; set; }
        [XmlElement(IsNullable = true)] public string Loop { get; set; }
        [XmlElement(IsNullable = true)] public string End { get; set; }

        [XmlIgnore] public float NormalizedVolume => MathHelper.Clamp(Volume, 0, 100) / 100.0f;

        [XmlIgnore] public bool HasBegin => Begin != null;
        [XmlIgnore] public bool HasLoop => Loop != null;
        [XmlIgnore] public bool HasEnd => End != null;

        [XmlIgnore] public bool IsDefaultBegin => HasBegin && Begin.Equals(Default, StringComparison.InvariantCultureIgnoreCase);
        [XmlIgnore] public bool IsDefaultLoop => HasLoop && Loop.Equals(Default, StringComparison.InvariantCultureIgnoreCase);
        [XmlIgnore] public bool IsDefaultEnd => HasEnd && End.Equals(Default, StringComparison.InvariantCultureIgnoreCase);

        [XmlIgnore]
        public string BeginSoundFilePath
        {
            get
            {
                if (IsDefaultBegin)
                    return null;

                return Path.Combine(Plugin.SoundsFolder, Begin);
            }
        }

        [XmlIgnore]
        public string LoopSoundFilePath
        {
            get
            {
                if (IsDefaultLoop)
                    return null;

                return Path.Combine(Plugin.SoundsFolder, Loop);
            }
        }

        [XmlIgnore]
        public string EndSoundFilePath
        {
            get
            {
                if (IsDefaultEnd)
                    return null;

                return Path.Combine(Plugin.SoundsFolder, End);
            }
        }
    }
}
