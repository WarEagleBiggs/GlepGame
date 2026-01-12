using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : MonoBehaviour
{
    public float defaultSize = 15f;
    public float minSize = 8f;
    public float maxSize = 30f;
    public float step = 1f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (cam && cam.orthographic)
            cam.orthographicSize = defaultSize;
    }

    void Update()
    {
        if (!cam || !cam.orthographic) return;

        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + step, minSize, maxSize);

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - step, minSize, maxSize);

        if (Input.GetKeyDown(KeyCode.Y))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public GameObject AssetsScreen;
    
    public void AssetsMenu()
    {
        AssetsScreen.SetActive(!AssetsScreen.activeSelf);
    }
}