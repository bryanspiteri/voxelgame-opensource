﻿using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MonoBMFont
{
    [Serializable()]
    public class FontTexture
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("file")]
        public string File { get; set; }
    }
}