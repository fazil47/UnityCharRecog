using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;

public class TextClassifier : MonoBehaviour
{
    [SerializeField] string fileName = "mnist.tflite";
    [SerializeField] Text outputTextView = null;
    [SerializeField] ComputeShader compute = null;

    Interpreter interpreter;

    public bool isProcessing = false;
    float[,] inputs = new float[28, 28];
    float[] outputs = new float[10];
    ComputeBuffer inputBuffer;

    public System.Text.StringBuilder sb = new System.Text.StringBuilder();

    public bool shouldClearText = true;
    RenderTexture drawingSurface;
    //bool _isStoppedDrag = true;
    //bool _hasPrinted = false;

    public void Start()
    {
        shouldClearText = true;

        var options = new Interpreter.Options()
        {
            threads = 2,
            gpuDelegate = null,
        };
        interpreter = new Interpreter(FileUtil.LoadFile(fileName), options);
        interpreter.ResizeInputTensor(0, new int[] { 1, 28, 28, 1 });
        interpreter.AllocateTensors();

        inputBuffer = new ComputeBuffer(28 * 28, sizeof(float));

        //_hasPrinted = false;

        //GameObject inputImage = GameObject.Find("Input Image");
        //LineDrawer lineDrawer = inputImage.GetComponent<LineDrawer>();
        //_isStoppedDrag = lineDrawer.isStoppedDrag;
    }

    public void Destroy()
    {
        interpreter?.Dispose();
        inputBuffer?.Dispose();
    }

    public void OnDrawTexture(RenderTexture texture)
    {
        drawingSurface = texture;
    }

    public RenderTexture ReturnTexture()
    {
        Debug.Log("ReturnTexture");
        return drawingSurface;
    }

    public void Infer(RenderTexture texture)
    {
        isProcessing = true;

        compute.SetTexture(0, "InputTexture", texture);
        compute.SetBuffer(0, "OutputTensor", inputBuffer);
        compute.Dispatch(0, 28 / 4, 28 / 4, 1);
        inputBuffer.GetData(inputs);

        float startTime = Time.realtimeSinceStartup;
        interpreter.SetInputTensorData(0, inputs);
        interpreter.Invoke();
        interpreter.GetOutputTensorData(0, outputs);
        float duration = Time.realtimeSinceStartup - startTime;

        if (shouldClearText == true)
        {
            sb.Clear();
            shouldClearText = false;
        }
        int outputValue = 0;
        for (int i = 0; i < outputs.Length; i++)
        {
            if (outputs[i] > outputs[outputValue])
            {
                outputValue = i;
            }
        }
        Debug.Log(outputValue);
        sb.Append(outputValue.ToString());
        outputTextView.text = sb.ToString();

        isProcessing = false;
        //_hasPrinted = true;
        //StartCoroutine("ResetPrintLetter");
    }

    //IEnumerator ResetPrintLetter()
    //{
    //    yield return new WaitForSeconds(1.75f);
    //    _hasPrinted = false;
    //}

    //private void Update()
    //{
    //    GameObject inputImage = GameObject.Find("Input Image");
    //    LineDrawer lineDrawer = inputImage.GetComponent<LineDrawer>();
    //    _isStoppedDrag = lineDrawer.isStoppedDrag;
    //}

    //IEnumerator InferText(RenderTexture texture)
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    if (!isProcessing && _isStoppedDrag == true && _hasPrinted == false)
    //    {
    //        Infer(texture);
    //    }

    //}
}
