namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.Xml.Serialization;

    [XmlInclude(typeof(LadderEntry))]
    [XmlInclude(typeof(OutriggersEntry))]
    [XmlInclude(typeof(RotatingPartEntry))]
    [XmlInclude(typeof(HideablePartEntry))]
    public abstract class VehicleGadgetEntry
    {
        [XmlIgnore]
        public abstract Type GadgetType { get; }
    }
}
