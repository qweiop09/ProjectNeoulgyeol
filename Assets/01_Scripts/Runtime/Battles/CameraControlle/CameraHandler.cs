using System.Threading.Tasks;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.CameraControlle
{
public class CameraHandler : Singleton<CameraHandler>
{
    [SerializeField] private Camera camera;

    public void Awake()
    {
        base.Awake();
        
        if (camera == null)
            camera = gameObject.GetComponent<UnityEngine.Camera>();
    }

    public void Move(Vector3 targetPosition, int targetSize )
    {
        transform.position = targetPosition;
        camera.orthographicSize = targetSize;
    }
    
    public async Task MoveToLerp(Vector3 targetPosition, int targetSize)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.001f
               || Mathf.Abs(camera.orthographicSize - targetSize) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.05f);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetSize, 0.05f);
            await Wait(0.001f);
        }

        transform.position = targetPosition;
        camera.orthographicSize = targetSize;
    }

    public void PositionReset()
    {
        transform.position = new Vector3(0,0,-10);
        camera.orthographicSize = 5;
    }

    public async Task PositionResetToLerp()
    {
        Vector3 temp = new Vector3(0,0,-10);
        
        while ( Vector3.Distance(transform.position,temp) > 0.001f 
               || Mathf.Abs( 5 - camera.orthographicSize) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, temp, 0.05f);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, 5, 0.05f);
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
