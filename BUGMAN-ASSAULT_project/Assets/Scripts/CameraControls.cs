using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public float cameraPanSpeedMod = 1;

    public int cameraZoomSpeed = 10;
    public int cameraStartingSize = 20;
    public int maxCameraSize = 40;
    public int minCameraSize = 10;

    void Start() {
        GetComponent<Camera>().orthographicSize = cameraStartingSize;
    }

    void Update() {
        //camera zoom controls
        if (Input.GetKey(KeyCode.Z)) {
            GetComponent<Camera>().orthographicSize += cameraZoomSpeed * Time.deltaTime;
            if (GetComponent<Camera>().orthographicSize > maxCameraSize)
                GetComponent<Camera>().orthographicSize = maxCameraSize;
        }
        if (Input.GetKey(KeyCode.C)) {
            GetComponent<Camera>().orthographicSize -= cameraZoomSpeed * Time.deltaTime;
            if (GetComponent<Camera>().orthographicSize < minCameraSize)
                GetComponent<Camera>().orthographicSize = minCameraSize;
        }

        //calculate speed for camera movement
        int cameraPanSpeed = Mathf.FloorToInt(cameraPanSpeedMod * GetComponent<Camera>().orthographicSize);

        //camera movement controls
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            transform.Translate(cameraPanSpeed * Vector3.Normalize(Vector3.forward + Vector3.up) * Time.deltaTime);
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            transform.Translate(cameraPanSpeed * -Vector3.Normalize(Vector3.forward + Vector3.up) * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            transform.Translate(cameraPanSpeed * Vector3.right * Time.deltaTime);
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            transform.Translate(cameraPanSpeed * -Vector3.right * Time.deltaTime);
    }

    public void SetResourceText(string str) {
        transform.GetChild(0).GetChild(0).GetComponent<TextMesh>().text = str;
        return;
    }

    public void SetScoreboardText(string str) {
        transform.GetChild(0).GetChild(1).GetComponent<TextMesh>().text = str;
        return;
    }

    public void SetSelectionText(string str) {
        transform.GetChild(0).GetChild(2).GetComponent<TextMesh>().text = str;
        return;
    }

    public void SetOrdersText(string str) {
        transform.GetChild(0).GetChild(3).GetComponent<TextMesh>().text = str;
        return;
    }
}
