using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tears_Of_Void.Core
{
    public class FacingCamera : MonoBehaviour
    {
        void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}
