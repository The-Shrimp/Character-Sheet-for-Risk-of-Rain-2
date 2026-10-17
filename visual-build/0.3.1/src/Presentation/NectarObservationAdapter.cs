using System.Collections.Generic;
using System.Linq;
using Alexander.CharacterSheet.Core;
using RoR2;
namespace Alexander.CharacterSheet
{
    internal static class NectarObservationAdapter
    {
        internal static NectarCounterView Read(CharacterBody body,long effectiveCopies)
        {
            if(!body)return NectarCounterPresentation.Create(effectiveCopies,null,null,null,null,true);
            var self=DLC2Content.Buffs.BoostAllStatsBuff;
            List<NectarBuffObservation> observations=null;
            if(BuffCatalog.buffCount>0)
            {
                observations=new List<NectarBuffObservation>();
                var ignored=BuffCatalog.ignoreGrowthNectarIndices;
                for(int i=0;i<BuffCatalog.buffCount;i++)
                {
                    var index=(BuffIndex)i;int stacks=body.GetBuffCount(index);
                    if(stacks<=0)continue;
                    var definition=BuffCatalog.GetBuffDef(index);
                    observations.Add(new NectarBuffObservation{
                        Id="buff:"+i,Stacks=stacks,
                        Hidden=definition?(bool?)definition.isHidden:null,
                        DefinitionIgnored=definition?(bool?)definition.ignoreGrowthNectar:null,
                        CatalogIgnored=ignored!=null?(bool?)ignored.Contains(index):null,
                        SelfCounter=definition&&self?(bool?)(definition==self):null
                    });
                }
            }
            return NectarCounterPresentation.Create(effectiveCopies,observations,
                body.currentGrowthNectarBuffCount,body.maxGrowthNectarBuffCount,
                self?(bool?)body.HasBuff(self):null,true);
        }
    }
}
