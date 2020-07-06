using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MonoBMFont
{
    [Serializable()]
    public class FontKerningPair
    {
        [XmlAttribute("first")]
        public int First { get; set; }

        [XmlAttribute("second")]
        public int Second { get; set; }

        [XmlAttribute("amount")]
        public int Amount { get; set; }
    }
}