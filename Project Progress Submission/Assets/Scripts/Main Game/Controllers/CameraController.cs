
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Controls")]
    [SerializeField] new Camera camera;
    [SerializeField] Level currentLevel;

    [SerializeField] PathFinder pf;

    Vector3 CameraOrigin;
    Vector3 PointerDifference;
    Vector3 ResetCameraOrigin;

    public void Start()
    {
        ResetCameraOrigin = camera.transform.position;
        GlobalSettings.ZoomSettings[CameraKey.targetZoom] = camera.orthographicSize;
    }

    public void ClampFollow(Vector3 EdgeChecker)
    {
        clampCameraPan(EdgeChecker);
        clampCameraZoom();
    }

    public void Move()
    {   
        Vector3 InitialCamPosition = UtilityClass.CopyVector(camera.transform.position);

        panCamera();

        dragPanCamera();

        clampCameraPan(InitialCamPosition);

        zoomCamera();

        clampCameraZoom();


        if(Input.GetKeyUp(KeyCode.Home))
        {
            camera.transform.position = ResetCameraOrigin;
            camera.orthographicSize = 5f;
            GlobalSettings.ZoomSettings[CameraKey.targetZoom] = 5f;
        }
    }

    void dragPanCamera()
    {
        if(Input.GetMouseButtonDown(2))
        {
            CameraOrigin = UtilityClass.GetScreenMouseToWorld();
        }

        if(Input.GetMouseButton(2))
        {
            PointerDifference = UtilityClass.GetScreenMouseToWorld() - camera.transform.position;

            camera.transform.position = CameraOrigin - PointerDifference;
        }
    }

    void panCamera()
    {
        if(Input.anyKey)
        {
            bool fastPan = Input.GetKey(GlobalSettings.ControlMap[SettingKey.FastPan]);
            Vector3 pan = Vector3.zero;
            
            if(Input.GetKey(GlobalSettings.ControlMap[SettingKey.Up]) || Input.GetKey(GlobalSettings.ControlMap[SettingKey.UpS])) pan += new Vector3(0, 1, 0);
            if(Input.GetKey(GlobalSettings.ControlMap[SettingKey.Down]) || Input.GetKey(GlobalSettings.ControlMap[SettingKey.DownS])) pan += new Vector3(0, -1, 0);
            if(Input.GetKey(GlobalSettings.ControlMap[SettingKey.Left]) || Input.GetKey(GlobalSettings.ControlMap[SettingKey.LeftS])) pan += new Vector3(-1, 0, 0);
            if(Input.GetKey(GlobalSettings.ControlMap[SettingKey.Right]) || Input.GetKey(GlobalSettings.ControlMap[SettingKey.RightS])) pan += new Vector3(1, 0, 0);

            //pan = clampPan((fastPan ? pan *= GlobalSettings.PanSpeed * 3.0f : pan *= GlobalSettings.PanSpeed) * Time.deltaTime);
            pan = (fastPan ? pan *= GlobalSettings.PanSpeed * 3.0f : pan *= GlobalSettings.PanSpeed) * Time.deltaTime;
            if(pan.Equals(Vector3.zero)) return;
            
            camera.transform.position += pan;
        }
    }

    float oldCamSize;
    float newCamSize;
    void zoomCamera()
    {
        if(Input.mouseScrollDelta.y != 0.00f)
        {
            GlobalSettings.ZoomSettings[CameraKey.targetZoom] -= Input.mouseScrollDelta.y * 0.25f;
            GlobalSettings.ZoomSettings[CameraKey.targetZoom] = Mathf.Clamp(GlobalSettings.ZoomSettings[CameraKey.targetZoom], GlobalSettings.ZoomSettings[CameraKey.minZoom], GlobalSettings.ZoomSettings[CameraKey.maxZoom]);
        }

        newCamSize = Mathf.MoveTowards(camera.orthographicSize, GlobalSettings.ZoomSettings[CameraKey.targetZoom], GlobalSettings.ZoomSettings[CameraKey.zoomSpeed] * Time.deltaTime);
        oldCamSize = camera.orthographicSize;
        camera.orthographicSize = newCamSize;
    }

    void clampCameraPan(Vector3 ResetVector)
    {
        //If Camera transform position exceeds the width and height bounds of the level, then reset the position to its initial position
        Vector3 FauxCamPos = UtilityClass.CopyAbsVector(camera.transform.position) + new Vector3(camera.orthographicSize * camera.aspect, camera.orthographicSize);
        //Reset X position of camera
        if(FauxCamPos.x > Level.MapSize.First/2) camera.transform.position = new Vector3(ResetVector.x, camera.transform.position.y, camera.transform.position.z);
        //Reset Y position of camera
        if(FauxCamPos.y > Level.MapSize.Second/2) camera.transform.position = new Vector3(camera.transform.position.x, ResetVector.y, camera.transform.position.z);
    }

    void clampCameraZoom()
    {
        float MapMaxWidth = Level.MapSize.First/2;
        float MapMaxHeight = Level.MapSize.Second/2;

        //Check to see if by scrolling out, we need to move the camera
        Vector3 FauxCameraPosition = UtilityClass.CopyVector(camera.transform.position);

        FauxCameraPosition.x += FauxCameraPosition.x > 0 ? camera.orthographicSize * camera.aspect : -camera.orthographicSize * camera.aspect;
        FauxCameraPosition.y += FauxCameraPosition.y > 0 ? camera.orthographicSize : -camera.orthographicSize;

        if(FauxCameraPosition.x > MapMaxWidth) FauxCameraPosition.x = MapMaxWidth;
        if(FauxCameraPosition.x < -MapMaxWidth) FauxCameraPosition.x = -MapMaxWidth;

        if(FauxCameraPosition.y > MapMaxHeight) FauxCameraPosition.y = MapMaxHeight;
        if(FauxCameraPosition.y < -MapMaxHeight) FauxCameraPosition.y = -MapMaxHeight;

        FauxCameraPosition.x -= FauxCameraPosition.x > 0 ? camera.orthographicSize * camera.aspect : -camera.orthographicSize * camera.aspect;
        FauxCameraPosition.y -= FauxCameraPosition.y > 0 ? camera.orthographicSize : -camera.orthographicSize;

        camera.transform.position = UtilityClass.CopyVector(FauxCameraPosition);

        //Clamp camera size to size restriction
        Vector3 AbsFauxCamera = UtilityClass.CopyAbsVector(camera.transform.position) + new Vector3(camera.orthographicSize * camera.aspect, camera.orthographicSize);
        if(AbsFauxCamera.x > MapMaxWidth || AbsFauxCamera.y > MapMaxHeight)
        {
            oldCamSize = Mathf.Round(oldCamSize * 10) / 10;
            camera.orthographicSize = oldCamSize;
            GlobalSettings.ZoomSettings[CameraKey.targetZoom] = oldCamSize;
        }
    }
}