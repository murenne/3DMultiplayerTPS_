using System.Collections.Generic;
using UnityEngine;

namespace Quantum.TPSroject
{
    public unsafe class RuntimePartsDataConfig : AssetObject
    {
        [Header("Body")]
        public List<PlayerPartsData_Body> bodysInventory = new List<PlayerPartsData_Body>();

        [Header("Gun")]
        public List<PlayerPartsData_Gun> gunsInventory = new List<PlayerPartsData_Gun>();
    }
}
