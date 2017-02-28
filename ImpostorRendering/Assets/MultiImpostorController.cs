using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiImpostorController : MonoBehaviour {
    private const float INF = 987654321.0f;

    private List<TextureManager> m_tmList;
    private List<GameObject> m_impostorObjList;
    private Camera mainCam;

    private Texture2D m_cachedTex;
    private List<Vector3> m_currCamDir;
    private List<Vector3> m_prevCamDir;
    private List<float> m_cacheAngleBorder;
    private Vector3 m_startingPos = new Vector3(-21.83f, 2.8f, 21.8f);
    private Vector3 m_endPos = new Vector3(33.68f, 2.8f, 21.8f);
    private int m_slice;
    private int m_rowObjectNum = 30;
    private float m_fallingSpeed = 0.1f;

    [SerializeField]    private GameObject targetObj;
    [SerializeField]    private bool m_AdaptiveResolution = true;
    [SerializeField]    private bool m_ViewAngleUpdate = true;
    [SerializeField]    private bool m_FrustumCulling = true;
    [SerializeField]    private bool m_UseCache = true;
    [SerializeField]    private bool m_FallingObject = true;
    [SerializeField]    private float m_cacheAngleThrehold = 1.0f;
    [SerializeField]    private float m_angleThrehold = 3.0f;
    [SerializeField]    private int m_ObjectNumber = 300;
    [SerializeField]    private positioning m_PositioningMethod = positioning.Uniform; // Uniform / Random / Random Space

    /// <summary>
    /// Default Functions
    /// </summary>
    public MultiImpostorController()
    {
    }
    // Use this for initialization
    void Awake()
    {
        Assign();
        Debug.Log(TextureCacheInfo.textureSize);
    }
    void Start () {
        Init();
    }
	// Update is called once per frame
	void Update () {
        SelectiveUpdate();
        FallingObjects();
        if (Input.GetKeyDown(KeyCode.Alpha1))
            m_AdaptiveResolution = !m_AdaptiveResolution;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            m_ViewAngleUpdate = !m_ViewAngleUpdate;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            m_FrustumCulling = !m_FrustumCulling;
    }

    ///
    /// Member Functions
    /// 
    /// Load/Assign Functions
    private void Assign()
    {
        /// Rendering Switch setting
        if(m_UseCache)
        {
            // when caching is applied, Adaptive Resolution is turned off
            m_AdaptiveResolution = false;
            LoadCacheTex();
        }
        /// GameObject Access
        if (targetObj==null)
            targetObj = GameObject.Find("Skateboard");
        mainCam = Camera.allCameras[0];                     // Get Camera from static camera array

        m_tmList = new List<TextureManager>();
        m_impostorObjList = new List<GameObject>();
        m_currCamDir = new List<Vector3>();
        m_prevCamDir = new List<Vector3>();
        List<Vector3> positionVec3 = new List<Vector3>();

        GeneratePosition(m_ObjectNumber, m_PositioningMethod, ref positionVec3);

        for (int i = 0; i < m_ObjectNumber; i++)
        {
            /// TextureManager List assign
            if (m_AdaptiveResolution)
            {
                /// If Adaptive Resolution switch is on, calculate texture resolution due to distance
                float dist = (positionVec3[i] - mainCam.transform.position).magnitude;
                int tsize = UtilFun.CalculateOptimalResolution(dist, 3, 0.1f);
                m_tmList.Add(new TextureManager(tsize, ref mainCam, targetObj.layer));
            }
            else
                m_tmList.Add(new TextureManager(TextureCacheInfo.textureSize, ref mainCam, targetObj.layer));

            m_impostorObjList.Add(new GameObject("Multi Impost :" + i.ToString()));
            m_impostorObjList[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_impostorObjList[i].transform.position = positionVec3[i];
            SetObjPropertyToImpostor(i);
        }
    }
    private void Init()
    {
        /// Initialize Parameter
        for (int i = 0; i < m_tmList.Count; i++)
        {
            m_currCamDir.Add(new Vector3(0.0f, 0.0f, 0.0f));
            m_currCamDir[i] = mainCam.transform.position - m_impostorObjList[i].transform.position;
            m_prevCamDir.Add(new Vector3(0.0f, 0.0f, 0.0f));
            m_prevCamDir[i] = m_currCamDir[i];
            RenderUpdate(i);
        }
    }
    private void LoadCacheTex()
    {
        /// Load Cache Texture
        m_slice = (int)(TextureCacheInfo.cacheAngleScope * 2 / TextureCacheInfo.cacheAngleRes) + 1;
        m_cachedTex = new Texture2D(TextureCacheInfo.textureSize * m_slice, TextureCacheInfo.textureSize * m_slice, TextureFormat.ARGB32, true);
        String m_FileName = "impostorCacheTex.png";
        byte[] fileData = File.ReadAllBytes(Application.dataPath + "/../" + m_FileName);
        m_cachedTex.LoadImage(fileData);
        m_cacheAngleBorder = new List<float>();
        /// set up cache borders
        for (float x_b = -TextureCacheInfo.cacheAngleScope; x_b <= TextureCacheInfo.cacheAngleScope; x_b += TextureCacheInfo.cacheAngleRes)
            m_cacheAngleBorder.Add(x_b);
        Debug.Log(m_cachedTex.ToString());
    }
    private void SetObjPropertyToImpostor(int i)
    {
        /// Assign Impostor Game Objects
        Shader alphaShader = Shader.Find("Imposter/Transparent");
        m_impostorObjList[i].GetComponent<Renderer>().sharedMaterial.shader = alphaShader;
        m_impostorObjList[i].GetComponent<MeshRenderer>().material.mainTexture = m_tmList[i].GetTexture();
        m_impostorObjList[i].GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(0.0f, 0.0f);
        m_impostorObjList[i].GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.0f, 1.0f); ;
        m_impostorObjList[i].GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m_impostorObjList[i].GetComponent<MeshRenderer>().enabled = true;
        m_impostorObjList[i].transform.localScale = new Vector3(3, 3, 0.001f);
    }
    private void SetObjPropertyToTexCache(int i, int i_cache, int j_cache)
    {
        m_impostorObjList[i].GetComponent<Renderer>().sharedMaterial.mainTexture = m_cachedTex;
        m_impostorObjList[i].GetComponent<Renderer>().sharedMaterial.mainTextureOffset = new Vector2(1.0f / m_slice * (float)i_cache, 1.0f / m_slice * (float)j_cache);
        m_impostorObjList[i].GetComponent<Renderer>().sharedMaterial.mainTextureScale = new Vector2(1.0f / m_slice, 1.0f / m_slice);
    }
    private bool SelectiveUpdate()
    {
        /// Select object due to View angle difference / Visibility
        /// Determine rendering update due to direction change
        for (int i = 0; i < m_tmList.Count; i++)
        {
            if (m_ViewAngleUpdate && m_FrustumCulling && IsAngleChanged(i) && IsInFrustum(i))
                RenderUpdate(i);
            else if (!m_ViewAngleUpdate && m_FrustumCulling && IsInFrustum(i))
                RenderUpdate(i);
            else if (m_ViewAngleUpdate && !m_FrustumCulling && IsAngleChanged(i))
                RenderUpdate(i);
            else
                RenderUpdate(i);
        }
        return false;
    }
    private void RenderUpdate(int i)
    {
        /// select target obj and rendering update
        Vector3 viewDir = (mainCam.transform.position - m_impostorObjList[i].transform.position).normalized;
        /// determine for using cache or not
        int i_cache=0, j_cache=0;
        if (m_UseCache && IsHitCacheTex(viewDir, ref i_cache, ref j_cache))
        {
            // Use texture cache
            SetObjPropertyToTexCache(i, i_cache, j_cache);
        }
        else
        {
            // change to dynamic impostor
            m_impostorObjList[i].GetComponent<MeshRenderer>().material.mainTexture = m_tmList[i].GetTexture();
            Vector3 pos = targetObj.transform.position + viewDir;
            m_tmList[i].UpdateTexCam(pos, targetObj.transform);
            SetObjPropertyToImpostor(i);
        }
        m_impostorObjList[i].transform.LookAt(mainCam.transform);
        m_impostorObjList[i].transform.Rotate(new Vector3(0, 1.0f, 0.0f), 180.0f);   /// Scene setting is flipped
    }
    //
    // Position Calculation / Update Functions
    private void FallingObjects()
    {
        /// Moving Objects
        if (m_FallingObject)
        {
            for (int i = 0; i < m_impostorObjList.Count; i++)
            {
                m_impostorObjList[i].transform.Translate(0.0f, -m_fallingSpeed, 0.0f);
                if (m_impostorObjList[i].transform.position.y < 2.8f)
                    m_impostorObjList[i].transform.position = new Vector3(m_impostorObjList[i].transform.position.x, 10.0f, m_impostorObjList[i].transform.position.z);
            }
        }
    }
    private int GeneratePosition(int _number, positioning _method, ref List<Vector3> _positionList)
    {
        /// Generate multiple objects position due to policy : Uniform/ Random2D / Random3D
        switch (_method)
        {
            case positioning.Uniform:
                {
                    Vector3 x_bias = new Vector3((float)(m_endPos.x - m_startingPos.x) / (float)m_rowObjectNum, 0.0f, 0.0f);
                    Vector3 z_bias = new Vector3(0.0f, 0.0f, (float)(m_endPos.x - m_startingPos.x) / (float)m_rowObjectNum);

                    for (int i = 0; i < _number; i++)
                    {
                        int column = i / m_rowObjectNum;
                        int row = i % m_rowObjectNum;
                        _positionList.Add(m_startingPos + x_bias * row + z_bias * column);
                    }
                    return 1;
                }
            case positioning.Random2D:
                {
                    Vector3 x_scale = new Vector3(m_endPos.x - m_startingPos.x, 0.0f, 0.0f);
                    Vector3 z_scale = new Vector3(0.0f, 0.0f, m_endPos.x - m_startingPos.x);
                    System.Random rand = new System.Random();

                    for (int i = 0; i < _number; i++)
                    {
                        float x_rand = ((float)rand.Next()) / ((float)Int32.MaxValue);
                        float z_rand = ((float)rand.Next()) / ((float)Int32.MaxValue);
                        _positionList.Add(m_startingPos + x_scale * x_rand + z_scale * z_rand);
                    }
                    return 1;
                }
            case positioning.Random3D:
                {
                    Vector3 x_scale = new Vector3(m_endPos.x - m_startingPos.x, 0.0f, 0.0f);
                    Vector3 y_scale = new Vector3(0.0f, 10, 0.0f);
                    Vector3 z_scale = new Vector3(0.0f, 0.0f, m_endPos.x - m_startingPos.x);
                    System.Random rand = new System.Random();
                    for (int i = 0; i < _number; i++)
                    {
                        float x_rand = ((float)rand.Next()) / ((float)Int32.MaxValue);
                        float y_rand = ((float)rand.Next()) / ((float)Int32.MaxValue);
                        float z_rand = ((float)rand.Next()) / ((float)Int32.MaxValue);
                        _positionList.Add(m_startingPos + x_scale * x_rand + y_scale * y_rand + z_scale * z_rand);
                    }
                    return 1;
                }
        }
        return 0;
    }
    //
    // test functions
    private bool IsHitCacheTex(Vector3 dir, ref int i_cache, ref int j_cache)
    {
        /// determine texture cache hit due to dir vector3
        /// Calculate x axis angle, y axis angle
        float angleX = Vector3.Angle(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, dir.y, dir.z));
        float angleY = Vector3.Angle(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(dir.x, 0.0f, dir.z));
        if (Vector3.Dot(dir, new Vector3(0.0f, 1.0f, 0.0f)) < 0)
            angleX = -angleX;
        if (Vector3.Dot(dir, new Vector3(1.0f, 0.0f, 0.0f)) > 0)
            angleY = -angleY;

        i_cache = 0;
        j_cache = 0;
        float minDistX = 9999.0f, minDistY = 9999.0f; ;
        for (int i = 0; i < m_slice; i++)
        {
            if (Mathf.Abs(m_cacheAngleBorder[i] - angleX) < minDistX)
            {
                i_cache = i;
                minDistX = Mathf.Abs(m_cacheAngleBorder[i] - angleX);
            }
        }
        for (int i = 0; i < m_slice; i++)
        {
            if (Mathf.Abs(m_cacheAngleBorder[i] - angleY) < minDistY)
            {
                j_cache = i;
                minDistY = Mathf.Abs(m_cacheAngleBorder[i] - angleY);
            }
        }
        if (minDistX < m_cacheAngleThrehold && minDistY < m_cacheAngleThrehold)
            return true;
        else
            return false;
    }
    private bool IsAngleChanged(int i)
    {
        /// Check if view angle change is exceed threshold
        m_currCamDir[i] = mainCam.transform.position - m_impostorObjList[i].transform.position;
        bool ret = (Vector3.Angle(m_currCamDir[i].normalized, m_prevCamDir[i].normalized) > m_angleThrehold);
        if (ret)
            m_prevCamDir[i] = m_currCamDir[i];
        return ret;
    }
    private bool IsInFrustum( int _index)
    {
        /// Check if _index object is in Frustum
       return UtilFun.IsRendererInCamera(m_impostorObjList[_index].GetComponent<Renderer>(), mainCam);
    }
}
