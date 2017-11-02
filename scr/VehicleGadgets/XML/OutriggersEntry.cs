namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.Xml.Serialization;

    using Rage;

    [XmlType(TypeName = XmlName)]
    public sealed class OutriggersEntry : VehicleGadgetEntry
    {
        public const string XmlName = "Outriggers";

        [XmlIgnore] public override Type GadgetType { get; } = typeof(Outriggers);

        [XmlElement(IsNullable = true)] public SoundEffectSet SoundsSet { get; set; }
        public Outrigger[] Outriggers { get; set; }

        [XmlIgnore] public bool HasSoundsSet => SoundsSet != null;

        public sealed class Outrigger
        {
            public string ExtensionBoneName { get; set; }
            [XmlElement(Type = typeof(XmlVector3))] public Vector3 ExtensionDirection { get; set; }
            public float ExtensionDistance { get; set; }
            public float ExtensionMoveSpeed { get; set; }

            public string SupportBoneName { get; set; }
            [XmlElement(Type = typeof(XmlVector3))] public Vector3 SupportDirection { get; set; }
            public float SupportDistance { get; set; }
            public float SupportMoveSpeed { get; set; }
        }
    }
}
