using RoR2;
using UnityEngine;
namespace Alexander.CharacterSheet
{
    // SetMaster's public callback supplies identity; no private native fields are read.
    internal sealed class NativeStripIdentity:MonoBehaviour
    {
        internal CharacterMaster OwnMaster;
    }
}
