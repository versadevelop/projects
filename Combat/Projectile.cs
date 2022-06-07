using System.Collections;
using System.Collections.Generic;
using Tears_Of_Void.Combat;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 1;

    AIHealth target = null;

    void Update()
    {
        if (target == null) return;
        transform.LookAt(GetAimLocation());
        transform.Translate(Vector3.forward * speed * Time.deltaTime);      
    }
    public void SetTarget(AIHealth target)
    {
        this.target = target;
    }
    private Vector3 GetAimLocation()
    {
        CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
        if (targetCapsule == null)
        {
            return target.transform.localPosition;
        }
        return target.transform.localPosition + Vector3.up * targetCapsule.height / 2;
    }

}