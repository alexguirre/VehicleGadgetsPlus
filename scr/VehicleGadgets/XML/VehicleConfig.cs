namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System.Xml.Serialization;

    using VehicleGadgetsPlus.VehicleGadgets.XML.Conditions;

    public class VehicleConfig
    {
        [XmlArrayItem(IsNullable = true, ElementName = "Condition")] public ConditionEntry[] ExtraConditions { get; set; }
        [XmlArrayItem(ElementName = "Entry")] public VehicleGadgetEntry[] Gadgets { get; set; }
    }
}
