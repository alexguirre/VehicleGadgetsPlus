namespace VehicleGadgetsPlus.VehicleGadgets.XML.Conditions
{
    using System.Xml;
    using System.Xml.Serialization;

    [XmlType(TypeName = XmlName)]
    public sealed class ConditionEntry
    {
        public const string XmlName = "Condition";
        
        public string Name { get; set; }
        [XmlIgnore] public string Code { get; set; }
        [XmlElement("Code")]
        public XmlCDataSection CodeCDataSection
        {
            get
            {
                return new XmlDocument().CreateCDataSection(Code);
            }
            set
            {
                Code = value.Value;
            }
        }
    }
}
