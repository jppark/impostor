using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// TextureManager Class : RenderTexture/Camera Management
/// </summary>
public class TextureManager {
    private RenderTexture m_texBuffer;
    private Camera m_textureCam; //
    private bool m_bDrawCache;

    public TextureManager( int _size, ref Camera _refCam, int _targetObjLayer )
    {
        GameObject tgo = new GameObject("Texture Caching Camera");
        m_textureCam = tgo.AddComponent<Camera>();
        m_texBuffer = new RenderTexture(_size, _size, 0, RenderTextureFormat.ARGB32);
        m_texBuffer.antiAliasing = 1;

        m_textureCam.nearClipPlane = _refCam.GetComponent<Camera>().nearClipPlane;
        m_textureCam.farClipPlane = _refCam.GetComponent<Camera>().farClipPlane;
        m_textureCam.targetTexture = m_texBuffer;
        m_textureCam.clearFlags = CameraClearFlags.SolidColor;
        m_textureCam.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        m_textureCam.cullingMask = 1 << _targetObjLayer;

        m_bDrawCache = false;
    }
    public void UpdateTexCam( Vector3 _pos, Transform _target )
    {
        /// Update Texture Camera properties
        m_textureCam.transform.position = _pos;
        m_textureCam.transform.LookAt(_target);
    }
    public void SetRenderTexture( RenderTexture _texture )
    {
        m_texBuffer = _texture;
    }
    public void Render()
    {
        m_textureCam.Render();
    }
    public RenderTexture GetTexture()    { return m_texBuffer; }
    public void switchRenderedWithCache() { m_bDrawCache = !m_bDrawCache; }
    public bool IsRenderedWithCache() { return m_bDrawCache; }
}
