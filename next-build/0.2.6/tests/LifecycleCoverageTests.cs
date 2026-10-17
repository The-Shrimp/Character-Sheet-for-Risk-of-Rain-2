using System;
using System.Collections.Generic;
using Alexander.CharacterSheet.Core;
static class LifecycleCoverageTests
{
    public static void Run(Action<bool,string> assert)
    {
        object buffHud=new object(),foreignHud=new object(),buffBody=new object(),foreignBody=new object();
        assert(NativeBuffHoverPolicy.CanHover(buffHud,buffHud,buffBody,buffBody,true,true,false,false,1),"Own active visible buff with Tab cursor is eligible");
        assert(!NativeBuffHoverPolicy.CanHover(buffHud,foreignHud,buffBody,buffBody,true,true,false,false,1),"Remote HUD buff is excluded");
        assert(!NativeBuffHoverPolicy.CanHover(buffHud,buffHud,buffBody,foreignBody,true,true,false,false,1),"Spectated or replaced source cannot match own body");
        assert(!NativeBuffHoverPolicy.CanHover(buffHud,buffHud,buffBody,buffBody,false,true,false,false,1),"Closed Tab has no buff hover target");
        assert(!NativeBuffHoverPolicy.CanHover(buffHud,buffHud,buffBody,buffBody,true,false,true,false,1),"Gameplay aim cannot activate a buff hover");
        assert(!NativeBuffHoverPolicy.CanHover(buffHud,buffHud,buffBody,buffBody,true,true,false,true,1),"Hidden buff cannot activate a hover");
        assert(!NativeBuffHoverPolicy.CanHover(buffHud,buffHud,buffBody,buffBody,true,true,false,false,0),"Expired buff cannot reuse a stale hover");
        assert(!NativeBuffHoverPolicy.CanHover(null,null,buffBody,buffBody,true,true,false,false,1),"Missing HUD cannot match itself");
        var buffRegistry=new NativeBindingRegistry();object pooledIcon=new object(),oldBinding=new object(),newBinding=new object();
        assert(buffRegistry.TryClaim(pooledIcon,oldBinding)&&!buffRegistry.TryClaim(pooledIcon,newBinding),"Pooled buff icon has one active binding");
        assert(buffRegistry.Release(pooledIcon,oldBinding)&&buffRegistry.TryClaim(pooledIcon,newBinding),"Detach permits safe reuse by next HUD session");
        assert(!buffRegistry.Release(pooledIcon,oldBinding)&&buffRegistry.Owns(pooledIcon,newBinding),"Delayed destroy cannot release replacement owner");

        assert(LuckEvidence.AdjustedProbability(.1,2,LuckApplicability.Affected,LuckEvidenceState.ReportedBug)==null,"Bug-flagged luck never produces a confident adjusted probability");
        assert(LuckEvidence.AdjustedProbability(.1,2,LuckApplicability.Affected,LuckEvidenceState.Unverified)==null,"Unverified luck never produces a confident adjusted probability");
        assert(LuckEvidence.AdjustedProbability(.1,2,LuckApplicability.Affected,LuckEvidenceState.Unsupported)==null,"Unsupported effects do not infer Clover behavior");
        assert(LuckEvidence.Label(LuckEvidenceState.ReportedBug)!=LuckEvidence.Label(LuckEvidenceState.Unsupported)&&LuckEvidence.Label(LuckEvidenceState.ReportedBug).Contains("unverified"),"Reported bug is distinct from unsupported or unaffected");
        assert(Math.Abs(LuckEvidence.AdjustedProbability(.1,1,LuckApplicability.Affected,LuckEvidenceState.Confirmed).Value-.19)<1e-9,"Explicit confirmed affected roll can use the known luck math");
        assert(LuckEvidence.AdjustedProbability(.1,1,LuckApplicability.Mixed,LuckEvidenceState.Confirmed)==null,"Mixed effect does not get one blanket adjusted chance");
        assert(LuckEvidence.AdjustedProbability(double.NaN,1,LuckApplicability.Affected,LuckEvidenceState.Confirmed)==null,"Nonfinite roll input stays unavailable");
        object a=new object(),b=new object(),icon=new object(),local=new object(),hud=new object(),body=new object(),pause=new object();
        var gate=new ExclusiveOwnerGate();
        assert(gate.TryEnter(a)&&gate.TryEnter(a),"Repeated initialization by the same owner is idempotent");
        assert(!gate.TryEnter(b)&&!gate.Release(b)&&gate.IsOwner(a),"Duplicate installation and foreign shutdown cannot displace active owner");
        assert(gate.Release(a)&&gate.TryEnter(b),"A later legitimate owner can start after teardown");
        assert(!gate.Release(a)&&gate.IsOwner(b),"Old owner teardown cannot clear replacement owner");
        var registry=new NativeBindingRegistry();
        assert(registry.TryClaim(icon,a)&&registry.TryClaim(icon,a)&&registry.Count==1,"Refresh does not duplicate a native icon attachment");
        assert(!registry.TryClaim(icon,b)&&!registry.Release(icon,b),"A second view cannot claim or release an active binding");
        assert(registry.Release(icon,a)&&!registry.Release(icon,a),"Binding teardown is idempotent");
        assert(registry.TryClaim(icon,b)&&!registry.Release(icon,a)&&registry.Owns(icon,b),"Deferred old destruction cannot detach a reopened view");
        assert(!registry.TryClaim(null,b)&&!registry.TryClaim(icon,null),"Invalid binding identity fails closed");
        registry.Release(icon,b);
        var session=new NativeSessionTracker();
        var t=session.Reconcile(true,true,local,hud,body,pause);
        assert(t.Open&&!t.Close&&session.IsOpen,"First visible eligible session opens once");
        t=session.Reconcile(true,true,local,hud,body,pause);
        assert(!t.Open&&!t.Close,"Steady session cannot install duplicate view");
        t=session.Reconcile(true,true,local,hud,new object(),pause);
        assert(t.Close&&t.Open,"Respawn/body replacement retires the old view before opening a new one");
        object nextBody=new object();
        session.Reconcile(true,true,local,hud,nextBody,pause);
        t=session.Reconcile(true,true,local,new object(),nextBody,pause);
        assert(t.Close&&t.Open,"HUD replacement replaces attachment scope");
        t=session.Reconcile(true,true,new object(),hud,nextBody,pause);
        assert(t.Close&&t.Open,"Local user switch replaces scope");
        t=session.Reconcile(true,true,local,hud,body,new object());
        assert(t.Close&&t.Open,"Native pause-controller replacement cannot retain a stale lease session");
        t=session.Reconcile(false,true,local,hud,body,pause);
        assert(t.Close&&!t.Open&&!session.IsOpen,"Focus loss, death, missing run or menu takeover closes additions");
        t=session.Reconcile(false,true,local,hud,body,pause);
        assert(!t.Close&&!t.Open,"Repeated ineligible frames do not repeat teardown");
        t=session.Reconcile(true,true,local,hud,body,pause);
        assert(t.Open&&!t.Close,"Eligible return can open a fresh native session");
        t=session.Reconcile(true,false,local,hud,body,pause);
        assert(t.Close&&!t.Open,"Native Tab closure retires the enhancement");
        assert(!session.Reconcile(true,true,local,null,body,pause).Open,"Missing HUD cannot create a replacement overlay");
        session.Reconcile(true,true,local,hud,body,pause);session.Reset();
        assert(!session.IsOpen&&session.Reconcile(true,true,local,hud,body,pause).Open,"Scene/disable reset permits one clean later open");
        var lease=new PauseLease();var requests=new List<bool>();
        lease.OpenAfterNative(true,true,true,pause,requests.Add);
        assert(lease.ProtectNativeClose(pause)&&!lease.ProtectNativeClose(new object()),"Borrowed pause protection is scoped to the exact native controller");
        lease.Close(true,new object(),requests.Add);
        assert(requests.Count==0&&!lease.IsOpen,"Scene replacement cannot unpause a different controller");
        session.Reset();
        t=session.Reconcile(true,true,local,hud,body,pause);
        object scope1=new object(),scope2=new object();
        registry.TryClaim(icon,scope1);lease.OpenAfterNative(true,false,true,pause,requests.Add);
        t=session.Reconcile(true,true,local,hud,new object(),pause);
        if(t.Close){registry.Release(icon,scope1);lease.Close(true,pause,requests.Add);}
        if(t.Open){registry.TryClaim(icon,scope2);lease.OpenAfterNative(true,false,false,pause,requests.Add);}
        assert(registry.Count==1&&registry.Owns(icon,scope2)&&!registry.Owns(icon,scope1),"Respawn sequence retains exactly one fresh attachment");
        assert(requests.Count==2&&!requests[0]&&requests[1],"Replace sequence releases old acquired pause before requesting a new one");
        assert(!registry.Release(icon,scope1)&&registry.Owns(icon,scope2),"Delayed old callback cannot touch new session");
        lease.ExternalPauseRequest();lease.Close(true,pause,requests.Add);registry.Release(icon,scope2);
        assert(requests.Count==2&&registry.Count==0,"External takeover plus teardown retains pause and releases only our attachment");
        var throwing=new PauseLease();
        bool caught=false;
        try{throwing.Open(true,false,pause,v=>{throw new InvalidOperationException();});}catch(InvalidOperationException){caught=true;}
        assert(caught&&!throwing.IsOpen&&throwing.Token==null,"Failed pause acquisition clears ownership");
        throwing.OpenAfterNative(true,false,true,pause,requests.Add);
        caught=false;try{throwing.Close(true,pause,v=>{throw new InvalidOperationException();});}catch(InvalidOperationException){caught=true;}
        assert(caught&&!throwing.IsOpen,"Failed release is not retried with stale ownership");
        assert(OriginalItemDefinitions.SupportedIds.Count==37,"Explicit registry includes 14 additional reviewed common-item components");
        assert(OriginalItemDefinitions.Find("unknown")==null&&OriginalItemDefinitions.Find(null)==null,"Unsupported IDs have no invented numeric formula");
        var beads=OriginalItemDefinitions.Find("ExtraStatsOnLevelUp").Create("Prayer Beads",4);
        assert(beads.CurrentValue==35&&beads.NextValue==40&&beads.Comparison.Confidence.Contains("Conditional"),"Live-source bead definition remains a conditional component example");
        var missileDef=OriginalItemDefinitions.Find("Missile");
        var missile=missileDef.Create("AtG",4);
        assert(missile.CurrentValue==1200&&missile.NextValue==1500,"AtG current and next coefficients are distinct from a chance prediction");
        assert(missile.LuckMark=="mix"&&missile.LuckText.Contains("trigger roll"),"On-hit roll and damage-scaling luck scopes stay distinct");
        assert(missileDef.TriggerContext.Contains("unavailable")&&missileDef.TriggerContext.Contains("not base trigger chance"),"Unknown attack proc coefficient does not fabricate actual chance");
        assert(missileDef.Revision==OriginalItemDefinitions.Revision&&missileDef.Source.Contains(missileDef.Revision),"Formula provenance is pinned to a source revision");
        assert(missileDef.Create("AtG",null).CurrentValue==null,"Unknown effective count cannot produce an on-hit coefficient");
        assert(missileDef.Create("AtG",0).NextValue==300,"First next copy activates the missile damage component");
        assert(OriginalItemDefinitions.Find("CritGlasses").Create("Glasses",2).LuckMark=="mix","Stat term and crit-roll luck applicability differ");
        assert(OriginalItemDefinitions.Find("FlatHealth").Create("Steak",2).CurrentValue==50,"Flat-health registry retains component math");
        assert(BuffGlossaryRules.NectarEligible(false,false,false),"Ordinary unignored active type can be eligible");
        assert(!BuffGlossaryRules.NectarEligible(true,false,false)&&!BuffGlossaryRules.NectarEligible(false,true,false),"Definition and catalog exclusions both exclude a type");
        assert(!BuffGlossaryRules.NectarEligible(false,false,true),"Nectar counter never counts itself");
        assert(BuffGlossaryRules.Kind(false,false,true).Contains("Counter"),"Nectar type counter is distinguished from an ordinary buff");
        assert(BuffGlossaryRules.Kind(false,true,false).Contains("Cooldown"),"Cooldown flag is not lost");
        assert(BuffGlossaryRules.Kind(true,false,false)=="Debuff","Debuff classification is explicit");
        assert(BuffGlossaryRules.NectarEligible(false,false,false),"Stack count is not part of per-type eligibility");
    }
}
