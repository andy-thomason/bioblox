using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LennardJonesGraph : Graphic 
{
  delegate void AddQuad(Color32 c, float x0, float y0, float x1, float y1);

	protected override void OnPopulateMesh(VertexHelper vh)
	{
    Debug.Log("OnPopulateMesh 2");

		Vector2 corner1 = Vector2.zero;
		Vector2 corner2 = Vector2.zero;

		corner1.x = 0f;
		corner1.y = 0f;
		corner2.x = 1f;
		corner2.y = 1f;

		corner1.x -= rectTransform.pivot.x;
		corner1.y -= rectTransform.pivot.y;
		corner2.x -= rectTransform.pivot.x;
		corner2.y -= rectTransform.pivot.y;

		corner1.x *= rectTransform.rect.width;
		corner1.y *= rectTransform.rect.height;
		corner2.x *= rectTransform.rect.width;
		corner2.y *= rectTransform.rect.height;

		vh.Clear();

    float x = corner1.x;
    float y = corner1.y;
    float w = corner2.x - corner1.x;
    float h = corner2.y - corner1.y;
    int index = 0;

    AddQuad addQuad = (c, x0, y0, x1, y1) => {
		  UIVertex vert = UIVertex.simpleVert;
		  vert.color = c;

		  vert.position = new Vector2(x + x0 * w, y + y0 * h);
		  vh.AddVert(vert);

		  vert.position = new Vector2(x + x0 * w, y + y1 * h);
		  vh.AddVert(vert);

		  vert.position = new Vector2(x + x1 * w, y + y1 * h);
		  vh.AddVert(vert);

		  vert.position = new Vector2(x + x1 * w, y + y0 * h);
		  vh.AddVert(vert);

		  vh.AddTriangle(index + 0, index + 1, index + 2);
		  vh.AddTriangle(index + 2, index + 3, index + 0);
      index += 4;
    };

    Color32 c2 = new Color32(0x00, 0x00, 0x00, 0xff);
    //addQuad(color, 0, 0, 1, 1);
    for (int i = 0; i != 30; ++i) {
      float vx = i * (1.0f/30);
      float vy = Mathf.Sin(vx * 4) * 0.5f + 0.5f;
      addQuad(c2, vx, vy, vx + 0.02f, vy + 0.02f);
    }
	}

/*    Color32 point_colour = new Color32(0x00, 0x00, 0x00, 0x00);
    for (int i = 0; i != 100; ++i) {
      float x = corner1.x + (corner2.x - corner1.x) * i / 100;
      float y = corner1.y + (corner2.x - corner2.y) * i / 100;
      vert.position = new Vector2(corner2.x, corner1.y);
      vert.color = point_colour;
      vbo.Add(vert);
    }
	}*/
}
