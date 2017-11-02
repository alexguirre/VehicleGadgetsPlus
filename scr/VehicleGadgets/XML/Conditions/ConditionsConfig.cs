namespace VehicleGadgetsPlus.VehicleGadgets.XML.Conditions
{
    using System.Xml.Serialization;

    public class ConditionsConfig
    {
        [XmlArrayItem(ElementName = "Condition")]
        public ConditionEntry[] Conditions{ get; set; }
    }
}
