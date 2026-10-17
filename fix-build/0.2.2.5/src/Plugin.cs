using System;
using System.Collections.Generic;
using Alexander.CharacterSheet.Core;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Alexander.CharacterSheet
{
    [BepInPlugin(Id,"Alexander's Character Sheet","0.2.2.5")]
    public sealed class Plugin:BaseUnityPlugin
    {
        public const string Id="alexander.charactersheet";
        static readonly ExclusiveOwnerGate InstanceGate=new ExclusiveOwnerGate();
        internal static Plugin Instance;
        internal readonly PauseLease Lease=new PauseLease();
        readonly NativeSessionTracker session=new NativeSessionTracker();
        readonly NativeInfoActivationGate activation=new NativeInfoActivationGate();
        internal bool InternalPauseRequest;
        internal int NativeCloseDepth;
        ConfigEntry<float> scale;
        ConfigEntry<bool> featureEnabled;
        Harmony harmony;
        HUD hud,capturedHud;
        PauseStopController capturedPause;
        bool capturedBeforePause,capturedOpen,sheetOpen,hasFocus=true,initialized,subscribed,faulted;
        SheetView view;
        LocalUser local;
        CharacterBody body;
        float nextRefresh;
        string lastStatus;
        readonly List<NativeStripIdentity> identities=new List<NativeStripIdentity>();
        internal bool CanRun=>InstanceGate.IsOwner(this)&&initialized&&isActiveAndEnabled&&featureEnabled!=null&&featureEnabled.Value;
        void Awake()
        {
            if(initialized)return;
            if(!InstanceGate.TryEnter(this)){Logger.LogWarning("Duplicate source instance skipped; active owner retained.");enabled=false;return;}
            Instance=this;
            featureEnabled=Config.Bind("General","Enabled",true,"Enable this UI addition. False leaves native UI/control behavior in place; takes effect on future startup or a live configuration reload.");
            scale=Config.Bind("Presentation","Scale",1f,"Addition scale (0.7-1.2); does not change native scoreboard geometry or HUD settings.");
            try
            {
                harmony=new Harmony(Id);harmony.PatchAll(typeof(Plugin).Assembly);
                SceneManager.activeSceneChanged+=SceneChanged;subscribed=true;initialized=true;
                Logger.LogInfo("Native Tab-extension source 0.2.2.5; HUD branch placement and held-Info re-entry guards; in-game fix verification pending.");
            }
            catch(Exception error)
            {
                Logger.LogError(error);harmony?.UnpatchSelf();InstanceGate.Release(this);
                if(Instance==this)Instance=null;enabled=false;
            }
        }
        void LateUpdate()
        {
            try
            {
                if(!CanRun){Cleanup();return;}
                var user=LocalUserManager.GetFirstLocalUser();
                var nextHud=user?.cameraRigController?user.cameraRigController.hud:null;
                var nextBody=user?.cachedBody;
                bool requested=nextHud&&nextHud.scoreboardPanel&&nextHud.scoreboardPanel.activeSelf;
                bool visible=nextHud&&nextHud.scoreboardPanel&&nextHud.scoreboardPanel.activeInHierarchy;
                if(!requested)faulted=false;
                bool menu=PauseManager.PauseScreenIsOpen()&&user?.pauseScreenController&&!user.pauseScreenController.isMinimallyPause;
                bool eligible=hasFocus&&!faulted&&Run.instance&&nextBody&&nextBody.healthComponent&&nextBody.healthComponent.alive&&!menu;
                if(menu&&sheetOpen)Lease.ExternalPauseRequest();
                var transition=session.Reconcile(eligible&&activation.Allows(user,nextHud),requested,user,nextHud,nextBody,PauseStopController.instance);
                local=user;hud=nextHud;body=nextBody;
                if(transition.Close){if(!requested)activation.NativeClosed(user,nextHud);Cleanup(false);}
                if(transition.Open)Open();
                if(!sheetOpen)return;
                var pause=PauseStopController.instance;
                if(ReferenceEquals(Lease.Token,pause))Lease.Observe(pause&&pause.isPaused);
                view.SetNativeVisibility(visible);
                if(!visible)return;
                if(Time.unscaledTime>=nextRefresh)
                {
                    nextRefresh=Time.unscaledTime+.2f;view.Refresh(body);
                    if(view.Status!=lastStatus){lastStatus=view.Status;Logger.LogInfo(lastStatus);}
                }
                view.UpdateTooltip();
            }
            catch(Exception error){faulted=true;Logger.LogError(error);Cleanup();}
        }
        internal void CaptureNativeOpen(HUD source)
        {
            if(!CanRun)return;
            var user=LocalUserManager.GetFirstLocalUser();
            var ownHud=user?.cameraRigController?user.cameraRigController.hud:null;
            if(source!=ownHud)return;
            var input=user?.inputPlayer;
            if(!activation.CaptureOpen(user,source,input!=null&&input.GetButton("info"),input!=null&&input.GetButtonDown("info"))&&activation.TakeWarning())
                Logger.LogWarning("Native Info reopened during the same held press; additions suspended until a fresh mapped Info press to prevent a visibility/pause loop.");
            capturedHud=source;capturedPause=PauseStopController.instance;
            capturedBeforePause=(capturedPause&&capturedPause.isPaused)||Time.timeScale==0;capturedOpen=true;
        }
        void Open()
        {
            if(!hud||!hud.scoreboardPanel)return;
            var pause=PauseStopController.instance;
            bool now=(pause&&pause.isPaused)||Time.timeScale==0;
            bool before=capturedOpen&&capturedHud==hud&&ReferenceEquals(capturedPause,pause)?capturedBeforePause:now;
            capturedOpen=false;
            Lease.OpenAfterNative(RoR2Application.isInSinglePlayer,before,now,pause,RequestPause);
            view=new SheetView(hud,Mathf.Clamp(scale.Value,.7f,1.2f));sheetOpen=true;lastStatus=null;nextRefresh=0;view.Refresh(body);
        }
        void RequestPause(bool value)
        {
            var pause=PauseStopController.instance;if(!pause||!RoR2Application.isInSinglePlayer)return;
            InternalPauseRequest=true;try{pause.Pause(value);}finally{InternalPauseRequest=false;}
        }
        void Cleanup(bool resetSession=true)
        {
            var previous=view;view=null;
            try{previous?.Dispose();}catch(Exception error){Logger.LogError(error);}
            try{Lease.Close(RoR2Application.isInSinglePlayer,PauseStopController.instance,RequestPause);}
            catch(Exception error){Logger.LogError(error);}
            finally{sheetOpen=false;if(resetSession){session.Reset();activation.Reset();}}
        }
        void ClearIdentities(){foreach(var identity in identities)if(identity)identity.OwnMaster=null;}
        void SceneChanged(Scene previous,Scene next){Cleanup();local=null;hud=null;body=null;capturedOpen=false;faulted=false;ClearIdentities();}
        void OnApplicationFocus(bool focused){hasFocus=focused;if(!focused){Cleanup();capturedOpen=false;}}
        void OnDisable(){Cleanup();capturedOpen=false;ClearIdentities();}
        void OnDestroy()
        {
            Cleanup();
            if(subscribed){SceneManager.activeSceneChanged-=SceneChanged;subscribed=false;}
            harmony?.UnpatchSelf();
            foreach(var identity in identities)if(identity)UnityEngine.Object.Destroy(identity);
            identities.Clear();initialized=false;InstanceGate.Release(this);if(Instance==this)Instance=null;
        }
        [HarmonyPatch(typeof(ScoreboardStrip),nameof(ScoreboardStrip.SetMaster))]
        static class NativeIdentityPatch
        {
            [HarmonyPostfix]static void Postfix(ScoreboardStrip __instance,CharacterMaster __0)
            {
                var plugin=Instance;if(plugin==null||!__instance)return;
                try
                {
                    var identity=__instance.GetComponent<NativeStripIdentity>();
                    if(!plugin.CanRun){if(identity)identity.OwnMaster=null;return;}
                    var own=LocalUserManager.GetFirstLocalUser()?.cachedMaster;
                    bool isOwn=own&&ReferenceEquals(own,__0);
                    if(!identity&&isOwn)
                    {
                        identity=__instance.gameObject.AddComponent<NativeStripIdentity>();
                        plugin.identities.RemoveAll(i=>!i);plugin.identities.Add(identity);
                    }
                    if(identity)identity.OwnMaster=isOwn?own:null;
                }
                catch(Exception error){plugin.Logger.LogError(error);}
            }
        }
        [HarmonyPatch(typeof(HUD),nameof(HUD.ActivateScoreboard))]
        static class NativeOpenPatch
        {
            [HarmonyPrefix]static void Prefix(HUD __instance){Instance?.CaptureNativeOpen(__instance);}
        }
        [HarmonyPatch(typeof(HUD),nameof(HUD.DeactivateScoreboard))]
        static class NativeClosePatch
        {
            [HarmonyPrefix]static void Prefix(HUD __instance,out bool __state)
            {__state=Instance!=null&&Instance.CanRun&&Instance.sheetOpen&&Instance.hud==__instance;if(__state)Instance.NativeCloseDepth++;}
            [HarmonyFinalizer]static Exception Finalizer(Exception __exception,bool __state)
            {if(__state&&Instance!=null)Instance.NativeCloseDepth=Math.Max(0,Instance.NativeCloseDepth-1);return __exception;}
        }
        [HarmonyPatch(typeof(PauseStopController),nameof(PauseStopController.Pause),new Type[]{typeof(bool)})]
        static class NativePausePatch
        {
            [HarmonyPrefix]static bool Prefix(PauseStopController __instance,bool __0)
            {
                var plugin=Instance;
                if(plugin==null||!plugin.CanRun||plugin.InternalPauseRequest||!plugin.sheetOpen||!RoR2Application.isInSinglePlayer)return true;
                if(!__0&&plugin.NativeCloseDepth>0)
                {
                    if(plugin.Lease.ProtectNativeClose(__instance))return false;
                    plugin.Lease.AcknowledgeNativeClose(__instance);return true;
                }
                if(plugin.NativeCloseDepth==0&&ReferenceEquals(plugin.Lease.Token,__instance))plugin.Lease.ExternalPauseRequest();return true;
            }
        }
    }
}
