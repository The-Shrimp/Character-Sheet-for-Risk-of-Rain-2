using System;
using System.Collections.Generic;
using Alexander.CharacterSheet.Core;
static class NativeTabRegressionTests
{
    sealed class Node
    {
        public readonly List<Node> Children=new List<Node>();
        public Node Parent;public bool Active=true;
        public bool Visible=>Active&&(Parent==null||Parent.Visible);
        public Node Add(){var n=new Node{Parent=this};Children.Add(n);return n;}
    }
    static void NativeHudTick(Node hud)
    {for(int i=0;i<hud.Children.Count;i++)hud.Children[i].Active=i==hud.Children.Count-1;}
    public static void Run(Action<bool,string> assert)
    {
        var oldHud=new Node();var oldBranch=oldHud.Add();var oldBoard=oldBranch.Add();
        oldHud.Add();oldHud.Add();NativeHudTick(oldHud);
        assert(!oldBoard.Visible,"Regression fixture reproduces native HUD hiding scoreboard after root-level additions");
        assert(!NativeHudPlacementPolicy.CanAttach(0,true,true),"Root HUD placement is rejected before owned objects are created");
        assert(!NativeHudPlacementPolicy.CanAttach(-1,true,true)&&!NativeHudPlacementPolicy.CanAttach(1,false,true)&&
            !NativeHudPlacementPolicy.CanAttach(1,true,false),"Unknown, inactive and unrelated placement contexts fail closed");
        assert(NativeHudPlacementPolicy.CanAttach(1,true,true),"A visible existing scoreboard branch is accepted");
        var hud=new Node();hud.Add();var branch=hud.Add();var board=branch.Add();
        var layer=branch.Add();layer.Add();var popup=layer.Add();
        int nativeChildren=hud.Children.Count;
        for(int i=0;i<240;i++){popup.Active=i%2==0;NativeHudTick(hud);}
        assert(hud.Children.Count==nativeChildren&&board.Visible,"240 native updates and popup visibility changes preserve native HUD children and scoreboard");
        var requests=new List<bool>();object user=new object(),body=new object(),pause=new object();
        var session=new NativeSessionTracker();var lease=new PauseLease();var gate=new NativeInfoActivationGate();
        int opens=0,closes=0;
        for(int cycle=0;cycle<12;cycle++)
        {
            gate.CaptureOpen(user,hud,true,true);board.Active=true;
            for(int frame=0;frame<120;frame++)
            {
                NativeHudTick(hud);
                var transition=session.Reconcile(gate.Allows(user,hud),board.Active,user,hud,body,pause);
                if(transition.Open){opens++;lease.OpenAfterNative(true,false,false,pause,requests.Add);lease.Observe(true);}
                assert(!transition.Close&&board.Visible,"Held native Info keeps one visible session without reattachment");
            }
            board.Active=false;var close=session.Reconcile(true,board.Active,user,hud,body,pause);
            if(close.Close){closes++;gate.NativeClosed(user,hud);lease.Close(true,pause,requests.Add);}
            lease.Close(true,pause,requests.Add);
        }
        assert(opens==12&&closes==12&&requests.Count==24,"Repeated hold/release pairs attach once and acquire/release pause once per press");
        gate.Reset();session.Reset();requests.Clear();gate.CaptureOpen(user,hud,true,true);board.Active=true;
        var opened=session.Reconcile(true,true,user,hud,body,pause);lease.OpenAfterNative(true,false,false,pause,requests.Add);
        branch.Active=false;
        var hidden=session.Reconcile(true,board.Active,user,hud,body,pause);
        assert(opened.Open&&!hidden.Close&&!hidden.Open&&!board.Visible&&lease.IsOpen,
            "Ancestor visibility loss does not recursively destroy and recreate an intended native Info session");
        branch.Active=true;var shown=session.Reconcile(true,board.Active,user,hud,body,pause);
        assert(!shown.Open&&!shown.Close&&board.Visible,"Ancestor restoration reuses the existing view");
        // If a pause subscriber suppresses Info input, native close still releases our pause once.
        board.Active=false;session.Reconcile(true,false,user,hud,body,pause);gate.NativeClosed(user,hud);lease.Close(true,pause,requests.Add);
        for(int i=0;i<120;i++)
        {
            bool allowed=gate.CaptureOpen(user,hud,true,false);
            var reopen=session.Reconcile(allowed,true,user,hud,body,pause);
            assert(!allowed&&!reopen.Open&&!reopen.Close,"A held key returning after pause-related input loss cannot reacquire pause or recreate additions");
        }
        assert(requests.Count==2&&requests[0]&&!requests[1],"Input-loss close has one pause acquisition and one release");
        assert(gate.TakeWarning()&&!gate.TakeWarning(),"Unexpected held-key re-entry reports once instead of logging every frame");
        assert(gate.CaptureOpen(user,hud,true,true)&&session.Reconcile(true,true,user,hud,body,pause).Open,
            "A fresh mapped Info press can start a clean session after held-input suppression");
        gate.NativeClosed(user,hud);
        assert(gate.CaptureOpen(user,hud,false,false),"Programmatic native opening without held Info remains eligible");
        gate.NativeClosed(user,hud);object replacement=new object();
        assert(gate.CaptureOpen(user,replacement,true,false),"Replacement HUD has independent activation ownership");
        gate.NativeClosed(user,hud);assert(gate.Allows(user,replacement),"Delayed old-HUD close cannot suppress a replacement HUD");
        assert(!gate.CaptureOpen(null,replacement,true,true)&&!gate.Allows(null,replacement),"Missing local owner fails closed");
        gate.Reset();assert(gate.Allows(user,hud),"Scene/focus teardown clears activation scope for a later legitimate session");
        requests.Clear();var borrowed=new PauseLease();borrowed.OpenAfterNative(true,true,true,pause,requests.Add);
        borrowed.Close(true,pause,requests.Add);borrowed.Close(true,pause,requests.Add);
        assert(requests.Count==0,"Visibility cleanup preserves a pause owned by another UI");
        var recursive=new PauseLease();recursive.OpenAfterNative(true,false,false,pause,v=>{});int releases=0;
        recursive.Close(true,pause,v=>{releases++;recursive.Close(true,pause,w=>releases++);});
        assert(releases==1&&!recursive.IsOpen,"Recursive native visibility/pause callbacks release owned pause exactly once");
        layer.Active=false;branch.Children.Remove(layer);layer.Parent=null;NativeHudTick(hud);
        assert(hud.Children.Count==nativeChildren&&board.Active==false&&branch.Active,
            "Owned layer teardown leaves native child order and closed scoreboard state intact");
    }
}
