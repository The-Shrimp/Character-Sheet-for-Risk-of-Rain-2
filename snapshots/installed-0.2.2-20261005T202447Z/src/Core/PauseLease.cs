using System;

namespace Alexander.CharacterSheet.Core
{
    public enum PauseOwnership { Closed, Pending, Owned, Borrowed }

    // Never manipulates a clock. The game adapter supplies the native pause operation.
    public sealed class PauseLease
    {
        public PauseOwnership Ownership { get; private set; }
        public object Token { get; private set; }
        public bool PreserveExternalPause => Ownership == PauseOwnership.Borrowed;
        public bool IsOpen => Ownership != PauseOwnership.Closed;
        public bool ProtectNativeClose(object currentToken)=>PreserveExternalPause&&ReferenceEquals(Token,currentToken);

        public void Open(bool singlePlayer, bool alreadyPaused, object token, Action<bool> request)
        {
            if (IsOpen || !singlePlayer || token == null) return;
            Token = token;
            Ownership = alreadyPaused ? PauseOwnership.Borrowed : PauseOwnership.Pending;
            if (!alreadyPaused)
            {
                try { request(true); }
                catch { Ownership = PauseOwnership.Closed; Token = null; throw; }
            }
        }

        // Adopt the native Tab pause without issuing a duplicate request.
        public void OpenAfterNative(bool singlePlayer,bool pausedBeforeOpen,bool pausedNow,object token,Action<bool> request)
        {
            if(IsOpen||!singlePlayer||token==null)return;
            if(pausedBeforeOpen)Open(singlePlayer,true,token,request);
            else if(pausedNow){Token=token;Ownership=PauseOwnership.Owned;}
            else Open(singlePlayer,false,token,request);
        }
        // Only while native scoreboard close is releasing this exact controller.
        public bool AcknowledgeNativeClose(object currentToken)
        {
            if(!ReferenceEquals(Token,currentToken)||(Ownership!=PauseOwnership.Owned&&Ownership!=PauseOwnership.Pending))return false;
            Ownership=PauseOwnership.Closed;Token=null;return true;
        }

        public void Observe(bool paused)
        {
            if (Ownership == PauseOwnership.Pending && paused) Ownership = PauseOwnership.Owned;
            else if (Ownership == PauseOwnership.Owned && !paused) Ownership = PauseOwnership.Borrowed;
        }

        public void ExternalPauseRequest()
        {
            if (IsOpen) Ownership = PauseOwnership.Borrowed;
        }

        public void Close(bool singlePlayer, object currentToken, Action<bool> request)
        {
            bool release = singlePlayer && ReferenceEquals(Token, currentToken) &&
                (Ownership == PauseOwnership.Owned || Ownership == PauseOwnership.Pending);
            // Clear first, so cleanup is idempotent even when game callbacks re-enter.
            Ownership = PauseOwnership.Closed;
            Token = null;
            if (release) request(false);
        }
    }
}

