using System.Collections.Generic;
using UnityEditor;

namespace UltimateFramework.Editor
{
    public class PopupDrawer
    {
        private int selectedIndex = 0;
        private List<string> options = new List<string>();

        public PopupDrawer(List<string> options)
        {
            this.options = options;
        }

        public int DrawPopup()
        {
            selectedIndex = EditorGUILayout.Popup(selectedIndex, options.ToArray());
            return selectedIndex;
        }
    }
}


