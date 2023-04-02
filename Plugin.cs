using BepInEx;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using BepInEx.Logging;
using UnityEngine.SceneManagement;

namespace RadsiStuff
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public GameObject player;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            PatchTurtleRide.endingImageTexture = new Texture2D(2, 2);
            PatchTurtleRide.endingImageTexture.LoadImage(Resource1.TurtleEndingImage);
        }
    }

    public class Billboard : MonoBehaviour
    {
        private float currentTime = 0f;
        private Transform _camera;
        private FadeController _splashController;

        void Start()
        {
            _splashController = GameObject.Find("Fade").GetComponent<FadeController>();
            _camera = Camera.main.transform;
            transform.localScale = new Vector3(3.1f, 1.8f, 1);
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        void Update()
        {
            currentTime += Time.deltaTime;

            if (currentTime >= 12f && currentTime <= 13f)
            {
                _splashController.FadeIn();
            }

            if (currentTime >= 13f && currentTime <= 14f)
            {
                Camera.main.transform.SetParent(null);
            }

            if (currentTime >= 13.5f && currentTime <= 15f)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                GameObject.Find("Directional Light").SetActive(false);
                Camera.main.cullingMask = 1 << LayerMask.NameToLayer("Player");
                _splashController.FadeOut();
            }

            if (currentTime >= 20f)
            {
                SceneManager.LoadScene(0);
            }
        }

        void LateUpdate()
        {
            transform.LookAt(transform.position + _camera.rotation * Vector3.forward, _camera.rotation * Vector3.down);
            transform.position = _camera.position + _camera.forward * 2;
        }
    }

    [HarmonyPatch(typeof(TurtleController), "Trigger")]
    public static class PatchTurtleRide
    {
        public static Texture2D endingImageTexture;

        public static void Postfix(TurtleController __instance)
        {
            Transform turtle = GameObject.FindGameObjectWithTag("Turtle").transform;

            RaycastHit hit;
            if (Physics.Raycast(turtle.position, Vector3.up, out hit, 1f) && hit.collider.CompareTag("Player"))
            {
                Camera.main.transform.SetParent(turtle);

                Material material = new Material(Shader.Find("Standard"));
                material.mainTexture = endingImageTexture;
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject cubeParent = new GameObject();
                cube.transform.SetParent(cubeParent.transform);
                cube.layer = LayerMask.NameToLayer("Player");
                cube.SetActive(false);
                cube.GetComponent<Renderer>().material = material;
                cubeParent.AddComponent<Billboard>();
            }
        }
    }
}