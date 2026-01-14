using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("=== HIP FIRE (От бедра) ===")]
    [SerializeField] private Vector3 hipRotation = new Vector3(-10f, 2f, 2f); // X=Вверх, Y=Вбок, Z=Крен
    [SerializeField] private float hipKickBackZ = 0.2f;  // Откат назад
    [SerializeField] private float hipSnappiness = 6f;   // Резкость удара
    [SerializeField] private float hipReturnSpeed = 2f;  // Скорость возврата

    [Header("=== ADS (В прицеле) ===")]
    [SerializeField] private Vector3 adsRotation = new Vector3(-3f, 0.5f, 0.5f);
    [SerializeField] private float adsKickBackZ = 0.1f;
    [SerializeField] private float adsSnappiness = 10f; 
    [SerializeField] private float adsReturnSpeed = 5f; 

    private Vector3 currentRot, targetRot;
    private Vector3 currentPos, targetPos;

    private float currentSnappiness;
    private float currentReturnSpeed;

    void Start()
    {
        currentSnappiness = hipSnappiness;
        currentReturnSpeed = hipReturnSpeed;
    }

    void Update()
    {
        targetRot = Vector3.Lerp(targetRot, Vector3.zero, currentReturnSpeed * Time.deltaTime);
        targetPos = Vector3.Lerp(targetPos, Vector3.zero, currentReturnSpeed * Time.deltaTime);

        currentRot = Vector3.Slerp(currentRot, targetRot, currentSnappiness * Time.deltaTime);
        currentPos = Vector3.Lerp(currentPos, targetPos, currentSnappiness * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(currentRot);
        transform.localPosition = currentPos;
    }

    public void RecoilFire(bool isAiming)
    {
        if (isAiming)
        {
            currentSnappiness = adsSnappiness;
            currentReturnSpeed = adsReturnSpeed;

            float randomY = Random.Range(-adsRotation.y, adsRotation.y);
            float randomZ = Random.Range(-adsRotation.z, adsRotation.z);

            targetRot += new Vector3(adsRotation.x, randomY, randomZ);
            targetPos += new Vector3(0, 0, -adsKickBackZ);
        }
        else
        {
            currentSnappiness = hipSnappiness;
            currentReturnSpeed = hipReturnSpeed;

            float randomY = Random.Range(-hipRotation.y, hipRotation.y);
            float randomZ = Random.Range(-hipRotation.z, hipRotation.z);

            targetRot += new Vector3(hipRotation.x, randomY, randomZ);
            targetPos += new Vector3(0, 0, -hipKickBackZ);
        }
    }
}