using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class DepthTexture : MonoBehaviour,IPointerClickHandler, IBeginDragHandler,
IDragHandler,IEndDragHandler {
	public Camera rightCamera;
	public Camera leftCamera;

	public RawImage leftScreen;
	public RawImage rightScreen;

	public RenderTexture rightTex;
	public RenderTexture leftTex;

	public Shader shader;

	//GameObject debugCube;

	Color[] rCol;
	Color[] lCol;

	Color[] centerCol;

	Texture2D rightT;
	Texture2D leftT;

	Texture2D tex;

	Vector2 oldMousePos;
	bool joining = false;
	// Use this for initialization
	void Start () {
		tex = new Texture2D (256, 256,TextureFormat.ARGB32,false);
		rightT = new Texture2D (256, 256,TextureFormat.ARGB32,false);
		leftT = new Texture2D (256, 256,TextureFormat.ARGB32,false);
		//debugCube = GameObject.CreatePrimitive (PrimitiveType.Cube);
		//debugCube.transform.localScale = new Vector3 (3, 3, 3);
	}

	void CalculateOptimumPlane(int molNumber)
	{
		BioBlox bioScript = GameObject.Find ("BioBlox").GetComponent<BioBlox> ();
		GameObject molObj = bioScript.molecules [molNumber];

		PDB_molecule mol = molObj.GetComponent<PDB_mesh> ().mol;
		Vector3 axis = molObj.transform.position.normalized;


		Vector3 bestWorldPos = axis * 1000;
		for(int i=0;i<mol.atom_centres.Length;++i)
		{
			Vector3 worldPos=molObj.transform.TransformPoint(mol.atom_centres[i]);
			float dot = Vector3.Dot(worldPos,axis);
			float bestDot = Vector3.Dot(bestWorldPos,axis);
			if(bestDot>dot)
			{
				bestWorldPos=worldPos;
			}
		}
		float radOffset = mol.atom_radii [0];
		if (molNumber==0) {
			Vector3 camPos=leftCamera.transform.position;
			camPos.x = bestWorldPos.x + leftCamera.nearClipPlane + radOffset +1;
			leftCamera.transform.position=camPos;
		}
		if (molNumber==1) {
			Vector3 camPos=rightCamera.transform.position;
			camPos.x = ((bestWorldPos.x - rightCamera.nearClipPlane) - radOffset) -1;
			rightCamera.transform.position=camPos;
		}

	}
	
	// Update is called once per frame
	void Update () {
		CalculateOptimumPlane (0);
		CalculateOptimumPlane (1);
		rightCamera.targetTexture = rightTex;
		rightCamera.RenderWithShader (shader, "");

		leftCamera.targetTexture = leftTex;
		leftCamera.RenderWithShader (shader, "");


		RenderTexture.active=rightTex;
		rightT.ReadPixels(new Rect(0,0,256,256),0,0);
		RenderTexture.active=leftTex;
		leftT.ReadPixels(new Rect(0,0,256,256),0,0);



		RenderTexture.active = null;

		rCol = rightT.GetPixels ();
		lCol = leftT.GetPixels ();
		for (int i=0; i<rCol.Length; ++i) {
			if(rCol[i]!=Color.black)
			{
			rCol[i]= new Color(1.0f - rCol[i].r, 
			               		 1.0f - rCol[i].g,
			                 	 1.0f - rCol[i].b);
			}
			if(lCol[i]!=Color.black)
			{
				lCol[i] = new Color(1.0f - lCol[i].r, 
				                    1.0f - lCol[i].g,
				                    1.0f - lCol[i].b);

			}
		}

		rightT.SetPixels (rCol);
		rightT.Apply ();
		leftT.SetPixels (lCol);
		leftT.Apply ();

		rightScreen.texture = rightT;
		leftScreen.texture = leftT;

		centerCol = new Color[rCol.Length];
		int textureWidth = rightT.width;
		int textureHeight = rightT.height;



		for (int i=0; i<textureHeight; ++i) {

			for(int j=0;j<textureWidth;++j)
			{
				int index = (i*textureHeight)+j;
				int reverseHorizontalIndex = (i*textureHeight) + textureWidth-1-j;
				//Color invert=Color.white-rCol[reverseHorizontalIndex];
				centerCol[index]=rCol[reverseHorizontalIndex]*0.5f + lCol[index]*0.5f;
				centerCol[index].a=1.0f;
			}

		}


	
		tex.SetPixels (centerCol);
		tex.Apply ();
		this.GetComponent<RawImage> ().texture =(Texture) tex;
	}

	IEnumerator JoinCameras()
	{
		joining = true;


		Vector3 rTarget = rightCamera.transform.position;
		rTarget.y = 0;
		rTarget.z = 0;

		Vector3 lTarget = leftCamera.transform.position;
		lTarget.y = 0;
		lTarget.z = 0;

		Vector3 rStart = rightCamera.transform.position;
		Vector3 lStart = leftCamera.transform.position;


		for (float t=0.0f; t<1.0f; t+=Time.deltaTime) {
			rightCamera.transform.position = Vector3.Lerp(
				rStart, rTarget, t);
			leftCamera.transform.position = Vector3.Lerp(
				lStart, lTarget, t);
			yield return null;
		}
		joining = false;
		yield break;
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		if (!joining) {
			StartCoroutine("JoinCameras");
		}
	}
	
	public void OnBeginDrag (PointerEventData eventData)
	{
		oldMousePos = eventData.position;
	}
	
	public void OnDrag (PointerEventData eventData)
	{
		Vector2 mousePos = eventData.position;
		Vector2 delta = mousePos - oldMousePos;
		delta.Normalize ();
		oldMousePos = mousePos;
		rightCamera.transform.Translate (new Vector3 (delta.x, -delta.y,0));
		leftCamera.transform.Translate (new Vector3 (-delta.x, -delta.y, 0));
	}
	
	public void OnEndDrag (PointerEventData eventData)
	{
		
	}
}
