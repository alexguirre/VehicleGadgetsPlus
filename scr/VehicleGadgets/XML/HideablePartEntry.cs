namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.Xml.Serialization;
    
    [XmlType(TypeName = XmlName)]
    public sealed class HideablePartEntry : VehicleGadgetEntry
    {
        public const string XmlName = nameof(HideablePart);

        [XmlIgnore] public override Type GadgetType { get; } = typeof(HideablePartEntry);

        public string BoneName { get; set; }
        public string ToggleConditions { get; set; }
    }
}
