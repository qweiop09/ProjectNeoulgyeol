using System;
using System.Threading.Tasks;
using _01_Scripts.Interfacese;
using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Battles.CameraControlle
{
public class CameraHandler : Singleton<CameraHandler>
{
    [SerializeField] private Camera camera;

    private Transform followTargetTransfrom;
    private float followTargetSize;
    private bool isFollowing = false;

    protected override void Awake()
    {
        base.Awake();
        if (this != Instance) return;
        if (camera == null)
            camera = GetComponent<Camera>();
    }

    public void LateUpdate()
    {
        if (followTargetTransfrom != null && isFollowing)
        {
            camera.transform.position =
                Vector3.Lerp(camera.transform.position, followTargetTransfrom.position, 0.1f * Time.deltaTime * 60)
                + new Vector3(0, 0, -10);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, followTargetSize, 0.3f);
        }
    }

    public void SetFollowTransform(Transform target, float targetSize)
    {
        followTargetTransfrom = target;
        followTargetSize = targetSize;
        isFollowing = true;
    }

    public void UnsetFollowTransform()
    {
        followTargetTransfrom = null;
        followTargetSize = 5;
        isFollowing = false;
    }
    
    public void Move(Vector3 targetPosition, int targetSize )
    {
        if (isFollowing)
            return;
        
        camera.transform.position = targetPosition;
        camera.orthographicSize = targetSize;
    }
    
    public async Task MoveToLerp(Vector3 targetPosition, float targetSize)
    {
        if (isFollowing)
            return;
        
        while (Vector3.Distance(camera.transform.position, targetPosition) > 0.05f
               || Mathf.Abs(camera.orthographicSize - targetSize) > 0.05f)
        {
            camera.transform.position = Vector3.Lerp(camera.transform.position, targetPosition, 0.15f * Time.deltaTime * 60);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetSize, 0.15f * Time.deltaTime * 60);
            await Wait(0.001f);
        }

        camera.transform.position = targetPosition;
        camera.orthographicSize = targetSize;
    }

    public void PositionReset()
    {
        if (isFollowing)
            return;
        
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographicSize = 5;
    }

    public async Task PositionResetToLerp()
    {
        if (isFollowing)
            return;
        
        Vector3 temp = new Vector3(0,0,-10);
        
        while (Vector3.Distance(camera.transform.position, temp) > 0.05f
               || Mathf.Abs(5 - camera.orthographicSize) > 0.05f)
        {
            camera.transform.position = Vector3.Lerp(camera.transform.position, temp, 0.15f * Time.deltaTime * 60);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, 5, 0.15f * Time.deltaTime * 60);
            await Wait(0.001f);
        }
        
        camera.transform.position = temp;
        camera.orthographicSize = 5;
    }
    
    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }
    
    
}
}
