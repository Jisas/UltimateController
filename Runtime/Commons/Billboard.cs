using System.Collections;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] Transform target;
    void OnEnable(){
        if(target == null) target = Camera.main.transform;
        StartCoroutine(LookAtTarget());
    }

    private IEnumerator LookAtTarget(){
        while(this.gameObject.activeInHierarchy){
            Vector3 _dir = target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(_dir);
            yield return null;
        }
    }
}
