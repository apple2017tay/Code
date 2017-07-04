using UnityEngine;
using System.Collections;
using Amslot_SW;

public class HitObject : MonoBehaviour {
    
	void Start () {
        if (transform.parent.gameObject.transform.childCount < 2)
        {
            AmslotDataManager.Instance.HitObject.Add(transform.parent.gameObject);
            AmslotDataManager.Instance.tempHit.Add(gameObject);
        }
        else Destroy(gameObject);
	}
	

}
