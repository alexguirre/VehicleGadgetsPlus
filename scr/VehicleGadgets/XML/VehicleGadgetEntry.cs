namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    using Rage;

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
        public LadderSoundsSet SoundsSet { get; set; }
        [XmlElement(IsNullable = true)]
        public LadderBase Base { get; set; }
        [XmlElement(IsNullable = true)]
        public LadderMain Main { get; set; }
        [XmlElement(IsNullable = true)]
        public LadderExtensions Extensions { get; set; }
        [XmlElement(IsNullable = true)]
        public LadderBucket Bucket { get; set; }

        [XmlIgnore]
        public bool HasSoundsSet => SoundsSet != null;
        [XmlIgnore]
        public bool HasBase => Base != null;
        [XmlIgnore]
        public bool HasMain => Main != null;
        [XmlIgnore]
        public bool HasExtensions => Extensions != null;
        [XmlIgnore]
        public bool HasBucket => Bucket != null;
        
        public sealed class LadderSoundsSet
        {
            public static readonly string Default = "default";
            
            // 0 - 100
            public int Volume { get; set; }
            // "default" or filename with extension from "Vehicle Gadgets+\Sounds\"
            public string Loop { get; set; }
            // "default" or filename with extension from "Vehicle Gadgets+\Sounds\"
            public string End { get; set; }

            [XmlIgnore]
            public float NormalizedVolume => MathHelper.Clamp(Volume, 0, 100) / 100.0f;

            [XmlIgnore]
            public bool IsDefaultLoop => Loop.Equals(Default, StringComparison.InvariantCultureIgnoreCase);
            [XmlIgnore]
            public bool IsDefaultEnd => End.Equals(Default, StringComparison.InvariantCultureIgnoreCase);

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

        public sealed class LadderExtensions
        {
            public XYZ Direction { get; set; }
            [XmlArrayItem(ElementName = "Entry")]
            public LadderExtension[] Parts { get; set; }
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

        [XmlElement(IsNullable = true)]
        public OutriggersSoundsSet SoundsSet { get; set; }
        public Outrigger[] Outriggers { get; set; }

        [XmlIgnore]
        public bool HasSoundsSet => SoundsSet != null;

        public sealed class Outrigger
        {
            public string ExtensionBoneName { get; set; }
            public XYZ ExtensionDirection { get; set; }
            public float ExtensionDistance { get; set; }
            public float ExtensionMoveSpeed { get; set; }

            public string SupportBoneName { get; set; }
            public XYZ SupportDirection { get; set; }
            public float SupportDistance { get; set; }
            public float SupportMoveSpeed { get; set; }
        }
        
        public sealed class OutriggersSoundsSet
        {
            public static readonly string Default = "default";

            // 0 - 100
            public int Volume { get; set; }
            // "default" or filename with extension from "Vehicle Gadgets+\Sounds\"
            public string Loop { get; set; }

            [XmlIgnore]
            public float NormalizedVolume => MathHelper.Clamp(Volume, 0, 100) / 100.0f;

            [XmlIgnore]
            public bool IsDefaultLoop => Loop.Equals(Default, StringComparison.InvariantCultureIgnoreCase);

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
