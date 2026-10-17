using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    // Wrap the actual native display. No copied icons, item order, stacks or subscribed inventory are written.
    internal sealed class NativeInventoryViewport:IDisposable
    {
        readonly ItemInventoryDisplay display;
        readonly RectTransform native;
        readonly Transform originalParent;
        readonly int originalSibling;
        readonly Vector2 originalMin,originalMax,originalPivot,originalSize,originalPosition;
        readonly Vector3 originalScale;
        readonly Quaternion originalRotation;
        readonly UiOverrideLease<float> iconLease=new UiOverrideLease<float>(),heightLease=new UiOverrideLease<float>();
        RectTransform viewport;
        ScrollRect scroll;
        RectTransform scrollbarRoot;
        LayoutElement footprint;
        float appliedIcon,appliedHeight;
        int lastCount=-1;
        float lastWidth=-1;
        bool disposed;
        GraphicRaycaster raycaster;
        internal ItemInventoryDisplay Display=>display;
        internal bool Attached=>!disposed&&native&&viewport&&native.parent==viewport;
        internal string Status{get;private set;}
        internal NativeInventoryViewport(ItemInventoryDisplay source,float scale,NativeUiStyle style)
        {
            display=source;native=source.transform as RectTransform;
            if(!native||!native.parent)throw new InvalidOperationException("Native inventory RectTransform is not ready.");
            originalParent=native.parent;originalSibling=native.GetSiblingIndex();
            originalMin=native.anchorMin;originalMax=native.anchorMax;originalPivot=native.pivot;
            originalSize=native.sizeDelta;originalPosition=native.anchoredPosition;
            originalScale=native.localScale;originalRotation=native.localRotation;
            NativeInventoryPlan plan;
            if(!NativeInventoryLayout.TryPlan(native.rect.width,display.GetTotalVisibleItemCount(),display.maxIconWidth,display.verticalMargin,
                scale,native.rect.height,out plan))throw new InvalidOperationException("Native inventory geometry/count is unavailable.");
            try
            {
                viewport=NativeUiStyle.Rect(originalParent,"AlexanderNativeInventoryViewport",originalMin,originalMax);
                viewport.pivot=originalPivot;viewport.sizeDelta=originalSize;viewport.anchoredPosition=originalPosition;
                viewport.localScale=originalScale;viewport.localRotation=originalRotation;viewport.SetSiblingIndex(originalSibling);
                footprint=viewport.gameObject.AddComponent<LayoutElement>();
                footprint.minWidth=display.minWidth;footprint.preferredWidth=display.preferredWidth;footprint.flexibleWidth=display.flexibleWidth;
                footprint.minHeight=(float)plan.ViewportHeight;footprint.preferredHeight=(float)plan.ViewportHeight;footprint.flexibleHeight=0;
                footprint.layoutPriority=display.layoutPriority;
                viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(float)plan.ViewportHeight);
                viewport.gameObject.AddComponent<Canvas>().overrideSorting=false;raycaster=viewport.gameObject.AddComponent<GraphicRaycaster>();
                var backdrop=viewport.gameObject.AddComponent<Image>();backdrop.color=Color.clear;backdrop.raycastTarget=true;
                viewport.gameObject.AddComponent<RectMask2D>();
                scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=native;
                scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
                scroll.scrollSensitivity=28*scale;scroll.inertia=false;
                var barRoot=NativeUiStyle.Rect(viewport,"ScrollBar",new Vector2(1,0),Vector2.one);
                scrollbarRoot=barRoot;barRoot.pivot=Vector2.one;barRoot.sizeDelta=new Vector2(10,0);barRoot.anchoredPosition=Vector2.zero;
                style.Background(barRoot,true);
                var handle=NativeUiStyle.Rect(barRoot,"Handle",Vector2.zero,Vector2.one);
                var image=handle.gameObject.AddComponent<Image>();image.color=new Color(.7f,.75f,.8f,.8f);
                var bar=barRoot.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=image;
                bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;
                scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent;
                native.SetParent(viewport,false);native.anchorMin=new Vector2(0,1);native.anchorMax=Vector2.one;
                native.pivot=new Vector2(.5f,1);native.localScale=Vector3.one;native.localRotation=Quaternion.identity;
                native.anchoredPosition=new Vector2(-7,0);native.sizeDelta=new Vector2(-14,(float)plan.ContentCeiling);
                appliedIcon=(float)plan.IconWidth;appliedHeight=(float)plan.ContentCeiling;
                iconLease.Capture(display.maxIconWidth,appliedIcon);heightLease.Capture(display.maxHeight,appliedHeight);
                display.maxIconWidth=appliedIcon;display.maxHeight=appliedHeight;
                if(!Refresh(scale))throw new InvalidOperationException(Status);scroll.verticalNormalizedPosition=1;
            }
            catch{Dispose();throw;}
        }
        internal bool Refresh(float scale)
        {
            if(!Attached||native.anchorMin!=new Vector2(0,1)||native.anchorMax!=Vector2.one||native.pivot!=new Vector2(.5f,1)||
                native.localScale!=Vector3.one||native.localRotation!=Quaternion.identity){Status="Native inventory layout ownership changed; viewport released.";return false;}
            if(display.maxIconWidth!=appliedIcon||display.maxHeight!=appliedHeight)
            {Status="Another UI writer changed native inventory geometry; viewport released.";return false;}
            int count=display.GetTotalVisibleItemCount();float width=viewport.rect.width;
            NativeInventoryPlan plan;
            if(!NativeInventoryLayout.TryPlan(width,count,appliedIcon,display.verticalMargin,scale,footprint.preferredHeight,out plan))
            {Status="Native inventory geometry/count became unavailable; viewport released.";return false;}
            if(count!=lastCount||Math.Abs(width-lastWidth)>.5f)
            {
                // Grow the conservative native layout ceiling as visible item count grows.
                // Updating the lease records its latest owned value and retains the original value for restoration.
                float desired=(float)Math.Max(plan.ContentCeiling,appliedHeight);
                if(desired!=appliedHeight)
                {
                    float original;bool owned=heightLease.Release(display.maxHeight,out original);
                    if(!owned)return false;
                    heightLease.Capture(original,desired);appliedHeight=desired;display.maxHeight=desired;
                }
                native.sizeDelta=new Vector2(-14,appliedHeight);
                display.UpdateDisplay();display.CalculateLayoutInputHorizontal();display.CalculateLayoutInputVertical();
                float height=display.preferredHeight;
                if(!NumberFormatting.IsFinite(height)||height<0||height==0&&count>0){Status="Native preferred content height is unavailable.";return false;}
                native.sizeDelta=new Vector2(-14,Mathf.Max(viewport.rect.height,height));
                lastCount=count;lastWidth=width;LayoutRebuilder.MarkLayoutForRebuild(viewport);
            }
            Status="Own native inventory wrapped: smaller icons, native reflow, four-row minimum viewport and vertical scroll; runtime unvalidated.";
            return true;
        }
        internal void SetCursorContext(bool open)
        {
            if(scroll)scroll.enabled=open&&Attached;
            if(raycaster)raycaster.enabled=open&&Attached;
        }
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            try
            {
            if(scroll)scroll.enabled=false;
            if(raycaster)raycaster.enabled=false;
            float restore;
            if(display)
            {
                if(iconLease.Release(display.maxIconWidth,out restore))display.maxIconWidth=restore;
                if(heightLease.Release(display.maxHeight,out restore))display.maxHeight=restore;
            }
            if(native&&viewport&&NativeInventoryLayout.OwnsParent(viewport,native.parent))
            {
                native.SetParent(originalParent?originalParent:null,false);
                if(originalParent)native.SetSiblingIndex(originalSibling);
                if(native.anchorMin==new Vector2(0,1))native.anchorMin=originalMin;
                if(native.anchorMax==Vector2.one)native.anchorMax=originalMax;
                if(native.pivot==new Vector2(.5f,1))native.pivot=originalPivot;
                // ScrollRect and native layout drive these two fields while the parent is ours.
                native.sizeDelta=originalSize;native.anchoredPosition=originalPosition;
                if(native.localScale==Vector3.one)native.localScale=originalScale;
                if(native.localRotation==Quaternion.identity)native.localRotation=originalRotation;
                if(originalParent is RectTransform parent)LayoutRebuilder.MarkLayoutForRebuild(parent);
                if(display&&display.isActiveAndEnabled)display.UpdateDisplay();
            }
            }
            finally
            {
            if(viewport)
            {
                // Do not destroy a native/foreign child inserted by a later writer.
                foreach(Transform child in viewport.Cast<Transform>().ToArray())
                    if(child!=scrollbarRoot)child.SetParent(originalParent?originalParent:null,true);
                viewport.gameObject.SetActive(false);UnityEngine.Object.Destroy(viewport.gameObject);viewport=null;
            }
            }
        }
    }
}
