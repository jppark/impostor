using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum positioning
{
    Uniform, Random2D, Random3D 
};

public class MultiImpostorController : MonoBehaviour {

    private List<TextureManager> tmList;
    private List<GameObject> impostorObjList;
    private Dictionary<int, RenderTexture> impostorCache;
    private GameObject targetObj;
    private Camera mainCam;

    private List<Vector3> m_currCamDir;
    private List<Vector3> m_prevCamDir;
    private Vector3 m_startingPos = new Vector3(-21.83f, 2.8f, 21.8f);
    private Vector3 m_endPos = new Vector3(33.68f, 2.8f, 21.8f);
    private int m_targetRenderLayer = 8;
    private int m_defaultTexSize = 512;

    [SerializeField]    private bool m_AdaptiveResolution = true;
    [SerializeField]    private bool m_AdaptiveUpdate = true;
    [SerializeField]    private bool m_FrustumCulling = true;
    [SerializeField]    private bool m_UseCacheing = true;
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
            m_AdaptiveUpdate = !m_AdaptiveUpdate;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            m_FrustumCulling = !m_FrustumCulling;
    }
    ///
    /// Member Function
    /// 
    private void Assign()
    {
        /// GameObject Access
        targetObj = GameObject.Find("Skateboard");
        mainCam = Camera.allCameras[0];                     // Get Camera

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
            if (m_AdaptiveResolution)
            {
                float dist = (positionVec3[i] - mainCam.transform.position).magnitude;
                int tsize = UtilFun.CalculateOptimalResolution(dist, 3, 0.1f);
                tmList.Add(new TextureManager(tsize, m_targetRenderLayer, ref mainCam));
            }
            else
                tmList.Add(new TextureManager(m_defaultTexSize, m_targetRenderLayer, ref mainCam));

            impostorObjList.Add(new GameObject( "MultiImp "+i.ToString() ));
            impostorObjList[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Shader alphaShader = Shader.Find("Imposter/Transparent");
            impostorObjList[i].GetComponent<Renderer>().sharedMaterial.shader = alphaShader;
            impostorObjList[i].GetComponent<MeshRenderer>().material.mainTexture = tmList[i].GetTexture();
            impostorObjList[i].GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            impostorObjList[i].GetComponent<MeshRenderer>().enabled = true;
            impostorObjList[i].transform.position = positionVec3[i];
            impostorObjList[i].transform.localScale = new Vector3(3, 3, 0.001f);
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
    private int GeneratePosition(int _number, positioning _method, ref List<Vector3> _positionList)
    {
        switch(_method)
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
                        _positionList.Add(m_startingPos + x_scale * x_rand + y_scale*y_rand + z_scale * z_rand);
                    }
                    return 1;
                }
        }
        return 0;
    }
    private void RenderUpdate( int i )
    {
        Vector3 pos = targetObj.transform.position + (mainCam.transform.position - impostorObjList[i].transform.position).normalized;
        tmList[i].UpdateTexCam(pos, targetObj.transform);
        //tmList[i].SetRenderTexture( tmList[0].GetTexture() );
        impostorObjList[i].transform.LookAt(mainCam.transform);
        impostorObjList[i].transform.Rotate(new Vector3(0, 1.0f, 0.0f), 180.0f);   /// Scene setting is flipped
    }
    private bool SelectiveUpdate()
    {
        int count = 0;
        /// Determine rendering update due to direction change
        if (!m_AdaptiveUpdate)
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
    private bool IsInFrustum( int _index)
    {
       return UtilFun.IsRendererInCamera(impostorObjList[_index].GetComponent<Renderer>(), mainCam);
    }
}
