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
            else if(Kind==SheetGlyph.Runner){Line(mesh,new Vector2(.12f,.28f),new Vector2(.88f,.28f),.13f);Line(mesh,new Vector2(.23f,.31f),new Vector2(.31f,.7f),.19f);Line(mesh,new Vector2(.32f,.58f),new Vector2(.72f,.28f),.13f);}
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
