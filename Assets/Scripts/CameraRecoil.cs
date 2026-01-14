using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("=== HIP FIRE (От бедра) ===")]
    [SerializeField] private float hipRecoilX = -2f;
    [SerializeField] private float hipRecoilY = 2f;
    [SerializeField] private float hipRecoilZ = 0.5f;
    [SerializeField] private float hipSnappiness = 6f;
    [SerializeField] private float hipReturnSpeed = 2f;

    [Header("=== ADS (В прицеле) ===")]
    [SerializeField] private float adsRecoilX = -0.5f;
    [SerializeField] private float adsRecoilY = 0.2f;
    [SerializeField] private float adsRecoilZ = 0.1f;
    [SerializeField] private float adsSnappiness = 10f;
    [SerializeField] private float adsReturnSpeed = 5f;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private float currentSnappiness;
    private float currentReturnSpeed;

    void Start()
    {
        currentSnappiness = hipSnappiness;
        currentReturnSpeed = hipReturnSpeed;
    }

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, currentReturnSpeed * Time.deltaTime);

        currentRotation = Vector3.Slerp(currentRotation, targetRotation, currentSnappiness * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(bool isAiming)
    {
        if (isAiming)
        {
            currentSnappiness = adsSnappiness;
            currentReturnSpeed = adsReturnSpeed;

            float randomY = Random.Range(-adsRecoilY, adsRecoilY);
            float randomZ = Random.Range(-adsRecoilZ, adsRecoilZ);
            targetRotation += new Vector3(adsRecoilX, randomY, randomZ);
        }
        else
        {
            currentSnappiness = hipSnappiness;
            currentReturnSpeed = hipReturnSpeed;

            float randomY = Random.Range(-hipRecoilY, hipRecoilY);
            float randomZ = Random.Range(-hipRecoilZ, hipRecoilZ);
            targetRotation += new Vector3(hipRecoilX, randomY, randomZ);
        }
    }
}