//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    public class DemoScriptTapAndDoubleTap : MonoBehaviour
    {
        private TapGestureRecognizer tapGesture;
        private TapGestureRecognizer doubleTapGesture;

        private void Start()
        {
            tapGesture = new TapGestureRecognizer { MaximumNumberOfTouchesToTrack = 10 };
            doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
            tapGesture.StateUpdated += TapGesture_StateUpdated;
            doubleTapGesture.StateUpdated += DoubleTapGesture_StateUpdated;
            FingersScript.Instance.AddGesture(tapGesture);
            FingersScript.Instance.AddGesture(doubleTapGesture);
        }

        private void TapGesture_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Debug.LogFormat("Single tap at {0},{1}", gesture.FocusX, gesture.FocusY);
            }
        }

        private void DoubleTapGesture_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Debug.LogFormat("Double tap at {0},{1}", gesture.FocusX, gesture.FocusY);
            }
        }

        private void Update()
        {

        }
    }
}
