using UnityEngine;

namespace Quantum
{
    public unsafe partial class PlayerPartsData : AssetObject
    {
        public string partName;//名字

        public Sprite partSprite;//图标

        //public AssetRef<EntityPrototype> partPrototype;//prototype

        [ReadOnly]　public PlayerPartsType partType;//类型

        [TextArea]　public string partDescription;//介绍

    }
}