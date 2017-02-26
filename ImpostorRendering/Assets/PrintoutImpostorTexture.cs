using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PrintoutImpostorTexture : MonoBehaviour {
    private GameObject targetObj;
    private Camera mainCam;

    private float m_cacheAngleRes = 20.0f;
    private float m_cacheXAngle = 40.0f;
    private float m_cacheYAngle = 40.0f;
    private int m_row;
    private int m_column;
    private int m_rendertexSize;
    private List<List<Texture2D>> impostorTexCache;

    [SerializeField]    private bool m_PrintoutTexture= false;
    [SerializeField]    private string m_FileName = "impostorCacheTex.png";
    // Use this for initialization
    void Start () {

        Debug.Log("Start m_PrintoutTexture "+ m_PrintoutTexture);
        if (m_PrintoutTexture)
        {
            Assign();
            GenerateCaching();
            PrintoutTexture();
        }
	}
	// Update is called once per frame
	void Update () {
	}
    private void Assign()
    {
        targetObj = GameObject.Find("Skateboard");
        mainCam = Camera.allCameras[0];
        impostorTexCache = new List<List<Texture2D>>();

        m_rendertexSize = 512;
        m_row = (int)(m_cacheXAngle*2 / m_cacheAngleRes) +1;
        m_column = (int)(m_cacheYAngle*2 / m_cacheAngleRes) + 1;
    }
    private void GenerateCaching()
    {
        /// Virtual Camera Setting
        Vector3 viewDir = new Vector3(0.0f, 0.0f, -1.0f);
        TextureManager virtualtm = new TextureManager(m_rendertexSize, 8, ref mainCam);
        int i = 0, j = 0;
        for (float x_angle = -m_cacheXAngle; x_angle <= m_cacheXAngle; x_angle += m_cacheAngleRes)
        {
            j = 0;
            impostorTexCache.Add(new List<Texture2D>());
            for (float y_angle = -m_cacheYAngle; y_angle <= m_cacheYAngle; y_angle += m_cacheAngleRes)
            {
                Vector3 curDir = Quaternion.Euler(x_angle, y_angle, 0.0f) * viewDir;
                virtualtm.UpdateTexCam(targetObj.transform.position + curDir.normalized*1.0f, targetObj.transform);
                RenderTexture.active = virtualtm.GetTexture();
                virtualtm.Render();

                impostorTexCache[i].Add(new Texture2D(virtualtm.GetTexture().width, virtualtm.GetTexture().height));
                impostorTexCache[i][j].ReadPixels(new Rect(0, 0, virtualtm.GetTexture().width, virtualtm.GetTexture().height), 0, 0);
                impostorTexCache[i][j].Apply();
                j++;
            }
            i++;
            Debug.Log("Calculating : "+i+"/ "+ ((m_cacheXAngle*2/ m_cacheAngleRes)+1) );
        }
        Debug.Log("Calculated Cache Files");
    }
    private void PrintoutTexture()
    {
        /// Printout Texture Atlas
        Texture2D bigTex = new Texture2D(m_rendertexSize * m_row, m_rendertexSize * m_column, TextureFormat.ARGB32, true);
        for (int r = 0; r < m_row; r++)
            for (int c = 0; c < m_column; c++)
            {
                for (int i = 0; i < m_rendertexSize; i++)
                    for (int j = 0; j < m_rendertexSize; j++)
                    {
                        bigTex.SetPixel(i + m_rendertexSize*r, j+ m_rendertexSize*c, impostorTexCache[r][c].GetPixel(i, j));
                    }
            }
        byte[] bytes = bigTex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../" + m_FileName, bytes);
        Debug.Log("Write : " + Application.dataPath + "/../" + m_FileName);
    }
}
