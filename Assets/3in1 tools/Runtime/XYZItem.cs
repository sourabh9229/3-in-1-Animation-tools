using UnityEngine;

namespace SVassets.AnimationCrafter
{
    public class XYZCurves
    {
        public AnimationCurve x = new();
        public AnimationCurve y = new();
        public AnimationCurve z = new();

        public bool IsNull()
        {
            return x == null || y == null || z == null;
        }

        public bool HaveNoKeyframes()
        {
            return x.length == 0 || y.length == 0 || z.length == 0;
        }

        public void Clear()
        {
            x = null;
            y = null;
            z = null;
        }
    }
}
