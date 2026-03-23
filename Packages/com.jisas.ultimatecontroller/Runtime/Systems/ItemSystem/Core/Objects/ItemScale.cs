using UltimateFramework.Utils;
using System;

namespace UltimateFramework.ItemSystem
{
    [Serializable]
    public class ItemScale
    {
        public ScalingLevel startScale;
        public string attributeTag;
        public ScaleMathOperation operation;
        private ScalingLevel currentScale;
        public int Index { get; set; }
        public ScalingLevel CurrentScaleLevel { get => this.currentScale; }
        public void SetCurrentScale(ScalingLevel newScale)
        {
            this.currentScale = newScale;
        }
    }
}
