namespace VehicleGadgetsPlus.Conditions.XML
{
    using System.Xml.Serialization;

    public class ConditionsConfig
    {
        [XmlArrayItem(ElementName = "Condition")]
        public ConditionEntry[] Conditions{ get; set; }
    }
}
