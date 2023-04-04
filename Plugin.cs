using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RadsiStuff
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject player;
        private Harmony harmony;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void Update()
        {
            if (player == null)
            {
                player = GameObject.Find("FPSController");
            }
            else if (player.GetComponent<BabyInjectedScript>() == null)
            {
                player.AddComponent<BabyInjectedScript>();
            }
        }
    }

    public class BabyInjectedScript : MonoBehaviour { }

    public class Billboard : MonoBehaviour
    {
        private const float fadeInTime = 12f;
        private const float fadeOutTime = 15f;

        private Transform cameraTransform;
        private FadeController splashController;
        private float timer;

        private void Start()
        {
            splashController = GameObject.Find("Fade").GetComponent<FadeController>();
            cameraTransform = Camera.main.transform;
            transform.localScale = new Vector3(3.1f, 1.8f, 1f);
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= fadeInTime && timer <= fadeInTime + 1f)
            {
                splashController.FadeIn();
            }

            if (timer >= fadeInTime + 1f && timer <= fadeInTime + 2f)
            {
                cameraTransform.SetParent(null);
            }

            if (timer >= fadeInTime + 1.5f && timer <= fadeOutTime)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                GameObject.Find("Directional Light").SetActive(false);
                Camera.main.cullingMask = 1 << LayerMask.NameToLayer("Player");
                splashController.FadeOut();
            }

            if (timer >= fadeOutTime + 5f)
            {
                SceneManager.LoadScene(0);
            }
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.down);
            transform.position = cameraTransform.position + cameraTransform.forward * 2f;
        }
    }

    public class BabyValidZone : MonoBehaviour
    {
        private const float spawnTime = 15.7f;
        private const float destroyTime = 17f;

        private float timer;
        private GameObject baby;
        private GameObject originalBaby;

        private void Start()
        {
            originalBaby = GameObject.Find("Village1/Villagers/BabyScene/NewVillagers/Mom/DefinitiveRiggedVillager_Woman/root/ctrl.torso/hip/spine.001/Baby/");
            baby = Instantiate(originalBaby, Vector3.zero, Quaternion.identity);
            baby.SetActive(false);
            baby.transform.SetParent(GameObject.Find("FPSController").transform);
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= spawnTime && timer <= destroyTime - 0.7f)
            {
                originalBaby.SetActive(false);
                baby.SetActive(true);
                baby.name = "Baby (mod)";
                baby.transform.localPosition = new Vector3(0.0618f, -0.2254f, -1.2563f);
                baby.transform.rotation = Quaternion.Euler(343.3828f, 141.8492f, 350.3807f);
                baby.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            if(timer >= 17)
            {
                Destroy(gameObject);
            }
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

                endingImageTexture = new Texture2D(2, 2);
                endingImageTexture.LoadImage(Resource1.TurtleEndingImage);

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

    [HarmonyPatch(typeof(BabyThrow_Baby), "Trigger")]
    public static class PatchBabyInteraction
    {
        public static void Postfix(BabyThrow_Baby __instance)
        {
            GameObject validZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(validZone.GetComponent<BoxCollider>());
            GameObject.Destroy(validZone.GetComponent<MeshRenderer>());
            validZone.transform.localScale = new Vector3(3f, 3f, 3f);
            validZone.transform.position = new Vector3(-44.7873f, 0.6012f, 96.6236f);
            validZone.AddComponent<BabyValidZone>();
        }
    }
}
