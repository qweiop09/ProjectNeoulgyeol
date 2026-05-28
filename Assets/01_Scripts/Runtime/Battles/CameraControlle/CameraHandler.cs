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

    public void Awake()
    {
        base.Awake();
        
        if (camera == null)
            camera = gameObject.GetComponent<UnityEngine.Camera>();
    }

    public void LateUpdate()
    {
        if (followTargetTransfrom != null && isFollowing)
        {
            transform.position = 
                Vector3.Lerp(transform.position, followTargetTransfrom.position, 0.1f * Time.deltaTime * 60)
                + new Vector3( 0,0,-10);
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
        
        transform.position = targetPosition;
        camera.orthographicSize = targetSize;
    }
    
    public async Task MoveToLerp(Vector3 targetPosition, int targetSize)
    {
        if (isFollowing)
            return;
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f
               || Mathf.Abs(camera.orthographicSize - targetSize) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.15f * Time.deltaTime * 60);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetSize, 0.15f * Time.deltaTime * 60);
            await Wait(0.001f);
        }

        transform.position = targetPosition;
        camera.orthographicSize = targetSize;
    }

    public void PositionReset()
    {
        if (isFollowing)
            return;
        
        transform.position = new Vector3(0,0,-10);
        camera.orthographicSize = 5;
    }

    public async Task PositionResetToLerp()
    {
        if (isFollowing)
            return;
        
        Vector3 temp = new Vector3(0,0,-10);
        
        while ( Vector3.Distance(transform.position,temp) > 0.01f 
               || Mathf.Abs( 5 - camera.orthographicSize) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, temp, 0.15f * Time.deltaTime * 60);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, 5, 0.15f * Time.deltaTime * 60);
            await Wait(0.001f);
        }
        
        transform.position = temp;
        camera.orthographicSize = 5;
    }
    
    private Task Wait(float seconds)
    {
        return Task.Delay((int)(seconds * 1000));
    }
    
    
}
}
