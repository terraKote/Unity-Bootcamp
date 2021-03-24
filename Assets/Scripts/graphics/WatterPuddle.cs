using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WatterPuddle : MonoBehaviour
{
    /// <summary>
    /// Private Data
    /// </summary>
	public bool m_DisablePixelLights = true;
	public int m_TextureSize = 256;
	public float m_ClipPlaneOffset = 0.07f;
	
	public LayerMask m_ReflectLayers = -1;

    public Transform transform1;
    public Transform transform2;

    public Collider soldierCollider;
    
    public float timeWave = 2.0f;
    public float scaleWave = 2.0f;
    public float scaleStart = 0.1f;

    public GameObject particleWatter;
	
	
    
    /// <summary>
    /// Public Data.
    /// </summary>
    private Hashtable m_ReflectionCameras = new Hashtable(); // Camera -> Camera table

    private RenderTexture m_ReflectionTexture = null;
    private int m_OldReflectionTextureSize = 0;

    private float wave1Scale;
    private float wave2Scale;
    private float wave1Alpha;
    private float wave2Alpha;

    //If true, update scale and alpha.
    private bool wave1Update;
    private bool wave2Update;

    //Coef of add scale transition;
    private float coefWaterScale;
   
    //Record position when transform collide water.
    private Vector3 positionTransf1;
    private Vector3 positionTransf2;
   
    private float _currentWave1Time;
    private float _currentWave2Time;

    private Texture2D mainTex;



    /// <summary>
    /// MonoBehaviour Start.
    /// </summary>
    void Start()
    {
        wave1Alpha = 0;
        wave2Alpha = 0;

        wave1Scale = 0;
        wave2Scale = 0;

        coefWaterScale = (scaleWave - (scaleStart * scaleWave));

        _currentWave1Time = 0;
        _currentWave2Time = 0;

        Physics.IgnoreCollision(transform1.collider, soldierCollider);
        Physics.IgnoreCollision(transform2.collider, soldierCollider);

        mainTex = ((Texture2D)renderer.sharedMaterial.mainTexture);
    }


    /// <summary>
    /// MonoBehaviour Update.
    /// </summary>
    void Update()
    {
        //Set values in shader.
        renderer.sharedMaterial.SetVector("_DataTransf1", new Vector4(positionTransf1.x, wave1Alpha, positionTransf1.z, wave1Scale));
        renderer.sharedMaterial.SetVector("_DataTransf2", new Vector4(positionTransf2.x, wave2Alpha, positionTransf2.z, wave2Scale));


        if (wave1Update)
        {
            wave1Scale += (Time.deltaTime / timeWave) * coefWaterScale;
            if (wave1Scale > scaleWave)
            {

                wave2Scale = scaleWave;
            }
            _currentWave1Time += Time.deltaTime;

            wave1Alpha = 1.0f - EaseOut(_currentWave1Time, 0, 1, timeWave);
            if (wave1Alpha < 0)
            {
                wave1Alpha = 0;
                wave1Update = false;

                wave1Scale = 0;
            }
        }

        if (wave2Update)
        {
            wave2Scale += (Time.deltaTime / timeWave) * coefWaterScale;
            if (wave2Scale > scaleWave)
            {
                wave2Scale = scaleWave;
            }

            _currentWave2Time += Time.deltaTime;

            wave2Alpha = 1.0f - EaseOut(_currentWave2Time, 0, 1, timeWave);
            if (wave2Alpha < 0)
            {
                wave2Alpha = 0;
                wave2Update = false;

                wave2Scale = 0;
            }
        }
    }
    
    /// <summary>
    /// Ease Out.
    /// </summary>
    public float EaseOut(float t, float b, float c, float d)
    {
        return c * ((t = t / d - 1) * t * t + 1) + b;
    }


    /// <summary>
    /// Test collision transforms with collider water.
    /// </summary>
    void OnTriggerEnter(Collider p_collider)
    {
        //If first collider enter.
        if (p_collider.name == transform1.name)
        {
            wave1Scale = scaleStart * scaleWave;
            
            _currentWave1Time = 0.0f;

            positionTransf1 = transform1.position;

            wave1Update = true;

            RaycastHit hit1;
            if (Physics.Raycast(transform1.position + new Vector3(0, 1, 0), Vector3.down, out hit1, 4, ~LayerMask.NameToLayer("watterPuddle")))
            {
                if (mainTex.GetPixelBilinear(hit1.textureCoord.x, hit1.textureCoord.y).a > 0.1f)
                {
                    ParticleEmitter emitter = ((GameObject)GameObject.Instantiate(particleWatter)).GetComponent<ParticleEmitter>();
                    emitter.transform.position = transform1.position;
                }
            }

            
        }
        //If second collider enter.
        if (p_collider.name == transform2.name)
        {
            wave2Scale = scaleStart * scaleWave;

            _currentWave2Time = 0.0f;
            
            positionTransf2 = transform2.position;

            wave2Update = true;

            RaycastHit hit2;
            if (Physics.Raycast(transform2.position + new Vector3(0, 1, 0), Vector3.down, out hit2, 4, ~LayerMask.NameToLayer("watterPuddle")))
            {
                if (mainTex.GetPixelBilinear(hit2.textureCoord.x, hit2.textureCoord.y).a > 0.1f)
                {
                    ParticleEmitter emitter = ((GameObject)GameObject.Instantiate(particleWatter)).GetComponent<ParticleEmitter>();
                    emitter.transform.position = transform2.position;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
	public void OnWillRenderObject()
	{
		if( !enabled || !renderer || !renderer.sharedMaterial || !renderer.enabled )
			return;
			

	    //Add tha base camera.
		Camera cam = Camera.current;
		if( !cam )
			return;
	
	    Camera reflectionCamera;
		CreateWaterObjects( cam, out reflectionCamera);
		
		// find out the reflection plane: position and normal in world space
		Vector3 pos = transform.position;
		Vector3 normal = transform.up;
		
		// Optionally disable pixel lights for reflection/refraction
		int oldPixelLightCount = QualitySettings.pixelLightCount;
		if( m_DisablePixelLights )
			QualitySettings.pixelLightCount = 0;
		
		UpdateCameraModes( cam, reflectionCamera );
		
		// Reflect camera around reflection plane
		float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
		Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);
	
		Matrix4x4 reflection = Matrix4x4.zero;
		CalculateReflectionMatrix (ref reflection, reflectionPlane);
		Vector3 oldpos = cam.transform.position;
		Vector3 newpos = reflection.MultiplyPoint( oldpos );
		reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
	
		// Setup oblique projection matrix so that near plane is our reflection
		// plane. This way we clip everything below/above it for free.
		Vector4 clipPlane = CameraSpacePlane( reflectionCamera, pos, normal, 1.0f );
		Matrix4x4 projection = cam.projectionMatrix;
		CalculateObliqueMatrix (ref projection, clipPlane);
		reflectionCamera.projectionMatrix = projection;
		
		reflectionCamera.cullingMask = ~(1<<4) & m_ReflectLayers.value; // never render water layer
		reflectionCamera.targetTexture = m_ReflectionTexture;
		GL.SetRevertBackfacing (true);
		reflectionCamera.transform.position = newpos;
		Vector3 euler = cam.transform.eulerAngles;
		reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
		
		reflectionCamera.Render();
		
    	reflectionCamera.transform.position = oldpos;
		GL.SetRevertBackfacing (false);
		renderer.sharedMaterial.SetTexture( "_ReflectionTex", m_ReflectionTexture );
		
		// Restore pixel light count
		if( m_DisablePixelLights )
			QualitySettings.pixelLightCount = oldPixelLightCount;
	}
	
	
	/// <summary>
    /// Cleanup all the objects we possibly have created.
	/// </summary>
    void OnDisable()
	{
		if( renderer )
		{
			Material mat = renderer.sharedMaterial;
			if( mat )
			{
				mat.SetTexture( "_ReflectionTex", null );
			}
		}
		if( m_ReflectionTexture ) 
        {
			DestroyImmediate( m_ReflectionTexture );
			m_ReflectionTexture = null;
		}
		foreach( DictionaryEntry kvp in m_ReflectionCameras )
        	DestroyImmediate( ((Camera)kvp.Value).gameObject );
        m_ReflectionCameras.Clear();
	}
	

    /// <summary>
    /// 
    /// </summary>
	private void UpdateCameraModes( Camera src, Camera dest )
	{
		if( dest == null )
			return;
		// set water camera to clear the same way as current camera
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;		
		if( src.clearFlags == CameraClearFlags.Skybox )
		{
			Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
			if( !sky || !sky.material )
			{
				mysky.enabled = false;
			}
			else
			{
				mysky.enabled = true;
				mysky.material = sky.material;
			}
		}

		// update other values to match current camera.
		// even if we are supplying custom camera&projection matrices,
		// some of values are used elsewhere (e.g. skybox uses far plane)
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}
	
	// On-demand create any objects we need for water
	private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera)
	{
		reflectionCamera = null;
		
		// Reflection render texture
		if( !m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize )
		{
			if( m_ReflectionTexture )
				DestroyImmediate( m_ReflectionTexture );
			m_ReflectionTexture = new RenderTexture( m_TextureSize, m_TextureSize, 16 );
			m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
			m_ReflectionTexture.isPowerOfTwo = true;
			m_ReflectionTexture.hideFlags = HideFlags.DontSave;
			m_OldReflectionTextureSize = m_TextureSize;
		}
		
		// Camera for reflection
		reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
		if( !reflectionCamera ) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
		{
			GameObject go = new GameObject( "Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox) );
			reflectionCamera = go.camera;
			reflectionCamera.enabled = false;
			reflectionCamera.transform.position = transform.position;
			reflectionCamera.transform.rotation = transform.rotation;
			reflectionCamera.gameObject.AddComponent("FlareLayer");
			go.hideFlags = HideFlags.HideAndDontSave;
			m_ReflectionCameras[currentCamera] = reflectionCamera;
		}
	}
	
	
	// Extended sign: returns -1, 0 or 1 based on sign of a
	private static float sgn(float a)
	{
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
	}
	
	// Given position/normal of the plane, calculates plane in camera space.
	private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint( offsetPos );
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
	}
	
	// Adjusts the given projection matrix so that near plane is the given clipPlane
	// clipPlane is given in camera space. See article in Game Programming Gems 5.
	private static void CalculateObliqueMatrix (ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 q;  
        q.x = (sgn(clipPlane.x) + projection[8]) / projection[0];
        q.y = (sgn(clipPlane.y) + projection[9]) / projection[5];
        q.z = -1.0F;
        q.w = (1.0F + projection[10]) / projection[14];
        
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
        
        projection[2] = c.x;
        projection[6] = c.y;
        projection[10] = c.z + 1.0F;
        projection[14] = c.w;
	}

	// Calculates reflection matrix around the given plane
	private static void CalculateReflectionMatrix (ref Matrix4x4 reflectionMat, Vector4 plane)
	{
	    reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
	    reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
	    reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
	    reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);

	    reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
	    reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
	    reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
	    reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);
	
    	reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
    	reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
    	reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
    	reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);

    	reflectionMat.m30 = 0F;
    	reflectionMat.m31 = 0F;
    	reflectionMat.m32 = 0F;
    	reflectionMat.m33 = 1F;
	}

    
    /// <summary>
    /// 
    /// </summary>
    private void Hit(RaycastHit p_hit)
    {
        if (mainTex.GetPixelBilinear(p_hit.textureCoord.x, p_hit.textureCoord.y).a > 0.1f)
        {
            ParticleEmitter emitter = ((GameObject)GameObject.Instantiate(particleWatter)).GetComponent<ParticleEmitter>();
            emitter.transform.position = p_hit.point;
        }
    }
}
