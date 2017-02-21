using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LennardJonesGraph : Graphic 
{
  delegate void AddQuad(Color32 c, float x0, float y0, float x1, float y1);

  public List<float> points;
  public float point_size = 0.01f;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
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

    if (points != null) {
      for (int i = 0; i != points.Count/2; ++i) {
        float vx = points[i*2+0];
        float vy = points[i*2+1];
        addQuad(color, vx - point_size, vy - point_size, vx + point_size, vy + point_size);
      }
    }
	}
}
