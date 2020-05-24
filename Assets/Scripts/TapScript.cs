using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapScript : MonoBehaviour
{
    int _tapCount = 0;
    [SerializeField] Text outputTextView = null;
    bool _isDragging = false;

    IEnumerator checkDragging()
    {
        yield return new WaitForSeconds(0.3f);
    }

    private void Start()
    {
        _tapCount = 0;
        _isDragging = !(GameObject.Find("Input Image").GetComponent<LineDrawer>().isStoppedDrag);

    }

    void Update()
    {
        int currentTextLength = outputTextView.text.Length;
        float doubleTapTimer = 0;
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began && !_isDragging)
        {
            _tapCount++;
        }

        if (_tapCount > 0)
        {
            doubleTapTimer += Time.deltaTime;
        }

        if (_tapCount == 3 && !_isDragging)
        {
            StartCoroutine("checkDragging");
            if (!_isDragging) 
            {
                Debug.Log("Double Tap");
                outputTextView.text = outputTextView.text + " ";
                doubleTapTimer = 0.0f;
                _tapCount = 0; 
            }
        }
        //else if (_tapCount == 1 && currentTextLength > 0)
        //{
        //    outputTextView.text.Remove(currentTextLength - 1, 1);
        //    doubleTapTimer = 0.0f;
        //    _tapCount = 0;
        //}
        if (doubleTapTimer > 0.5f)
        {
            doubleTapTimer = 0f;
            _tapCount = 0;
        }
    }

    //private IEnumerator Countdown()
    //{
    //    yield return new WaitForSeconds(0.3f);
    //    _tapCount = 0;
    //}
}
