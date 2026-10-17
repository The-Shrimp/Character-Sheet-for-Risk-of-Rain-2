using System;
using System.Collections.Generic;
using Alexander.CharacterSheet.Core;
static class NativeTabTests
{
    public static void Run(Action<bool,string> assert)
    {
        var large=new Calculation("Baseline",12500000.125);
        string detail=large.Describe(12500000.625,false,true);
        assert(detail.Contains("12,500,000.125")&&detail.Contains("+0.5"),"Detailed attribution retains full values and small residuals beside millions");
        assert(detail.Contains("\n")&&!detail.Contains("\\n"),"Calculation details contain real lines rather than displayed escape text");
        object own=new object(),remote=new object();
        assert(NativeTabPolicy.OwnVisibleItem(own,own,false,1,1),"Own visible native icon is eligible");
        assert(!NativeTabPolicy.OwnVisibleItem(own,remote,false,1,1),"Remote native icons are excluded");
        assert(!NativeTabPolicy.OwnVisibleItem(null,null,false,1,1),"Unknown owner never matches");
        assert(!NativeTabPolicy.OwnVisibleItem(own,own,true,1,1),"Hidden own item is excluded");
        assert(!NativeTabPolicy.OwnVisibleItem(own,own,false,0,0),"Absent item is excluded");
        assert(NativeTabPolicy.OwnVisibleItem(own,own,false,2,0),"Owned inactive item remains visible");
        assert(NativeTabPolicy.OwnVisibleItem(own,own,false,0,2),"Active temporary own item remains visible");
        assert(!NativeTabPolicy.OwnVisibleItem(own,own,false,-1,2),"Invalid counts are not silently accepted");
        var occupied=new[]{new UiRect(200,150,600,500),new UiRect(200,650,600,100)};
        UiRect dock;
        assert(NativeTabPolicy.TryDock(1200,900,occupied,300,500,out dock),"Room outside native strips supports a dock");
        assert(dock.Valid&&dock.X>=0&&dock.Y>=0&&dock.Right<=1200&&dock.Top<=900,"Dock remains inside canvas");
        assert(!dock.Overlaps(occupied[0])&&!dock.Overlaps(occupied[1]),"Dock never covers any native player strip");
        assert(!NativeTabPolicy.TryDock(800,600,new[]{new UiRect(0,0,800,600)},300,400,out dock),"Crowded canvas preserves native panels and declines addition");
        assert(!NativeTabPolicy.TryDock(800,600,new UiRect[0],300,400,out dock),"Native layout not ready has no full-screen fallback");
        assert(!NativeTabPolicy.TryDock(double.NaN,600,occupied,300,400,out dock),"Nonfinite geometry cannot position UI");
        assert(!NativeTabPolicy.TryDock(800,600,new[]{new UiRect(0,0,-1,10)},300,400,out dock),"Invalid occupied geometry is rejected");
        assert(NativeTabPolicy.TryDock(1200,900,new[]{new UiRect(-50,100,800,500)},300,400,out dock)&&dock.X>=762,"Offscreen native geometry is clipped conservatively");
        var overrideLease=new UiOverrideLease<bool>();overrideLease.Capture(true,false);bool restore;
        assert(overrideLease.Release(false,out restore)&&restore,"Cleanup restores a still-owned UI value");
        assert(!overrideLease.Release(false,out restore),"Repeated cleanup makes no second write");
        overrideLease.Capture(true,false);
        assert(!overrideLease.Release(true,out restore),"Cleanup preserves a later distinguishable external UI change");
        var textLease=new UiOverrideLease<string>();textLease.Capture("native","ours");string text;
        assert(!textLease.Release("another mod",out text),"Foreign tooltip override is not overwritten");
        var lease=new PauseLease();var requests=new List<bool>();
        lease.OpenAfterNative(true,false,true,own,requests.Add);
        assert(lease.Ownership==PauseOwnership.Owned&&requests.Count==0,"Native opening pause is adopted without a duplicate request");
        assert(!lease.AcknowledgeNativeClose(remote)&&lease.IsOpen,"Wrong controller cannot acknowledge native release");
        assert(lease.AcknowledgeNativeClose(own)&&!lease.IsOpen,"Native close consumes only owned pause");
        lease.Close(true,own,requests.Add);
        assert(requests.Count==0,"Cleanup after native release does not request another unpause");
        lease.OpenAfterNative(true,true,true,own,requests.Add);
        assert(lease.PreserveExternalPause&&!lease.AcknowledgeNativeClose(own),"Preexisting pause remains borrowed");
        lease.Close(true,own,requests.Add);assert(requests.Count==0,"Closing borrowed native sheet keeps external pause");
        lease.OpenAfterNative(true,false,false,own,requests.Add);
        assert(lease.Ownership==PauseOwnership.Pending&&requests.Count==1&&requests[0],"Fallback pause uses one native request");
        lease.Close(true,own,requests.Add);assert(requests.Count==2&&!requests[1],"Fallback acquired pause releases once");
        requests.Clear();lease.OpenAfterNative(false,false,true,own,requests.Add);
        assert(!lease.IsOpen&&requests.Count==0,"Multiplayer never adopts or requests pause");
        lease.OpenAfterNative(true,false,true,own,requests.Add);lease.ExternalPauseRequest();
        assert(lease.PreserveExternalPause&&!lease.AcknowledgeNativeClose(own),"External takeover blocks owned-close acknowledgement");
        lease.Close(true,own,requests.Add);assert(requests.Count==0,"External takeover survives cleanup");
    }
}
