using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class PrintoutImpostorTexture : MonoBehaviour {
    
    private Camera m_mainCam;

    private int m_row;
    private int m_column;
    private List<List<Texture2D>> m_impostorTexCache;

    [SerializeField]    private GameObject m_targetObj;
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
        m_targetObj = GameObject.Find("Skateboard");
        m_mainCam = Camera.allCameras[0];
        m_impostorTexCache = new List<List<Texture2D>>();

        m_row = (int)(TextureCacheInfo.cacheAngleScope*2 / TextureCacheInfo.cacheAngleRes) +1;
        m_column = (int)(TextureCacheInfo.cacheAngleScope * 2 / TextureCacheInfo.cacheAngleRes) + 1;
    }
    private void GenerateCaching()
    {
        /// Virtual Camera Setting
        Vector3 viewDir = new Vector3(0.0f, 0.0f, -1.0f);
        TextureManager virtualtm = new TextureManager(TextureCacheInfo.textureSize, ref m_mainCam, m_targetObj.layer);
        int i = 0, j = 0;
        for (float x_angle = -TextureCacheInfo.cacheAngleScope; x_angle <= TextureCacheInfo.cacheAngleScope; x_angle += TextureCacheInfo.cacheAngleRes)
        {
            j = 0;
            m_impostorTexCache.Add(new List<Texture2D>());
            for (float y_angle = -TextureCacheInfo.cacheAngleScope; y_angle <= TextureCacheInfo.cacheAngleScope; y_angle += TextureCacheInfo.cacheAngleRes)
            {
                Vector3 curDir = Quaternion.Euler(x_angle, y_angle, 0.0f) * viewDir;
                virtualtm.UpdateTexCam(m_targetObj.transform.position + curDir.normalized*1.0f, m_targetObj.transform);
                RenderTexture.active = virtualtm.GetTexture();
                virtualtm.Render();

                m_impostorTexCache[i].Add(new Texture2D(virtualtm.GetTexture().width, virtualtm.GetTexture().height));
                m_impostorTexCache[i][j].ReadPixels(new Rect(0, 0, virtualtm.GetTexture().width, virtualtm.GetTexture().height), 0, 0);
                m_impostorTexCache[i][j].Apply();
                j++;
            }
            i++;
            Debug.Log("Calculating : "+i+"/ "+ ((TextureCacheInfo.cacheAngleScope*2/ TextureCacheInfo.cacheAngleRes)+1) );
        }
        Debug.Log("Calculated Cache Files");
    }
    private void PrintoutTexture()
    {
        /// Printout Texture Atlas
        Texture2D bigTex = new Texture2D(TextureCacheInfo.textureSize * m_row, TextureCacheInfo.textureSize * m_column, TextureFormat.ARGB32, true);
        for (int r = 0; r < m_row; r++)
            for (int c = 0; c < m_column; c++)
            {
                for (int i = 0; i < TextureCacheInfo.textureSize; i++)
                    for (int j = 0; j < TextureCacheInfo.textureSize; j++)
                    {
                        bigTex.SetPixel(i + TextureCacheInfo.textureSize*r, j+ TextureCacheInfo.textureSize*c, m_impostorTexCache[r][c].GetPixel(i, j));
                    }
            }
        byte[] bytes = bigTex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../" + m_FileName, bytes);
        Debug.Log("Write : " + Application.dataPath + "/../" + m_FileName);
    }
}
