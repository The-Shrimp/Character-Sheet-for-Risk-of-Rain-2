using Alexander.CharacterSheet.Core;
using UnityEngine;
using UnityEngine.UI;
namespace Alexander.CharacterSheet
{
    // Original vector shapes, generated only on owned UI geometry.
    internal sealed class SheetGlyphGraphic:MaskableGraphic
    {
        internal SheetGlyph Kind;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if(Kind==SheetGlyph.Shield)Polygon(mesh,new[]{new Vector2(.15f,.9f),new Vector2(.85f,.9f),new Vector2(.8f,.4f),new Vector2(.5f,.08f),new Vector2(.2f,.4f)});
            else if(Kind==SheetGlyph.Sword){Line(mesh,new Vector2(.22f,.15f),new Vector2(.82f,.86f),.11f);Line(mesh,new Vector2(.12f,.4f),new Vector2(.48f,.1f),.09f);}
            else if(Kind==SheetGlyph.Runner){
                Circle(mesh,new Vector2(.63f,.84f),.11f);
                Line(mesh,new Vector2(.57f,.68f),new Vector2(.45f,.43f),.14f);
                Line(mesh,new Vector2(.57f,.66f),new Vector2(.32f,.62f),.09f);Line(mesh,new Vector2(.32f,.62f),new Vector2(.22f,.48f),.09f);
                Line(mesh,new Vector2(.54f,.65f),new Vector2(.75f,.49f),.09f);
                Line(mesh,new Vector2(.46f,.43f),new Vector2(.7f,.3f),.12f);Line(mesh,new Vector2(.7f,.3f),new Vector2(.83f,.12f),.12f);
                Line(mesh,new Vector2(.47f,.43f),new Vector2(.32f,.18f),.12f);Line(mesh,new Vector2(.32f,.18f),new Vector2(.13f,.18f),.1f);
            }
            else if(Kind==SheetGlyph.CrossedSwords){
                Line(mesh,new Vector2(.2f,.16f),new Vector2(.83f,.87f),.1f);Line(mesh,new Vector2(.16f,.4f),new Vector2(.4f,.16f),.08f);
                Line(mesh,new Vector2(.8f,.16f),new Vector2(.17f,.87f),.1f);Line(mesh,new Vector2(.6f,.16f),new Vector2(.84f,.4f),.08f);
            }
            else if(Kind==SheetGlyph.Misc){Circle(mesh,new Vector2(.2f,.5f),.1f);Circle(mesh,new Vector2(.5f,.5f),.1f);Circle(mesh,new Vector2(.8f,.5f),.1f);}
            else if(Kind==SheetGlyph.Clover){Circle(mesh,new Vector2(.33f,.65f),.2f);Circle(mesh,new Vector2(.67f,.65f),.2f);Circle(mesh,new Vector2(.33f,.33f),.2f);Circle(mesh,new Vector2(.67f,.33f),.2f);Line(mesh,new Vector2(.5f,.4f),new Vector2(.5f,.05f),.07f);}
            else if(Kind==SheetGlyph.Skull){Circle(mesh,new Vector2(.5f,.6f),.35f);Line(mesh,new Vector2(.32f,.25f),new Vector2(.68f,.25f),.2f);Circle(mesh,new Vector2(.37f,.61f),.08f,Color.black);Circle(mesh,new Vector2(.63f,.61f),.08f,Color.black);}
            else if(Kind==SheetGlyph.Burst){for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(mesh,new Vector2(.5f,.5f)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.13f,new Vector2(.5f,.5f)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.45f,.075f);}}
        }
        Vector2 Point(Vector2 p)=>new Vector2(rectTransform.rect.xMin+p.x*rectTransform.rect.width,rectTransform.rect.yMin+p.y*rectTransform.rect.height);
        void Polygon(VertexHelper mesh,Vector2[] points,Color? ink=null)
        {
            int first=mesh.currentVertCount;
            foreach(var point in points){var v=UIVertex.simpleVert;v.position=Point(point);v.color=ink??color;mesh.AddVert(v);}
            for(int i=1;i<points.Length-1;i++)mesh.AddTriangle(first,first+i,first+i+1);
        }
        void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width)
        {var normal=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;Polygon(mesh,new[]{a-normal,b-normal,b+normal,a+normal});}
        void Circle(VertexHelper mesh,Vector2 center,float radius,Color? ink=null)
        {var points=new Vector2[12];for(int i=0;i<points.Length;i++){float a=i*Mathf.PI*2/points.Length;points[i]=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}Polygon(mesh,points,ink);}
    }
}
