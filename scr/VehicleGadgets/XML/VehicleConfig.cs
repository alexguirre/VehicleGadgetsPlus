namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System.Xml.Serialization;

    using VehicleGadgetsPlus.Conditions.XML;

    public class VehicleConfig
    {
        [XmlArrayItem(IsNullable = true, ElementName = "Condition")] public ConditionEntry[] ExtraConditions { get; set; }
        [XmlArrayItem(ElementName = "Entry")] public VehicleGadgetEntry[] Gadgets { get; set; }
    }
}
