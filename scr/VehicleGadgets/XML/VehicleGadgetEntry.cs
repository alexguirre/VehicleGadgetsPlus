namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.Xml.Serialization;

    [XmlInclude(typeof(LadderEntry))]
    [XmlInclude(typeof(OutriggersEntry))]
    [XmlInclude(typeof(RotatingPartEntry))]
    [XmlInclude(typeof(ToggleablePartEntry))]
    public abstract class VehicleGadgetEntry
    {
        [XmlIgnore]
        public abstract Type GadgetType { get; }
    }


    [XmlType(TypeName = "Ladder")]
    public sealed class LadderEntry : VehicleGadgetEntry
    {
        [XmlIgnore]
        public override Type GadgetType { get; } = typeof(Ladder);

        [XmlElement(IsNullable = true)]
        public LadderBase Base { get; set; }
        [XmlElement(IsNullable = true)]
        public LadderMain Main { get; set; }
        [XmlArray(ElementName = "ExtensionParts", IsNullable = true)]
        [XmlArrayItem(ElementName = "Entry")]
        public LadderExtension[] Extensions { get; set; }
        [XmlElement(IsNullable = true)]
        public LadderBucket Bucket { get; set; }

        [XmlIgnore]
        public bool HasBase => Base != null;
        [XmlIgnore]
        public bool HasMain => Main != null;
        [XmlIgnore]
        public bool HasExtensions => Extensions != null && Extensions.Length > 0;
        [XmlIgnore]
        public bool HasBucket => Bucket != null;

        public sealed class LadderBase
        {
            public string BoneName { get; set; }
            public float RotationSpeed { get; set; }
            public XYZ RotationAxis { get; set; }
        }

        public sealed class LadderMain
        {
            public string BoneName { get; set; }
            public float RotationSpeed { get; set; }
            public XYZ RotationAxis { get; set; }
            public float MinAngle { get; set; }
            public float MaxAngle { get; set; }
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
            public XYZ RotationAxis { get; set; }
            public float MinAngle { get; set; }
            public float MaxAngle { get; set; }
        }
    }


    [XmlType(TypeName = "Outriggers")]
    public sealed class OutriggersEntry : VehicleGadgetEntry
    {
        [XmlIgnore]
        public override Type GadgetType { get; } = typeof(Outriggers);

        public Outrigger[] LeftOutriggers { get; set; }
        public Outrigger[] RightOutriggers { get; set; }

        public sealed class Outrigger
        {
            public string HorizontalBoneName { get; set; }
            public string VerticalBoneName { get; set; }

            public float HorizontalDistance { get; set; }
            public float HorizontalMoveSpeed { get; set; }
            public float VerticalDistance { get; set; }
            public float VerticalMoveSpeed { get; set; }
        }
    }


    [XmlType(TypeName = "RotatingPart")]
    public sealed class RotatingPartEntry : VehicleGadgetEntry
    {
        [XmlIgnore]
        public override Type GadgetType { get; } = typeof(RotatingPart);

        public string BoneName { get; set; }
        public string ActivationConditions { get; set; }
        public float RotationSpeed { get; set; }
        public XYZ RotationAxis { get; set; }
    }


    [XmlType(TypeName = "ToggleablePart")]
    public sealed class ToggleablePartEntry : VehicleGadgetEntry
    {
        [XmlIgnore]
        public override Type GadgetType { get; } = typeof(ToggleablePart);

        public string BoneName { get; set; }
        public string ToggleConditions { get; set; }
    }
}
