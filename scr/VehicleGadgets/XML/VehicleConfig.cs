namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System.Xml.Serialization;

    public class VehicleConfig
    {
        [XmlArrayItem(ElementName = "Entry")]
        public VehicleGadgetEntry[] Gadgets { get; set; }
    }
}
