using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using TMPro;
using System;

public enum eFSState
{
    idle,
    pre,
    active,
    post
}


public class FloatingScore : MonoBehaviour
{
    public eFSState state = eFSState.idle;
    [SerializeField]
    private int _score = 0; // The score field
    public string scoreString;
    public TextMeshProUGUI textMeshPro;
    // The score property also sets scoreString when set
    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");// "N0" adds commas to the num
                                                // Search "C# Standard Numeric Format Strings" for ToString formats
        }
    }
    public List<Vector2> bezierPts; // B��zier points for movement
    public List<float> fontSizes; // B��zier points for font scaling
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut; 
    public GameObject reportFinishTo = null;
    private RectTransform rectTrans;
    private TextMeshProUGUI txt;
    // Set up the FloatingScore and movement
    // Note the use of parameter defaults for eTimeS & eTimeD
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;
        txt = GetComponent<TextMeshProUGUI>();
        bezierPts = new List<Vector2>(ePts);
        if (ePts.Count == 1)
        { // If there's only one point
          // ...then just go there.
            transform.position = ePts[0]; return;
        }
        // If eTimeS is the default, just start at the current time
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        state = eFSState.pre; // Set it to the pre state, ready to start moving
    }
    public void FSCallback(FloatingScore fs)
    {
        // When this callback is called by SendMessage,
        // add the score from the calling FloatingScore
        score += fs.score;
    }
    // Update is called once per frame
    void Update()
    {
        // If this is not moving, just return
        if (state == eFSState.idle) return;
        // Get u from the current time and duration
        // u ranges from 0 to 1 (usually)
        float u = (Time.time - timeStart) / timeDuration;
        // Use Easing class from Utils to curve the u value
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        { // If u<0, then we shouldn't move yet.
            state = eFSState.pre;
            txt.enabled = false; // Hide the score initially
        }
        else
        {
            if (u >= 1)
            { // If u>=1, we're done moving
                uC = 1; // Set uC=1 so we don't overshoot
                state = eFSState.post;
                if (reportFinishTo != null)
                { //If there's a callback GameObject
                  // Use SendMessage to call the FSCallback method
                  // with this as the parameter.
                    reportFinishTo.SendMessage("FSCallback", this);
                    // Now that the message has been sent,
                    // Destroy this gameObject
                    Destroy(gameObject);
                }
                else
                { // If there is nothing to callback
                  // ...then don't destroy this. Just let it stay still.
                    state = eFSState.idle;
                }
            }
            else
            {
                // 0<=u<1, which means that this is active and moving
                state = eFSState.active;
                txt.enabled = true; // Show the score once more
            }
            // Use B��zier curve to move this to the right point
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            // R// RectTransform anchors can be used to position UI objects relative
            // to total size of the screen
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontSizes != null && fontSizes.Count > 0)
            {
                // If fontSizes has values in it
                // ...then adjust the fontSize of this GUIText
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                textMeshPro.fontSize = size;
            }
        }
    }

    internal void Init(List<Vector3> fsPts, int v1, int v2)
    {
        throw new NotImplementedException();
    }
}