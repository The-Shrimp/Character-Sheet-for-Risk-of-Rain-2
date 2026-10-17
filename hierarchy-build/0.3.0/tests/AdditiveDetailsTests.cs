using System;
using System.Linq;
using Alexander.CharacterSheet.Core;
static class AdditiveDetailsTests
{
    public static void Run(Action<bool,string> assert)
    {
        object first=new object(),second=new object(),context=new object();
        var details=new DetailsInteraction();
        assert(!details.Active&&!details.Expanded,"Closed Tab has no active or expanded detail state");
        assert(details.Enter(first,"item:1")&&details.Active&&!details.Expanded,"First native hover starts collapsed");
        assert(!details.Toggle(true,true),"Details cannot toggle before pointer enters the owned control");
        details.Leave(first,10);assert(!details.Expire(10.2),"Transfer gap retains the source during the grace window");
        details.PopupEnter();assert(!details.Expire(20),"Pointer over owned add-on keeps its source reachable");
        assert(!details.Toggle(false,true)&&!details.Expanded,"Absent native cursor context cannot open Details");
        assert(!details.Toggle(true,false)&&!details.Expanded,"Secondary click cannot activate Details");
        assert(details.Toggle(true,true)&&details.Expanded,"Primary native-UI click opens Details");
        assert(!details.Enter(first,"item:1")&&details.Expanded,"Repeated native callback on same item retains the toggle");
        assert(details.Toggle(true,true)&&!details.Expanded,"Second click closes Details");
        details.Toggle(true,true);details.Enter(second,"item:2");
        assert(!details.Expanded&&!details.PointerInside,"Item change resets expansion and pointer containment");
        details.PopupEnter();details.Toggle(true,true);
        assert(details.Enter(second,"item:3")&&!details.Expanded,"A reused source with a different identity starts collapsed");
        details.Leave(first,30);assert(!details.Expire(100),"Exit from obsolete source cannot close current source");
        details.Leave(second,30);details.PopupEnter();details.PopupExit(31);
        assert(!details.Expire(31.2)&&details.Expire(31.5)&&!details.Active,"Leaving both source and add-on expires exactly after transfer grace");
        details.Enter(first,"stat:Health");details.PopupEnter();details.Toggle(true,true);details.Reset();details.Reset();
        assert(!details.Active&&!details.Expanded&&!details.PointerInside,"Repeated Tab/scene cleanup clears click and hover state");
        details.Enter(first,"stat:Health");
        assert(!details.Expanded,"Reopening Tab starts collapsed with no stale keyboard/toggle state");

        var lease=new NativePointerExitLease();int exits=0;
        assert(!lease.Defer(null,context,()=>exits++),"Invalid native provider cannot acquire an exit lease");
        assert(lease.Defer(first,context,()=>exits++),"Actual own UI exit can be deferred");
        assert(lease.Defer(first,context,()=>exits+=100),"Duplicate same-provider exit does not replace the original callback");
        assert(!lease.Defer(second,context,()=>exits+=1000),"Another provider cannot take the pending lease");
        lease.Release(true);lease.Release(true);
        assert(exits==1&&!lease.Pending,"Transferred native exit releases at most once");
        lease.Defer(first,context,()=>exits++);lease.Cancel();lease.Release(true);
        assert(exits==1&&!lease.Pending,"Reentering the same source cancels deferred exit rather than hiding its new hover");
        lease.Defer(first,context,()=>exits++);lease.Release(false);
        assert(exits==1&&!lease.Pending,"Lost/replaced native context cannot hide another provider's tooltip");
        lease.Defer(first,context,()=>{exits++;lease.Release(true);});lease.Release(true);
        assert(exits==2&&!lease.Pending,"Native exit callback cannot recursively release the same lease");
        lease.Defer(first,context,()=>{throw new Exception("Synthetic native UI failure");});
        bool threw=false;try{lease.Release(true);}catch(Exception){threw=true;}
        assert(threw&&!lease.Pending,"Native failure still clears the owned lease before callback invocation");

        for(int tab=0;tab<24;tab++)
        {
            details.Reset();lease.Cancel();int released=0;
            for(int item=0;item<16;item++)
            {
                var source=new object();details.Enter(source,"item:"+item);
                assert(!details.Expanded,"Every new item starts collapsed across repeated Tab sessions");
                details.Leave(source,item);lease.Defer(source,context,()=>released++);
                for(int frame=0;frame<12;frame++)assert(!details.Expire(item+frame*.015),"Transfer remains open while pointer is crossing the bounded gap");
                details.PopupEnter();assert(details.Toggle(true,true)&&details.Expanded,"Click works after each actual pointer transfer");
                details.PopupExit(item+1);assert(details.Expire(item+1.5),"Leave closes each item detail");
                lease.Release(true);lease.Release(true);
            }
            details.Reset();lease.Release(true);
            assert(released==16&&!details.Active&&!lease.Pending,"Close/reopen and cleanup release exactly the native exits acquired");
        }

        int layouts=0;
        foreach(var resolution in new[]{new[]{1280d,720d},new[]{1920d,1080d},new[]{3440d,1440d}})
        foreach(double scale in new[]{.7,.85,1,1.2})
        foreach(double desired in new[]{42d,220d,580d})
        {
            var native=new UiRect(resolution[0]*.3,resolution[1]*.6,420*scale,120*scale);UiRect added;
            bool placed=NativeTooltipAddonPlacement.TryBelow(resolution[0],resolution[1],native,desired*scale,42*scale,scale,out added);
            assert(placed&&added.Y>=12*scale&&added.X>=12*scale&&added.X+added.Width<=resolution[0]-12*scale,"Owned add-on stays inside the canvas");
            assert(added.Y+added.Height<=native.Y-4*scale+1e-6,"Owned add-on does not cover the original native tooltip");
            assert(added.Height<=desired*scale&&native.Y==resolution[1]*.6,"Bounding changes only owned height, never native geometry");
            layouts++;
        }
        UiRect invalid;
        assert(!NativeTooltipAddonPlacement.TryBelow(1920,1080,new UiRect(500,30,420,120),200,42,1,out invalid),"No space beneath native tooltip declines augmentation rather than relocating native content");
        assert(!NativeTooltipAddonPlacement.TryBelow(double.NaN,1080,new UiRect(500,300,420,120),200,42,1,out invalid),"Unknown placement context fails closed");
        var dagger=OriginalItemDefinitions.Find("BleedOnHit").Create("Localized native dagger name",1);
        string addedText=ItemDetailsPresentation.Create(new[]{dagger},1,1);
        assert(addedText.Contains("10%")&&addedText.Contains("20%")&&addedText.Contains("attack proc coefficient 1, luck 0"),"Expanded added block includes current/next and required context");
        assert(!addedText.Contains("Localized native dagger name")&&!addedText.Contains("Shift"),"Added block does not duplicate native title or introduce a modifier shortcut");
        var beads=OriginalItemDefinitions.Find("ExtraStatsOnLevelUp").Create("Beads",4);
        addedText=ItemDetailsPresentation.Create(new[]{beads},4,4);
        assert(addedText.Contains("35%")&&addedText.Contains("5%/extra bead")&&addedText.Contains("40%"),"Click details retains correct n-1 additive current/formula/next terms");
        assert(ItemDetailsPresentation.Create(null,0,3)=="","Unsupported scaling does not fabricate a next benefit");
        var calculation=new Calculation("Base",110);calculation.Apply("Level growth",CalculationStage.Flat,99);
        string stat=TooltipMarkup.Render(StatTooltipPresentation.Create(209,33,calculation,"cIsHealing",growthSource:"Level growth"),s=>false);
        assert(stat.Split((char)10).SequenceEqual(new[]{"Total: 209","Base: 110 + 33 per level","Bonuses: None"}),"Clickable stat details preserve the latest compact health structure");
        Console.WriteLine("Additive Details coverage:24 Tab sessions x16 items, pointer transfers, exact-once native exits, "+layouts+" placement cases and zero modifier-key input.");
    }
}
