using System;
using Alexander.CharacterSheet.Core;
static class NativeUiFeatureTests
{
    public static void Run(Action<bool,string> assert)
    {
        NativeInventoryPlan small,large,narrow;
        assert(NativeInventoryLayout.TryPlan(360,5,64,4,1,50,out small)&&small.IconWidth==36&&small.Columns==9&&small.Rows==1,
            "Native plan uses smaller icons with available native width");
        assert(small.ViewportHeight==160,"Inventory viewport expands from 50 to four 40-unit rows");
        assert(NativeInventoryLayout.TryPlan(360,120,64,4,1,50,out large)&&large.Rows==14&&large.ContentCeiling>large.ViewportHeight,
            "Long own inventory has content beyond the real viewport for scrolling");
        assert(NativeInventoryLayout.TryPlan(180,120,64,4,1,50,out narrow)&&narrow.Columns<large.Columns&&narrow.Rows>large.Rows,
            "Resolution width change requires native reflow and retains all entries");
        assert(NativeInventoryLayout.TryPlan(360,0,30,4,1,250,out small)&&small.IconWidth==30&&small.ViewportHeight==250,
            "Existing smaller icons and larger footprint are retained for empty inventory");
        assert(!NativeInventoryLayout.TryPlan(double.NaN,120,64,4,1,50,out small)&&
            !NativeInventoryLayout.TryPlan(360,-1,64,4,1,50,out small)&&
            !NativeInventoryLayout.TryPlan(360,5001,64,4,1,50,out small),"Unavailable geometry or invalid count declines native override");
        object wrapper=new object(),nativeParent=new object(),foreignParent=new object();
        assert(NativeInventoryLayout.OwnsParent(wrapper,wrapper)&&!NativeInventoryLayout.OwnsParent(wrapper,foreignParent)&&
            !NativeInventoryLayout.OwnsParent(null,null),"Restoration follows actual owned parent; absent/foreign parent never matches");
        var height=new UiOverrideLease<float>();float original;
        height.Capture(50,large.ContentCeiling>0?(float)large.ContentCeiling:1000);
        float first=(float)large.ContentCeiling;
        assert(height.Release(first,out original)&&original==50,"Growing content ceiling retrieves original native maxHeight");
        height.Capture(original,10000);
        assert(height.Release(10000,out original)&&original==50,"Tab close after inventory growth restores original maxHeight");
        assert(!height.Release(10000,out original),"Repeated Tab/dispose cleanup makes no second restore");
        height.Capture(50,10000);
        assert(!height.Release(7000,out original),"Later distinguishable native or mod maxHeight change is preserved");
        var sessions=new NativeSessionTracker();object user=new object(),hud=new object(),body=new object(),respawn=new object();
        var open=sessions.Reconcile(true,true,user,hud,body,null);
        assert(open.Open&&!open.Close,"First native Tab session requests bindings and viewport once");
        var replacement=sessions.Reconcile(true,true,user,hud,respawn,null);
        assert(replacement.Close&&replacement.Open,"Respawn closes old buff/viewport owner before replacement binds");
        var focusLoss=sessions.Reconcile(false,true,user,hud,respawn,null);
        assert(focusLoss.Close&&!focusLoss.Open,"Focus loss requests cleanup without opening hidden hover targets");
        assert(!sessions.Reconcile(false,true,user,hud,respawn,null).Close,"Repeated ineligible update does not repeat cleanup");
    }
}
