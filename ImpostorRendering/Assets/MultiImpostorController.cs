using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum positioning
{
    Uniform, Random2D, Random3D 
};
public class MultiImpostorController : MonoBehaviour {
    private const float INF = 987654321.0f;

    private List<TextureManager> tmList;
    private List<GameObject> impostorObjList;
    private List<List<Texture2D>> impostorCacheTexList;
    private Camera mainCam;

    
    

    private List<Vector3> m_currCamDir;
    private List<Vector3> m_prevCamDir;
    private Vector3 m_startingPos = new Vector3(-21.83f, 2.8f, 21.8f);
    private Vector3 m_endPos = new Vector3(33.68f, 2.8f, 21.8f);

    private Texture2D m_BigTex;
    private float m_cacheAngleRes = 20.0f;
    private float m_cacheXAngle = 40.0f;
    private float m_cacheYAngle = 40.0f;

    private int m_targetRenderLayer = 8;
    private int m_rendertexSize = 512;
    private int m_row;
    private int m_column;

    [SerializeField]    private GameObject targetObj;
    [SerializeField]    private bool m_AdaptiveResolution = true;
    [SerializeField]    private bool m_ViewAngleUpdate = true;
    [SerializeField]    private bool m_FrustumCulling = true;
    [SerializeField]    private bool m_UseCache = true;
    [SerializeField]    private bool m_FallingObject = true;
    [SerializeField]    private float m_angleThrehold = 3.0f;
    [SerializeField]    private int m_ObjectNumber = 300;
    [SerializeField]    private positioning m_PositioningMethod = positioning.Uniform; // Uniform / Random / Random Space

    public MultiImpostorController()
    {
    }
    // Use this for initialization
    void Awake()
    {
        Assign();
    }
    void Start () {
        Init();
    }
	// Update is called once per frame
	void Update () {
        SelectiveUpdate();
        if (Input.GetKeyDown(KeyCode.Alpha1))
            m_AdaptiveResolution = !m_AdaptiveResolution;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            m_ViewAngleUpdate = !m_ViewAngleUpdate;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            m_FrustumCulling = !m_FrustumCulling;
    }

///
/// Member Function
/// 
private void Assign()
    {
        /// Rendering Switch setting
        if(m_UseCache)
        {
            m_AdaptiveResolution = false;
            m_ViewAngleUpdate = false;
            impostorCacheTexList = new List<List<Texture2D>>();
            LoadCacheTex();
            SplitCacheTextToList();
        }
        /// GameObject Access
        if (targetObj==null)
            targetObj = GameObject.Find("Skateboard");
        mainCam = Camera.allCameras[0];                     // Get Camera from static camera array
        /// Assign Texture Manager
        tmList = new List<TextureManager>();
        impostorObjList = new List<GameObject>();
        /// Create Multiple Object
        List<Vector3> positionVec3 = new List<Vector3>();
        m_currCamDir = new List<Vector3>();
        m_prevCamDir = new List<Vector3>();

        GeneratePosition(m_ObjectNumber, m_PositioningMethod, ref positionVec3);
        
        for (int i = 0; i < m_ObjectNumber; i++)
        {
            /// TextureManager List assign
            if (m_AdaptiveResolution)
            {
                /// If Adaptive Resolution switch is on, calculate texture resolution due to distance
                float dist = (positionVec3[i] - mainCam.transform.position).magnitude;
                int tsize = UtilFun.CalculateOptimalResolution(dist, 3, 0.1f);
                tmList.Add(new TextureManager(tsize, m_targetRenderLayer, ref mainCam));
            }
            else
                tmList.Add(new TextureManager(m_rendertexSize, m_targetRenderLayer, ref mainCam));
            ImpostorObjAssign(i, ref positionVec3);
        }



        ////////////////test
        int i_out=0, j_out=0;
        Vector3 viewDir = new Vector3(0.0f, 0.0f, -1.0f);
        for (float x_angle = -m_cacheXAngle; x_angle <= m_cacheXAngle; x_angle += m_cacheAngleRes)
        {
            //float y_angle = 20.0f;
            for (float y_angle = -m_cacheYAngle; y_angle <= m_cacheYAngle; y_angle += m_cacheAngleRes)
            {

                Vector3 dir = Quaternion.Euler(x_angle, y_angle, 0.0f) * viewDir;
                IsHitCacheTex(dir, ref i_out, ref j_out);
                Debug.Log(x_angle + ", " + y_angle + " -> " + i_out + "," + j_out);
            }
        }
    }
    private void Init()
    {
        /// Initialize Parameter
        for (int i = 0; i < tmList.Count; i++)
        {
            m_currCamDir.Add(new Vector3(0.0f, 0.0f, 0.0f));
            m_currCamDir[i] = mainCam.transform.position - impostorObjList[i].transform.position;
            m_prevCamDir.Add(new Vector3(0.0f, 0.0f, 0.0f));
            m_prevCamDir[i] = m_currCamDir[i];
            RenderUpdate(i);
        }
    }
    private void LoadCacheTex()
    {
        /// Load Cache Texture
        m_rendertexSize = 512;
        m_row = (int)(m_cacheXAngle * 2 / m_cacheAngleRes) + 1;
        m_column = (int)(m_cacheYAngle * 2 / m_cacheAngleRes) + 1;
        m_BigTex = new Texture2D(m_rendertexSize* m_row, m_rendertexSize* m_column, TextureFormat.ARGB32, true);
        String m_FileName = "impostorCacheTex.png";
        byte[] fileData = File.ReadAllBytes(Application.dataPath + "/../" + m_FileName);
        m_BigTex.LoadImage(fileData);
    }
    private void SplitCacheTextToList()
    {
        /// Split cache texture to texture list
        for(int r=0; r< m_row; r++)
        {
            impostorCacheTexList.Add(new List<Texture2D>() );
            for(int c=0; c<m_column; c++)
            {
                impostorCacheTexList[r].Add(new Texture2D(m_rendertexSize, m_rendertexSize, TextureFormat.ARGB32, true));
                for (int i = 0; i < m_rendertexSize; i++)
                    for (int j = 0; j < m_rendertexSize; j++)
                    {
                        impostorCacheTexList[r][c].SetPixel(i, j, m_BigTex.GetPixel( (i + m_rendertexSize * r), (j + m_rendertexSize * c)));
                     }
            }
        }
    }
    private void ImpostorObjAssign(int i, ref List<Vector3> positionVec3)
    {
        /// Assign Impostor Objects
        impostorObjList.Add(new GameObject("MultiImp " + i.ToString()));
        impostorObjList[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Shader alphaShader = Shader.Find("Imposter/Transparent");
        impostorObjList[i].GetComponent<Renderer>().sharedMaterial.shader = alphaShader;
        impostorObjList[i].GetComponent<MeshRenderer>().material.mainTexture = tmList[i].GetTexture();
        impostorObjList[i].GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        impostorObjList[i].GetComponent<MeshRenderer>().enabled = true;
        impostorObjList[i].transform.position = positionVec3[i];
        impostorObjList[i].transform.localScale = new Vector3(3, 3, 0.001f);
    }
    private bool SelectiveUpdate()
    {
        /// Select object due to View angle difference / Visiblity
        if (m_FallingObject)
        {
            for (int i = 0; i < impostorObjList.Count; i++)
            {
                impostorObjList[i].transform.Translate(0.0f, -0.1f, 0.0f);
                if (impostorObjList[i].transform.position.y < 2.8f)
                    impostorObjList[i].transform.position = new Vector3(impostorObjList[i].transform.position.x, 10.0f, impostorObjList[i].transform.position.z);
            }
        }
        int count = 0;
        /// Determine rendering update due to direction change
        if (!m_ViewAngleUpdate)
        {
            for (int i = 0; i < tmList.Count; i++)
            {
                if (!m_FrustumCulling)
                    RenderUpdate(i);
                else if (IsInFrustum(i))
                {
                    RenderUpdate(i);
                    count++;
                }
            }
        }
        else
        {
            for (int i = 0; i < impostorObjList.Count; i++)
            {
                m_currCamDir[i] = mainCam.transform.position - impostorObjList[i].transform.position;
                if (Vector3.Angle(m_currCamDir[i].normalized, m_prevCamDir[i].normalized) > m_angleThrehold)
                {
                    m_prevCamDir[i] = m_currCamDir[i];
                    if (!m_FrustumCulling)
                        RenderUpdate(i);
                    else if (IsInFrustum(i))
                    {
                        RenderUpdate(i);
                        count++;
                    }
                }
            }
        }
        //Debug.Log("Hit Ratio : " + ( (float)count) / ((float)impostorObjList.Count));
        return false;
    }
    private void RenderUpdate(int i)
    {
        /// select target obj and rendering update
        Vector3 pos = targetObj.transform.position + (mainCam.transform.position - impostorObjList[i].transform.position).normalized;
        Vector3 dir = (mainCam.transform.position - impostorObjList[i].transform.position).normalized;
        /// determine for using cache or not
        int i_cache=0, j_cache=0;
        //if(m_UseCache && IsHitCacheTex(dir, ref i_cache, ref j_cache))
        //    impostorObjList[i].GetComponent<Renderer>().material.SetTexture("mainTex", impostorCacheTexList[i_cache][j_cache] );
        //else
            tmList[i].UpdateTexCam(pos, targetObj.transform);

        impostorObjList[i].transform.LookAt(mainCam.transform);
        impostorObjList[i].transform.Rotate(new Vector3(0, 1.0f, 0.0f), 180.0f);   /// Scene setting is flipped
    }
    private bool IsHitCacheTex( Vector3 dir, ref int i_cache, ref int j_cache )
    {
        /// determine texture cache hit due to dir vector3
        /// Calculate x axis angle, y axis angle
        float angleX = Vector3.Angle(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, dir.y, dir.z));//
        float angleY = Vector3.Angle(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(dir.x, 0.0f, dir.z));//
        if (Vector3.Dot(dir, new Vector3(0.0f, 1.0f, 0.0f)) < 0)
            angleX = -angleX;
        if (Vector3.Dot(dir, new Vector3(1.0f, 0.0f, 0.0f)) > 0)
            angleY = -angleY;

        Debug.Log(angleX + " / " + angleY);

        int angleX_divide = (int)(angleX / m_cacheAngleRes+0.5);
        int angleY_divide = (int)(angleY / m_cacheAngleRes+0.5);

        if ( ( angleX - angleX_divide * m_cacheAngleRes < m_cacheAngleRes)
            && (angleY - angleY_divide * m_cacheAngleRes < m_cacheAngleRes) )
        {
            i_cache = angleX_divide + (int)m_cacheXAngle/ (int)m_cacheAngleRes;
            j_cache = angleY_divide + (int)m_cacheYAngle / (int)m_cacheAngleRes;
            return true;
        }
        return false;
    }
    private int GeneratePosition(int _number, positioning _method, ref List<Vector3> _positionList)
    {
        /// Generate multiple objects position due to policy : Uniform/ Random2D / Random3D
        switch (_method)
        {
            case positioning.Uniform:
                {
                    int rowNum = 30;
                    Vector3 x_bias = new Vector3((float)(m_endPos.x - m_startingPos.x) / (float)rowNum, 0.0f, 0.0f);
                    Vector3 z_bias = new Vector3(0.0f, 0.0f, (float)(m_endPos.x - m_startingPos.x) / (float)rowNum);

                    for (int i = 0; i < _number; i++)
                    {
                        int column = i / rowNum;
                        int row = i % rowNum;
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
    private bool IsInFrustum( int _index)
    {
        /// Check if _index object is in Frustum
       return UtilFun.IsRendererInCamera(impostorObjList[_index].GetComponent<Renderer>(), mainCam);
    }
}
