using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tears_Of_Void.Resources
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] Health healthComponent = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas canvas = null;

        private void Update()
        {
            var newScale = new Vector3(healthComponent.GetFraction(), 1, 1);
            float speed = 4.0f;
            if (Mathf.Approximately(healthComponent.GetFraction(), 0))
            {
                canvas.enabled = false;
                return;
            }

            canvas.enabled = true;
            foreground.localScale = Vector3.Lerp(foreground.localScale, newScale, speed * Time.deltaTime);
        }
    }

}