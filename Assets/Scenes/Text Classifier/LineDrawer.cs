using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class LineDrawer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{


    [System.Serializable]
    public class DrawEvent : UnityEvent<RenderTexture>
    {
    }

    TextClassifier _textClassifier;

    [SerializeField] Vector2Int imageDimention = new Vector2Int(28, 28);
    [SerializeField] public Color paintColor = Color.white;
    [SerializeField] RenderTextureFormat format = RenderTextureFormat.ARGBFloat;

    [SerializeField] DrawEvent OnDraw = new DrawEvent();


    RawImage imageView = null;

    Mesh lineMesh;
    Material lineMaterial;
    Texture2D clearTexture;

    RenderTexture texture;

    public bool isStoppedDrag = true;
    bool _hasPrinted = false;

    void OnEnable()
    {
        imageView = GetComponent<RawImage>();

        lineMesh = new Mesh();
        lineMesh.MarkDynamic();
        lineMesh.vertices = new Vector3[2];
        lineMesh.SetIndices(new[] { 0, 1 }, MeshTopology.Lines, 0);
        lineMaterial = new Material(Shader.Find("Hidden/LineShader"));
        lineMaterial.SetColor("_Color", paintColor);

        texture = new RenderTexture(imageDimention.x, imageDimention.y, 0, format);
        texture.filterMode = FilterMode.Bilinear;
        imageView.texture = texture;

        clearTexture = Texture2D.blackTexture;

        var trigger = GetComponent<EventTrigger>();
    }

    void Start()
    {
        ClearTexture();
        _textClassifier = GameObject.Find("Text Classifier").GetComponent<TextClassifier>();
        _textClassifier.Start();
        _hasPrinted = false;
    }


    void OnDisable()
    {
        texture?.Release();
        Destroy(lineMesh);
        Destroy(lineMaterial);
        //OnDestroy();
    }

    public void ClearTexture()
    {
        Graphics.Blit(clearTexture, texture);
    }

    void Update()
    {
        //_textClassifier.ReturnTexture();
    }

    public void OnDrag(PointerEventData data)
    {
        data.Use();

        var area = data.pointerDrag.GetComponent<RectTransform>();
        var p0 = area.InverseTransformPoint(data.position - data.delta);
        var p1 = area.InverseTransformPoint(data.position);

        var scale = new Vector3(2 / area.rect.width, -2 / area.rect.height, 0);
        p0 = Vector3.Scale(p0, scale);
        p1 = Vector3.Scale(p1, scale);

        DrawLine(p0, p1);

        OnDraw.Invoke(texture);
    }

    void DrawLine(Vector3 p0, Vector3 p1)
    {
        var prevRT = RenderTexture.active;
        RenderTexture.active = texture;

        lineMesh.SetVertices(new List<Vector3>() { p0, p1 });
        lineMaterial.SetPass(0);
        Graphics.DrawMeshNow(lineMesh, Matrix4x4.identity);

        RenderTexture.active = prevRT;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isStoppedDrag = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isStoppedDrag = true;
        //if (!_textClassifier.isProcessing)
        StartCoroutine("InferAndClearScreen");
    }

    IEnumerator InferAndClearScreen()
    {
        yield return new WaitForSeconds(1.75f);
        if (isStoppedDrag == true && _hasPrinted == false)
        {
            _textClassifier.Infer(texture);
            _hasPrinted = true;
            ClearTexture();
        }
        yield return new WaitForSeconds(1.75f);
        _hasPrinted = false;
    }

    //void OnDestroy()
    //{
    //    _textClassifier.Destroy();
    //}
}

