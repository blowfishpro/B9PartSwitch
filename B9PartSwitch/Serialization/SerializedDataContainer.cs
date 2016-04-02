using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

namespace B9PartSwitch
{
    [Serializable]
    public class SerializedDataContainer : ScriptableObject
    {
        public string data = null;
    }
}
