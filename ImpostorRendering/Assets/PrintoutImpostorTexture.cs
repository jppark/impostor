using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintoutImpostorTexture : MonoBehaviour {
    private GameObject targetObj;
    private Camera texPrintCam;

    [SerializeField]    private bool m_PrintoutTexture= true;
    [SerializeField]    private string m_FileName = "impostorTex.png";
    [SerializeField]    private float m_ResolutionDeg = 3.0f;
    // Use this for initialization
    void Start () {
        if (m_PrintoutTexture)
        {
            Assign();
            PrintoutTexture();
        }
	}
	// Update is called once per frame
	void Update () {
	}
    private void Assign()
    {
        targetObj = GameObject.Find("Skateboard");
        texPrintCam = new Camera();
        texPrintCam.transform.position = targetObj.transform.position + new Vector3(1.0f, 0.0f, 0.0f);
        texPrintCam.transform.LookAt(targetObj.transform);
    }
    private int PrintoutTexture()
    {
        return 1;
    }
    
}
