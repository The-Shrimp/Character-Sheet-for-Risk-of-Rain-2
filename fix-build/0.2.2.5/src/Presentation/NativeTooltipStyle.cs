using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    // Public component fields and public TMP properties, read only at runtime.
    // No native tooltip is created, modified, reparented, or made visible.
    internal sealed class NativeTooltipStyle
    {
        TMP_Text nativeBody,nativeTitle;
        TMP_StyleSheet styleSheet;
        float nextCapture;
        internal float PreferredWidth=420;
        internal string Status{get;private set;}="Native tooltip component not yet observed; native TMP defaults retained.";
        internal bool CanStyle(string name)=>styleSheet&&styleSheet.GetStyle(name)!=null;
        internal void Refresh(TextMeshProUGUI title,TextMeshProUGUI body,Image panel,Image heading,float scale,TooltipController exactController=null)
        {
            if(exactController||Time.unscaledTime>=nextCapture)
            {
                nextCapture=Time.unscaledTime+2;
                var controller=exactController?exactController:Resources.FindObjectsOfTypeAll<TooltipController>()
                    .Where(c=>c&&c.bodyLabel&&c.bodyLabel.font&&c.titleLabel&&c.titleLabel.font)
                    .OrderByDescending(c=>c.gameObject.activeInHierarchy)
                    .ThenByDescending(c=>c.gameObject.scene.IsValid()).FirstOrDefault();
                if(controller)
                {
                    nativeBody=controller.bodyLabel;nativeTitle=controller.titleLabel;
                    var rect=controller.transform as RectTransform;
                    if(rect&&rect.rect.width>=200&&rect.rect.width<=800)PreferredWidth=rect.rect.width;
                    var background=FindBodyFill(controller);
                    var headerImage=nativeTitle.GetComponentsInParent<Image>(true)
                        .FirstOrDefault(i=>i&&i!=background&&(i.transform==controller.transform||i.transform.IsChildOf(controller.transform)));
                    CopyImage(background,panel);CopyImage(headerImage,heading);
                    Status=background?"Native tooltip text styles and body-covering background fill observed.":
                        "Native tooltip text styles observed; native body fill not yet available.";
                }
            }
            if(nativeBody)CopyText(nativeBody,body,scale,14,30);
            if(nativeTitle)CopyText(nativeTitle,title,scale,14,36);
            styleSheet=nativeBody&&nativeBody.styleSheet?nativeBody.styleSheet:TMP_Settings.defaultStyleSheet;
            body.styleSheet=styleSheet;body.richText=true;title.richText=false;
        }
        static Image FindBodyFill(TooltipController controller)
        {
            var bodyBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(controller.transform,controller.bodyLabel.rectTransform);
            var images=controller.GetComponentsInChildren<Image>(true).Where(i=>i&&i.enabled).ToArray();
            var candidates=images.Select(i=>{
                var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(controller.transform,i.rectTransform);
                string name=i.name.ToLowerInvariant();
                bool named=(name.Contains("background")||name.Contains("backdrop")||name.Contains("body")||name.Contains("panel"))&&
                    !name.Contains("border")&&!name.Contains("outline")&&!name.Contains("shadow")&&!name.Contains("glow");
                bool ancestor=controller.bodyLabel.transform==i.transform||controller.bodyLabel.transform.IsChildOf(i.transform);
                bool covers=bounds.size.x>0&&bounds.size.y>0&&bounds.min.x<=bodyBounds.center.x&&bounds.max.x>=bodyBounds.center.x&&
                    bounds.min.y<=bodyBounds.center.y&&bounds.max.y>=bodyBounds.center.y&&
                    bounds.size.x>=bodyBounds.size.x*.8f&&bounds.size.y>=bodyBounds.size.y*.6f;
                // A native prefab's empty body can have no laid-out bounds yet.
                if(bodyBounds.size.x<=0||bodyBounds.size.y<=0)covers=named||ancestor;
                return new NativeTooltipFillCandidate(covers,ancestor,named,i.color.a,bounds.size.x*bounds.size.y);
            }).ToArray();
            int selected=NativeTooltipFillPolicy.Select(candidates);
            return selected<0?null:images[selected];
        }
        static void CopyText(TMP_Text source,TMP_Text target,float scale,float min,float max)
        {
            target.font=source.font;target.fontSharedMaterial=source.fontSharedMaterial;
            target.color=source.color;target.fontStyle=source.fontStyle;target.styleSheet=source.styleSheet;
            target.fontSize=Mathf.Clamp(source.fontSize,min,max)*scale;
            target.characterSpacing=source.characterSpacing;target.wordSpacing=source.wordSpacing;target.lineSpacing=source.lineSpacing;
        }
        static void CopyImage(Image source,Image target)
        {
            if(!source||!target)return;
            target.sprite=source.sprite;target.type=source.type;target.color=source.color;target.material=source.material;
        }
    }
}
