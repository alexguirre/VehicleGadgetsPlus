namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.Xml.Serialization;

    using Rage;

    [XmlType(TypeName = XmlName)]
    public sealed class RotatingPartEntry : VehicleGadgetEntry
    {
        public const string XmlName = "RotatingPart";

        [XmlIgnore] public override Type GadgetType { get; } = typeof(RotatingPart);

        public string BoneName { get; set; }
        public string ActivationConditions { get; set; }
        public float RotationSpeed { get; set; }
        [XmlElement(Type = typeof(XmlVector3))] public Vector3 RotationAxis { get; set; }
    }
}
