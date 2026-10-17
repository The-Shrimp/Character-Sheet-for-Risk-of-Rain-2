using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace Alexander.CharacterSheet.Core
{
    public sealed class ExclusiveOwnerGate
    {
        object owner;
        public bool TryEnter(object candidate)
        {if(candidate==null)return false;if(owner==null)owner=candidate;return ReferenceEquals(owner,candidate);}
        public bool IsOwner(object candidate)=>candidate!=null&&ReferenceEquals(owner,candidate);
        public bool Release(object candidate){if(!IsOwner(candidate))return false;owner=null;return true;}
    }
    sealed class ReferenceKeyComparer:IEqualityComparer<object>
    {
        public new bool Equals(object left,object right)=>ReferenceEquals(left,right);
        public int GetHashCode(object key)=>RuntimeHelpers.GetHashCode(key);
    }
    public sealed class NativeBindingRegistry
    {
        readonly Dictionary<object,object> owners=new Dictionary<object,object>(new ReferenceKeyComparer());
        public int Count=>owners.Count;
        public bool TryClaim(object key,object owner)
        {
            if(key==null||owner==null)return false;
            object existing;if(owners.TryGetValue(key,out existing))return ReferenceEquals(existing,owner);
            owners.Add(key,owner);return true;
        }
        public bool Owns(object key,object owner)
        {object existing;return key!=null&&owner!=null&&owners.TryGetValue(key,out existing)&&ReferenceEquals(existing,owner);}
        public bool Release(object key,object owner){if(!Owns(key,owner))return false;return owners.Remove(key);}
    }
    public readonly struct NativeSessionTransition
    {
        public readonly bool Close,Open;
        internal NativeSessionTransition(bool close,bool open){Close=close;Open=open;}
    }
    public sealed class NativeSessionTracker
    {
        object local,hud,body,pause;
        public bool IsOpen{get;private set;}
        public NativeSessionTransition Reconcile(bool eligible,bool visible,object nextLocal,object nextHud,object nextBody,object nextPause)
        {
            bool desired=eligible&&visible&&nextLocal!=null&&nextHud!=null&&nextBody!=null;
            bool replaced=IsOpen&&desired&&(!ReferenceEquals(local,nextLocal)||!ReferenceEquals(hud,nextHud)||
                !ReferenceEquals(body,nextBody)||!ReferenceEquals(pause,nextPause));
            bool close=IsOpen&&(!desired||replaced),open=desired&&(!IsOpen||replaced);
            IsOpen=desired;
            local=desired?nextLocal:null;hud=desired?nextHud:null;body=desired?nextBody:null;pause=desired?nextPause:null;
            return new NativeSessionTransition(close,open);
        }
        public void Reset(){IsOpen=false;local=hud=body=pause=null;}
    }
    public static class BuffGlossaryRules
    {
        public static bool NectarEligible(bool hidden,bool definitionIgnored,bool catalogIgnored,bool nectarCounter)=>
            !hidden&&NectarEligible(definitionIgnored,catalogIgnored,nectarCounter);
        public static bool NectarEligible(bool definitionIgnored,bool catalogIgnored,bool nectarCounter)=>
            !definitionIgnored&&!catalogIgnored&&!nectarCounter;
        public static string Kind(bool debuff,bool cooldown,bool nectarCounter)=>
            nectarCounter?"Counter (Nectar types)":cooldown?"Cooldown / counter":debuff?"Debuff":"Buff / status";
    }
}
