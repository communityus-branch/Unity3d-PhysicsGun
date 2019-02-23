﻿using cakeslice;
using UnityEngine;

public class GunLineRenderer : MonoBehaviour
{
    [Header("Line Renderer Settings"), Space(5)]
    [SerializeField]
    private Vector2         _uvAnimationRate        = new Vector2(1.0f, 0.0f);
    private Vector2         _uvOffset               = Vector2.zero;

    [SerializeField]
    private int             _arcResolution          = 12;
    private Vector3[]       _inputPoints;
    private LineRenderer    _lineRenderer;

    [SerializeField]
    private Transform       _attachEffect           = null;

    private GameObject      _objectToHightlight;
    private OutlineEffect   _outlineEffect;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        if (_lineRenderer == null)
            _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _inputPoints                = new Vector3[_arcResolution];
        _lineRenderer.positionCount = _arcResolution;
        _lineRenderer.enabled       = false;

        _outlineEffect = Camera.main.GetComponent<OutlineEffect>();

        if(_outlineEffect == null)
        {
            _outlineEffect = Camera.main.gameObject.AddComponent<OutlineEffect>();
        }

        _attachEffect.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!_lineRenderer.enabled)
            return;

        //Animate Line Renderer Texture
        _uvOffset -= _uvAnimationRate * Time.deltaTime;
        _lineRenderer.material.SetTextureOffset(StringID.MainTex, _uvOffset);
        _lineRenderer.SetPositions(_inputPoints);

        //Align our attached effect with the surface of the grabbed object

        var rayOrigin       = Vector3.Lerp(_inputPoints[0], _inputPoints[_arcResolution - 1], 0.999f);
        var rayDirection    = _objectToHightlight.transform.position - rayOrigin;
#if UNITY_EDITOR
        Debug.DrawRay(rayOrigin, rayDirection, Color.yellow);
#endif

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 5f))
        {
            if (hit.collider.gameObject == _objectToHightlight)
            {
                _attachEffect.position = rayOrigin;
                _attachEffect.rotation = Quaternion.FromToRotation(_objectToHightlight.transform.forward, hit.normal) * _objectToHightlight.transform.rotation;
            }
        }
        else
        {
            _attachEffect.position = _inputPoints[_arcResolution - 1];
        }
    }

    public void StartLineRenderer(GameObject objectToHighlight)
    {
        _lineRenderer.enabled = true;
        _objectToHightlight = objectToHighlight;

        var outline = _objectToHightlight.GetComponent<Outline>();

        if(outline == null)
            outline = _objectToHightlight.AddComponent<Outline>();

         _outlineEffect.AddOutline(outline); 

        _attachEffect.gameObject.SetActive(true);
    }

    public void StopLineRenderer()
    {
        _lineRenderer.enabled = false;
        UpdateArcPoints(Vector3.zero, Vector3.zero, Vector3.zero);

        _lineRenderer.SetPositions(_inputPoints);

        var outline = _objectToHightlight.GetComponent<Outline>();

        _outlineEffect.RemoveOutline(outline);       

        Destroy(outline);

        _objectToHightlight = null;

        _attachEffect.gameObject.SetActive(false);
    }

    public void UpdateArcPoints(Vector3 a, Vector3 b, Vector3 c)
    {
        b = Vector3.Lerp(a, b, 0.6f);

        for (int i = 0; i < _arcResolution - 1; i++)
        {
            var t = (float)i / _arcResolution;
            _inputPoints[i] = Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
        }

        _inputPoints[_arcResolution - 1] = c;
    }
}
