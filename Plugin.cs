using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RadsiStuff
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public bool loaded = false;

        public static GameObject Player;
        GameObject gun;

        static public AssetBundle gunBundle = AssetBundle.LoadFromMemory(Resource1.gun);

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
        }

        private void Update()
        {
            if (Player == null)
            {
                Player = GameObject.Find("FPSController");
            }
            else if (Player != null)
            {
                if(gun == null)
                {
                    gun = Instantiate(gunBundle.LoadAsset<GameObject>("Beretta M9 Sketchfab"));
                    gun.AddComponent<GunScript>();
                    gun.SetActive(false);
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        gun.SetActive(!gun.activeSelf);
                    }
                }
            }
        }
    }

    public class GunScript : MonoBehaviour
    {
        ParticleSystem particleSystem;
        GameObject crosshair;

        private void Start()
        {
            transform.SetParent(Plugin.Player.transform);
            transform.localPosition = new Vector3(-0.2836f, 0.5345f, 0.271f);
            transform.localEulerAngles = new Vector3(1f, 270f, 3f);
            particleSystem = GetComponentInChildren<ParticleSystem>();
            crosshair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(crosshair.GetComponent<BoxCollider>());
            crosshair.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                particleSystem.Play();

                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20f) && hit.transform.parent.CompareTag("Villager"))
                {
                    Destroy(hit.transform.parent.gameObject);
                }
            }
        }

        void LateUpdate()
        {
            crosshair.transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.down);
            crosshair.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.2f;
        }
    }

    public class Billboard : MonoBehaviour
    {
        private float timer = 0f;
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
            if (Time.time != 0) { timer += Time.deltaTime; }

            if (timer >= 12f && timer <= 13f)
            {
                _splashController.FadeIn();
            }

            if (timer >= 13f && timer <= 14f)
            {
                Camera.main.transform.SetParent(null);
            }

            if (timer >= 13.5f && timer <= 15f)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                GameObject.Find("Directional Light").SetActive(false);
                Camera.main.cullingMask = 1 << LayerMask.NameToLayer("Player");
                _splashController.FadeOut();
            }

            if (timer >= 20f)
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

    public class BabyValidZone : MonoBehaviour
    {
        private float timer = 0f;
        private GameObject Baby;
        private GameObject originalBaby = GameObject.Find("Village1/Villagers/BabyScene/NewVillagers/Mom/DefinitiveRiggedVillager_Woman/root/ctrl.torso/hip/spine.001/Baby/");

        void Start()
        {
            Baby = Instantiate(originalBaby);
            Baby.SetActive(false);
            Baby.transform.SetParent(GameObject.Find("FPSController").transform);

        }

        void Update()
        {
            if (Time.time != 0) { timer += Time.deltaTime; }

            if (timer >= 15.7f)
            {
                Destroy(originalBaby);
                Baby.SetActive(true);
                Baby.name = "Baby (mod)";
                Baby.transform.localPosition = new Vector3(0.0618f, -0.2254f, -1.2563f);
                Baby.transform.rotation = Quaternion.Euler(343.3828f, 141.8492f, 350.3807f);
                Baby.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            if (timer >= 17)
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
