using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxParserClient
{
    [Serializable]
    public class UserExpression
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
    }
}
