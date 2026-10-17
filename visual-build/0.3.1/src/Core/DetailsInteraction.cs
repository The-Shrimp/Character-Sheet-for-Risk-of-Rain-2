using System;
namespace Alexander.CharacterSheet.Core
{
    public sealed class DetailsInteraction
    {
        object source;
        string key;
        bool sourceInside,popupInside,focused;
        double leaveAt=double.PositiveInfinity;
        public bool Expanded{get;private set;}
        public bool Active=>source!=null;
        public bool PointerInside=>popupInside;
        public string Key=>key;
        public object Source=>source;
        public bool Enter(object nextSource,string nextKey)
        {
            if(nextSource==null||string.IsNullOrEmpty(nextKey)){Reset();return false;}
            bool changed=!ReferenceEquals(source,nextSource)||key!=nextKey;
            if(changed)Reset();
            source=nextSource;key=nextKey;sourceInside=true;leaveAt=double.PositiveInfinity;return changed;
        }
        public void Leave(object token,double now)
        {
            if(!ReferenceEquals(source,token)||!NumberFormatting.IsFinite(now))return;
            sourceInside=false;if(!popupInside)leaveAt=now+.4;
        }
        public void PopupEnter(){if(Active){popupInside=true;leaveAt=double.PositiveInfinity;}}
        public void PopupExit(double now){popupInside=false;if(Active&&!sourceInside)leaveAt=now+.4;}
        public bool Toggle(bool nativeCursor,bool primaryClick)
        {
            if(!Active||!nativeCursor||!primaryClick||!popupInside)return false;
            Expanded=!Expanded;return true;
        }
        public void Focus(bool value,double now)
        {focused=Active&&value;if(focused)leaveAt=double.PositiveInfinity;else if(Active&&!sourceInside&&!popupInside)leaveAt=now+.4;}
        public bool FocusToggle(bool nativeCursor)
        {if(!Active||!focused||!nativeCursor)return false;Expanded=!Expanded;return true;}
        public bool FocusExpand(bool nativeCursor,bool expanded)
        {if(!Active||!focused||!nativeCursor)return false;Expanded=expanded;return true;}
        public bool Expire(double now)
        {
            if(!Active||sourceInside||popupInside||focused||!NumberFormatting.IsFinite(now)||now<=leaveAt)return false;
            Reset();return true;
        }
        public void Reset(){source=null;key=null;sourceInside=popupInside=focused=Expanded=false;leaveAt=double.PositiveInfinity;}
    }
    public sealed class NativePointerExitLease
    {
        object provider,context;
        Action release;
        public bool Pending=>release!=null;
        public bool Defer(object nextProvider,object nextContext,Action callback)
        {
            if(nextProvider==null||nextContext==null||callback==null)return false;
            if(Pending)return ReferenceEquals(provider,nextProvider)&&ReferenceEquals(context,nextContext);
            provider=nextProvider;context=nextContext;release=callback;return true;
        }
        public void Release(bool stillOwnsContext)
        {
            var callback=release;Cancel();
            if(stillOwnsContext)callback?.Invoke();
        }
        public void Cancel(){provider=context=null;release=null;}
    }
}
