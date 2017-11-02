namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.Xml.Serialization;

    using Rage;

    [XmlType(TypeName = XmlName)]
    public sealed class LadderEntry : VehicleGadgetEntry
    {
        public const string XmlName = nameof(Ladder);

        [XmlIgnore] public override Type GadgetType { get; } = typeof(Ladder);

        [XmlElement(IsNullable = true)] public SoundEffectSet SoundsSet { get; set; }
        [XmlElement(IsNullable = true)] public LadderBase Base { get; set; }
        [XmlElement(IsNullable = true)] public LadderMain Main { get; set; }
        [XmlElement(IsNullable = true)] public LadderExtensions Extensions { get; set; }
        [XmlElement(IsNullable = true)] public LadderBucket Bucket { get; set; }

        [XmlIgnore] public bool HasSoundsSet => SoundsSet != null;
        [XmlIgnore] public bool HasBase => Base != null;
        [XmlIgnore] public bool HasMain => Main != null;
        [XmlIgnore] public bool HasExtensions => Extensions != null;
        [XmlIgnore] public bool HasBucket => Bucket != null;

        public sealed class LadderBase
        {
            public string BoneName { get; set; }
            public float RotationSpeed { get; set; }
            [XmlElement(Type = typeof(XmlVector3))] public Vector3 RotationAxis { get; set; }
        }

        public sealed class LadderMain
        {
            public string BoneName { get; set; }
            public float RotationSpeed { get; set; }
            [XmlElement(Type = typeof(XmlVector3))] public Vector3 RotationAxis { get; set; }
            public float MinAngle { get; set; }
            public float MaxAngle { get; set; }
        }

        public sealed class LadderExtensions
        {
            [XmlElement(Type = typeof(XmlVector3))] public Vector3 Direction { get; set; }
            [XmlArrayItem(ElementName = "Entry")] public LadderExtension[] Parts { get; set; }
        }

        public sealed class LadderExtension
        {
            public string BoneName { get; set; }
            public float MoveSpeed { get; set; }
            public float ExtensionDistance { get; set; }
        }

        public sealed class LadderBucket
        {
            public string BoneName { get; set; }
            public float RotationSpeed { get; set; }
            [XmlElement(Type = typeof(XmlVector3))] public Vector3 RotationAxis { get; set; }
            public float MinAngle { get; set; }
            public float MaxAngle { get; set; }
        }
    }
}
