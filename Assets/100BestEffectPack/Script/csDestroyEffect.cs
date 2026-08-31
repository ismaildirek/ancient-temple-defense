using UnityEngine;
using System.Collections;

public class csDestroyEffect : MonoBehaviour {
	
	void Update () {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.C))
        {
            Destroy(gameObject);
        }
#endif
    }
}
