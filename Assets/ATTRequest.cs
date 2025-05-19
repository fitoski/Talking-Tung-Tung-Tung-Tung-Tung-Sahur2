using UnityEngine;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
using System.Collections;
#endif

public class ATTRequest : MonoBehaviour
{
#if UNITY_IOS
    IEnumerator Start()
    {
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
            ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
        yield return new WaitForSeconds(1);
    }
#endif
}
