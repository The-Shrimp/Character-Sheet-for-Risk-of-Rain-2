using RoR2.UI;
using TMPro;
using UnityEngine;
class PublicApiCompileProbe {
    void ReadPublicStyle(TooltipController controller,TMP_Text text,TMP_StyleSheet sheet) {
        TMP_Text body=controller.bodyLabel;
        TMP_Text title=controller.titleLabel;
        var tooltipSheet=text.styleSheet;
        var defaultSheet=TMP_Settings.defaultStyleSheet;
        var namedStyle=sheet.GetStyle("cIsDamage");
        var scene=controller.gameObject.scene;
    }
}
