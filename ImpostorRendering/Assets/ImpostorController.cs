using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Class for Impostor Control
/// </summary>
public class ImpostorController : MonoBehaviour {
    /// <summary>
    /// Data Declaration
    /// Private Data
    /// </summary>
    private TextureManager m_tm;
    private Camera m_mainCam;
    private Vector3 m_currCamDir;
    private Vector3 m_prevCamDir;

    private double m_renderUpdate;
    private double m_totalUpdate;

    [SerializeField]    private GameObject targetDrawObj;
    [SerializeField]    private float m_angleThrehold = 3.0f;
    [SerializeField]    private int m_textureSize = 512;

    /// <summary>
    ///  Default Functions
    /// </summary>
    void Awake()
    {
        Assign();
    }
    void Start()
    {
        Init();
    }
    /// Update is called once per frame
    void Update()
    {
        if (isUpdate())
        {
            UpdateTexCamera();
            m_prevCamDir = m_currCamDir;
            m_renderUpdate += 1.0;
        }
        m_totalUpdate += 1.0;
        //Debug.Log("Update Ratio : " + (m_renderUpdate / m_totalUpdate) + " / " + m_totalUpdate);
        transform.LookAt(m_mainCam.transform);
        transform.Rotate(new Vector3(0, 1.0f, 0.0f), 180.0f);   /// Scene setting is flipped
    }
    ///
    /// member function
    private void Assign()
    {
        /// GameObject Access
        if(targetDrawObj==null)
            targetDrawObj = GameObject.Find("Skateboard");
        m_mainCam = Camera.allCameras[0];                     // Get Camera from static camera array
        /// Assign Texture Manager
        m_tm = new TextureManager(m_textureSize, ref m_mainCam, targetDrawObj.layer );
        GetComponent<Renderer>().material.mainTexture = m_tm.GetTexture();
    }
    private void Init()
    {
        /// Initialize Parameter
        m_renderUpdate = 0.0;
        m_totalUpdate = 0.0;
        /// Calculate object offset
        UpdateTexCamera();
        m_currCamDir = m_mainCam.transform.position - transform.position;
        m_prevCamDir = m_currCamDir;
    }
    private void UpdateTexCamera()
    {
        /// Update Texture Camera Pos/Ort
        Vector3 pos = targetDrawObj.transform.position + (m_mainCam.transform.position - transform.position).normalized*1.0f;
        m_tm.UpdateTexCam(pos, targetDrawObj.transform);
        m_tm.Render();
    }
    private bool isUpdate() 
    {
        /// Determine rendering update
        m_currCamDir = m_mainCam.transform.position - transform.position;
        if (Vector3.Angle(m_currCamDir.normalized, m_prevCamDir.normalized) > m_angleThrehold)
            return true;
        return false;
    }
}
