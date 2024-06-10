using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Survival.HxPly
{
    public struct Header
    {
        public Format Format;
        public Dictionary<ElementType, Element> ElementDictionary;
        public List<Element> ElementList;
    }
}
