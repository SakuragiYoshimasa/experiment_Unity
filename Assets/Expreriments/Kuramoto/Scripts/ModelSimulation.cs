using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModelSimulation : MonoBehaviour {

	[SerializeField, Range(1, 4096)]
	private int _pointNum;
	public int pointNum {
		get { return _pointNum;}
		private set { _pointNum = value;}
	}

	[SerializeField]
	private float _baseFreq;
	public float baseFreq {
		get { return _baseFreq;}
		private set { _baseFreq = value;}
	}

	[SerializeField]
	private float _connectionCoefficient;
	public float connectionCoefficient {
		get { return _connectionCoefficient;}
		private set { _connectionCoefficient = value;}
	}

	[SerializeField] Shader kernelShader;
	[SerializeField] Shader surfaceShader;
	[SerializeField] private Mesh mesh;

	private Material kernelMat;
	private Material surfaceMat;

	private Texture2D naturalFreqBuffer;
	private RenderTexture positionBuffer;
	private RenderTexture velocityBuffer;


	[SerializeField] private Color _color1;
	public Color color1 {
		get{ return _color1;}
		set{ _color1 = value;}
	}

	[SerializeField] private Color _color2;
	public Color color2 {
		get{ return _color2;}
		set{ _color2 = value;}
	}

	[SerializeField, Range(0f, 1.0f)] private float _metallic = 0.5f;
	public float metallic {
		get{ return _metallic;}
		set{ _metallic = value;}
	}

	[SerializeField, Range(0f, 1.0f)] private float _smoothness = 0.5f;
	public float smoothness {
		get{ return _smoothness;}
		set{ _smoothness = value;}
	}

	[SerializeField, Range(0.5f, 2.0f)] private float _radius = 1.0f;
	public float radius {
		get{ return _radius;}
		set{ _radius = value;}
	}


	private float elapsedTime;

	void Start () {
		Initialize ();
	}	

	void Update () {
		elapsedTime += Time.deltaTime;
		kernelMat.SetFloat ("_DeltaTime", Time.deltaTime);
		kernelMat.SetFloat ("_K", connectionCoefficient);
		kernelMat.SetFloat ("_BaseFreq", baseFreq);

		UpdateVelocity ();
		UpdatePosition ();
		DrawMesh ();
	}

	void Initialize(){

		elapsedTime = 0f;
		naturalFreqBuffer = createT2Buffer (pointNum);
		positionBuffer = createRTBuffer (pointNum);
		velocityBuffer = createRTBuffer (pointNum);


		int vNum = mesh.vertexCount;

		Vector2[] uv = new Vector2[vNum];

		for(int i = 0; i < vNum; i++){
			uv [i] = new Vector2 (((float)i + 0.5f)/ (float)vNum, 0.5f);
		}

		mesh.uv = uv;

		InitializeMat ();
		InitializePosition (); 
		InitializeNaturalFreqs (); 
	}

	void InitializeMat(){

		surfaceMat = CreateMaterial (surfaceShader);
		kernelMat = CreateMaterial (kernelShader);
		kernelMat.SetInt ("_PointNum", pointNum);
		kernelMat.SetFloat ("_K", connectionCoefficient);
		kernelMat.SetFloat ("_BaseFreq", baseFreq);
		kernelMat.SetFloat ("_DeltaTime", 0);
	}

	void InitializePosition(){
		
		Graphics.Blit (null, positionBuffer, kernelMat, 0);
		kernelMat.SetTexture ("_PositionTex", positionBuffer);
	}

	void InitializeNaturalFreqs(){

		RandomBoxMuller random = new RandomBoxMuller();

		for(int i = 0; i < pointNum; i++){
			//naturalFreqBuffer.SetPixel (1, i, new Color((float)random.next(0, 2.0, true) * baseFreq * 10.0f, 0, 0, 0));
			naturalFreqBuffer.SetPixel (1, i, new Color((Random.value - 0.5f) * baseFreq , 0, 0, 0));
		}
		naturalFreqBuffer.Apply ();

		kernelMat.SetTexture ("_NaturalFreqTex", naturalFreqBuffer);
		surfaceMat.SetTexture ("_NaturalFreqTex", naturalFreqBuffer);
	}
					
	void UpdateVelocity(){
		Graphics.Blit (null, velocityBuffer, kernelMat, 1);

		kernelMat.SetTexture ("_VelocityTex", velocityBuffer);
	}

	void UpdatePosition(){
		
		Graphics.Blit (null, positionBuffer, kernelMat, 2);

		float[] param = calcParams ();

		kernelMat.SetTexture ("_PositionTex", positionBuffer);
		kernelMat.SetFloat ("_ParamTheta", param[0]);
		kernelMat.SetFloat ("_ParamR", param[1]);
	}

	void DrawMesh(){
		
		surfaceMat.SetColor ("_Color1", color1);
		surfaceMat.SetColor ("_Color2", color2);
		surfaceMat.SetTexture ("_PositionTex", positionBuffer);
		surfaceMat.SetFloat ("_Metallic", metallic);
		surfaceMat.SetFloat ("_Smoothness", smoothness);
		surfaceMat.SetFloat ("_Radius", radius);
		surfaceMat.SetFloat ("_ElapsedTime", elapsedTime);
		surfaceMat.SetFloat ("_BaseFreq", baseFreq);

		Graphics.DrawMesh (mesh, transform.localToWorldMatrix, surfaceMat, gameObject.layer);
	}

	float[] calcParams(){

		Texture2D tex = new Texture2D(positionBuffer.width, positionBuffer.height, TextureFormat.RGBAFloat, false);

		RenderTexture.active = positionBuffer;
		tex.ReadPixels(new Rect(0, 0, positionBuffer.width, positionBuffer.height), 0, 0);
		tex.Apply();

		float real = 0;
		float imag = 0;

		for(int i = 0; i < pointNum; i++){
			float phai = tex.GetPixel (i, 0).r;
			real += Mathf.Cos (phai);
			imag += Mathf.Sin (phai);
		}

		real /= (float)pointNum;
		imag /= (float)pointNum;

		float paramTheta = Mathf.Atan (imag/real);
		float paramR = Mathf.Sqrt (real * real + imag * imag);

		radius = paramR * 0.7f + 0.5f;


		return new float[2]{paramTheta, paramR};
	}
		
	Material CreateMaterial(Shader shader)
	{
		var material = new Material(shader);
		material.hideFlags = HideFlags.DontSave;
		return material;
	}

	RenderTexture createRTBuffer(int size){
		var format = RenderTextureFormat.ARGBFloat;
		var buffer = new RenderTexture(size, 1, 0, format);
		buffer.hideFlags = HideFlags.DontSave;
		buffer.filterMode = FilterMode.Point;
		buffer.wrapMode = TextureWrapMode.Clamp;
		return buffer;
	}

	Texture2D createT2Buffer(int size){

		var texture = new Texture2D(size, 1, TextureFormat.RGBAFloat, false);

		for(int i = 0; i < size; i++){
			texture.SetPixel (i, 1, new Color(0, 0, 0, 0));
		}
		texture.Apply ();
		return texture;
	}
}
