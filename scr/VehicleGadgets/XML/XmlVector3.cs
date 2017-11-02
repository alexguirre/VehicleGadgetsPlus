namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System.Xml.Serialization;

    using Rage;

    public sealed class XmlVector3
    {
        [XmlAttribute] public float X { get; set; }
        [XmlAttribute] public float Y { get; set; }
        [XmlAttribute] public float Z { get; set; }

        public static implicit operator Vector3(XmlVector3 value) => new Vector3(value.X, value.Y, value.Z);
        public static implicit operator XmlVector3(Vector3 value) => new XmlVector3 { X = value.X, Y = value.Y, Z = value.Z };
    }
}
