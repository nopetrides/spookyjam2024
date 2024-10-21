using Murder.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Assets
{
    public class DoorAsset : GameAsset
    {
        public override char Icon => 'D';
        public override string EditorFolder => "#EDoors";
        public override Vector4 EditorColor => new Vector4(0f,1f,1f,1f);
    }
}
