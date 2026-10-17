using System;
using Alexander.CharacterSheet.Core;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    // HUD.Update selects its last direct child. Every object owned here stays inside one existing branch.
    internal sealed class NativeSheetLayer:IDisposable
    {
        internal readonly RectTransform Root;
        internal readonly Canvas Canvas;
        NativeSheetLayer(RectTransform parent,Canvas canvas)
        {
            Canvas=canvas;
            Root=NativeUiStyle.Rect(parent,"AlexanderSheetLayer",Vector2.zero,Vector2.one);
            Root.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;
        }
        internal static bool TryCreate(HUD hud,out NativeSheetLayer layer)
        {
            layer=null;
            if(!hud||!hud.scoreboardPanel)return false;
            var branch=hud.scoreboardPanel.transform;
            while(branch&&branch.parent&&branch.parent!=hud.transform)branch=branch.parent;
            var parent=branch as RectTransform;
            var canvas=hud.GetComponent<Canvas>();
            bool belowHud=parent&&parent.parent==hud.transform;
            bool contains=parent&&(hud.scoreboardPanel.transform==parent||hud.scoreboardPanel.transform.IsChildOf(parent));
            if(!canvas||!NativeHudPlacementPolicy.CanAttach(belowHud?1:0,parent&&parent.gameObject.activeInHierarchy,contains))return false;
            int directChildren=hud.transform.childCount;
            var candidate=new NativeSheetLayer(parent,canvas);
            if(hud.transform.childCount!=directChildren){candidate.Dispose();return false;}
            layer=candidate;return true;
        }
        internal void SetVisible(bool visible){if(Root)Root.gameObject.SetActive(visible);}
        public void Dispose()
        {
            if(!Root)return;
            Root.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(Root.gameObject);
        }
    }
}
