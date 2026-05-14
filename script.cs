using System;
using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

namespace Mod
{
    public class Mod
    {
        public static void Main()
        {
            CategoryBuilder.Create("Weather", "Weather items!", ModAPI.LoadSprite("icon.png"));

            string[] names = { "\u200BTornado EFU", "Tornado EF-0", "Tornado EF-1", "Tornado EF-2", "Tornado EF-3", "Tornado EF-4", "Tornado EF-5", "Rope Tornado", "Multi-Vortex Tornado" };
            
            string[] descriptions = { 
                "Tornado EFU. Lifts small objects and slightly pulls people.",
                "Tornado EF0. It already lifts people, can brake cars and lift them a little.",
                "Tornado EF1. It already throws people high and lifts parked cars.",
                "Tornado EF2. A strong hurricane that sweeps away everything.",
                "Tornado EF3. A very strong tornado that will destroy any building.",
                "Tornado EF4. Get away immediately, it will tear people to pieces.",
                "Tornado EF5. A massive monster that tears apart everything in its path.",
                "Rope Tornado. A thin, highly curved tornado with moderate power.",
                "Multi-Vortex Tornado. A massive, chaotic tornado containing multiple sub-vortices."
            };

            string[] sprites = { "efu.png", "ef0.png", "ef1.png", "ef2.png", "ef3.png", "ef4.png", "ef5.png", "rtornado.png", "mvt.png" };

            float[] liftPowers = { 5f, 10f, 20f, 40f, 100f, 220f, 450f, 25f, 350f };
            float[] pullPowers = { 15f, 25f, 50f, 90f, 200f, 400f, 800f, 60f, 700f }; 
            float[] maxHeights = { 30f, 60f, 150f, 250f, 500f, 850f, 1500f, 350f, 1200f };
            float[] maxSizes =   { 2f, 6f, 18f, 35f, 50f, 80f, 140f, 10f, 130f };
            float[] maxWidths =  { 3f, 8f, 25f, 50f, 90f, 160f, 280f, 12f, 250f };
            int[] lengths =      { 80, 130, 240, 350, 600, 950, 1500, 450, 1300 };

            for (int i = 0; i < 9; i++)
            {
                int index = i;
                ModAPI.Register(
                    new Modification()
                    {
                        OriginalItem = ModAPI.FindSpawnable("Generator"),
                        NameOverride = names[index],
                        NameToOrderByOverride = "Tornado_" + index,
                        DescriptionOverride = descriptions[index], 
                        CategoryOverride = ModAPI.FindCategory("Weather"),
                        ThumbnailOverride = ModAPI.LoadSprite(sprites[index]),
                        AfterSpawn = (Instance) =>
                        {
                            Light light = Instance.GetComponent<Light>();
                            if (light != null) light.enabled = false;

                            PhysicalBehaviour pb = Instance.GetComponent<PhysicalBehaviour>();

                            Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite(sprites[index]);
                            
                            Tornado tornado = Instance.AddComponent<Tornado>();
                            tornado.efLevel = index;
                            tornado.liftPower = liftPowers[index];
                            tornado.pullPower = pullPowers[index];
                            tornado.maxHeight = maxHeights[index];
                            tornado.maxSize = maxSizes[index];
                            tornado.maxWidth = maxWidths[index];
                            tornado.length = lengths[index];
                            tornado.clips = ModAPI.LoadSound("wind.wav");

				float colorValue = 1f - (Mathf.Min(index, 6) * 0.14f);
				tornado.baseColor = new Color(colorValue, colorValue, colorValue, 1f);

				tornado.pivots = new Transform[tornado.length];
				tornado.counter = new float[tornado.length];

				for (int j = 0; j < tornado.length; j++)
				{
				    GameObject obj = new GameObject("p_" + j);
				    
				    SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
				    renderer.sprite = ModAPI.LoadSprite("dust.png");
				    renderer.color = tornado.baseColor;
				    renderer.sortingOrder = 1000 - j;
				    
				    obj.transform.SetParent(Instance.transform);
				    obj.transform.localPosition = Vector3.zero;
				    
				    obj.transform.localScale = Vector3.one; 

				    tornado.pivots[j] = obj.transform;
				    tornado.counter[j] = j * 0.3f;
				}

                            tornado.pivots = pivots;
                            tornado.counter = counter;

                            if (pb != null)
                            {
                                Color[] tColors = new Color[] { Color.white, new Color(0.7f, 0.7f, 0.7f, 1f), new Color(0.4f, 0.4f, 0.4f, 1f), new Color(0.1f, 0.1f, 0.1f, 1f), Color.red, Color.yellow, Color.green, Color.blue, Color.magenta };
                                int[] tColorIdx = new int[] { 0 };
                                pb.ContextMenuOptions.Buttons.Add(new ContextMenuButton("changeColor", "Change Color", "Cycle through colors.", () => {
                                    tColorIdx[0] = (tColorIdx[0] + 1) % tColors.Length;
                                    tornado.baseColor = tColors[tColorIdx[0]];
                                }));
                            }
                        }
                    }
                );
            }

            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Brick"),
                    NameOverride = "Anemometer",
                    NameToOrderByOverride = "Z_Anemometer",
                    DescriptionOverride = "Measures wind speed.\nSends RED signal (Ch 1) if speed > limit.\nSends BLUE signal (Ch 2) if speed drops below limit.\nModes: All, Only Blue, Only Red, Without Signal.",
                    CategoryOverride = ModAPI.FindCategory("Weather"),
                    ThumbnailOverride = ModAPI.LoadSprite("SpeedometerView.png", 1f, false),
                    AfterSpawn = (Instance) =>
                    {
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("Speedometer.png");

                        foreach (var c in Instance.GetComponents<Collider2D>())
                            GameObject.Destroy(c);
                        Instance.AddComponent<BoxCollider2D>();

                        Instance.GetComponent<PhysicalBehaviour>().Properties = ModAPI.FindPhysicalProperties("Metal");
                        Instance.GetOrAddComponent<AnemometerBehaviour>().GlowSprite = ModAPI.LoadSprite("SpeedometerGlow.png");

                        UnityEngine.Object.Destroy(Instance.GetComponent<GeneratorBehaviour>());
                        var phys = Instance.GetComponent<PhysicalBehaviour>();
                        float mass = 1.05f;
                        phys.TrueInitialMass = mass;
                        phys.rigidbody.mass = mass;
                        phys.InitialMass = mass;
                    }
                }
            );
        }
    }

    public class Tornado : MonoBehaviour
    {
        public static HashSet<Tornado> ActiveTornadoes = new HashSet<Tornado>();

        public int efLevel;
        public int length;
        public float liftPower;
        public float pullPower;
        public float maxHeight;
        public float maxWidth;
        public float maxSize;
        public Color baseColor;
        public AudioClip clips;
        public Transform[] pivots;
        public float[] counter;
        public bool active;

        private AudioSource audioSrc;
        private float height, width, size, volume;
        private float rotationSpeed;

        private void Awake()
        {
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.outputAudioMixerGroup = Global.main.SoundEffects;
            audioSrc.spatialBlend = 1f;
            audioSrc.minDistance = 30f;
            audioSrc.maxDistance = 1500f;
            audioSrc.loop = true;
            rotationSpeed = -6f - (Mathf.Min(efLevel, 6) * 3.5f);
        }

        private void OnEnable() => ActiveTornadoes.Add(this);
        private void OnDisable() => ActiveTornadoes.Remove(this);
        private void OnDestroy() => ActiveTornadoes.Remove(this);

        private void Update()
        {
            float target = active ? 1f : -1f;
            float step = Time.deltaTime * 2.5f;

            height = Mathf.Clamp(height + step * target * (maxHeight / 4f), 0f, maxHeight);
            size = Mathf.Clamp(size + step * target * (maxSize / 4f), 0f, maxSize);
            width = Mathf.Clamp(width + step * target * (maxWidth / 4f), 0f, maxWidth);
            volume = Mathf.Clamp01(volume + Time.deltaTime * target);

            if (active && !audioSrc.isPlaying)
            {
                audioSrc.clip = clips;
                audioSrc.Play();
            }
            else if (!active && volume <= 0f && audioSrc.isPlaying)
            {
                audioSrc.Stop();
            }

            audioSrc.volume = volume * 2f;

            UpdateVisuals();
            if (active || height > 0.1f) ApplyPhysics();

            if (active && height > 0.1f && Camera.main != null)
            {
                float radius = (maxWidth * 1.2f) + Mathf.Pow(Mathf.Min(efLevel, 6) + 1, 1.2f) * 10f;
                float distToCam = Vector2.Distance(transform.position, Camera.main.transform.position);
                if (distToCam < radius * 1.5f)
                {
                    float distance = Vector2.Distance(transform.position, Camera.main.transform.position);
                    float maxShakeRange = 30f;

                    if (distance < maxShakeRange)
                    {
                        float distanceFactor = 1f - (distance / maxShakeRange);
                        float intensity = (liftPower / 20f) * distanceFactor;

                        CameraShakeBehaviour.main.Shake(intensity, transform.position);
                    }
                }
            }
        }

        public void Use() => active = !active;

        private void UpdateVisuals()
        {
            float swirlFactor = efLevel == 0 ? 0.05f : (efLevel == 1 ? 0.15f : (efLevel == 2 ? 0.4f : 1.2f + (efLevel * 0.15f)));
            if (efLevel == 7) swirlFactor = 0.8f;
            if (efLevel == 8) swirlFactor = 1.8f;

            float curveExponent = efLevel == 0 ? 3.0f : (efLevel == 1 ? 2.5f : (efLevel == 2 ? 1.8f : (efLevel == 3 ? 1.5f : (efLevel == 4 ? 1.3f : 1.1f))));
            if (efLevel == 7) curveExponent = 2.2f;
            if (efLevel == 8) curveExponent = 1.0f;

            for (int i = 0; i < pivots.Length; i++)
            {
                float progress = (float)i / length;
                float curve = Mathf.Pow(progress, curveExponent);
                
                float x = Mathf.Cos(counter[i]) * width * curve * swirlFactor;
                
                if (efLevel == 7)
                {
                    x += Mathf.Sin(progress * Mathf.PI) * width * 1.5f; 
                }

                if (efLevel == 8)
                {
                    x += Mathf.Sin(counter[i] * 2.5f + progress * 10f) * width * 0.4f;
                }

                pivots[i].position = transform.position + new Vector3(x, height * progress, 0f);
                
                Color pColor = baseColor;
                pColor.a = Mathf.Clamp01(1.2f - progress);
                pivots[i].GetComponent<SpriteRenderer>().color = pColor;
                pivots[i].rotation = Quaternion.identity;
            }
        }

        private void ApplyPhysics()
        {
            float radius = (maxWidth * 1.2f) + Mathf.Pow(Mathf.Min(efLevel, 6) + 1, 1.2f) * 10f; 
            Vector2 origin = transform.position;
            Vector2 areaCenter = origin + new Vector2(0f, height / 2f);
            Vector2 areaSize = new Vector2(radius * 2.5f, height);

            Collider2D[] cols = Physics2D.OverlapBoxAll(areaCenter, areaSize, 0f);

            foreach (Collider2D col in cols)
            {
                if (col.transform.IsChildOf(transform)) continue;
                Rigidbody2D rb = col.attachedRigidbody;
                if (rb == null || rb.isKinematic) continue;

                float effectiveMass = rb.mass;
                Joint2D[] joints = col.GetComponents<Joint2D>();
                foreach (Joint2D joint in joints)
                {
                    if (joint.connectedBody == null || joint.connectedBody.isKinematic)
                    {
                        effectiveMass += 5000f;
                        break;
                    }
                }

                Vector2 targetPos = col.transform.position;
                float dist = Mathf.Abs(origin.x - targetPos.x);
                float dir = (origin.x > targetPos.x) ? 1f : -1f;

                float distanceFactor = Mathf.Pow(Mathf.Clamp01(1f - (dist / radius)), 2.5f);
                float pull = pullPower * distanceFactor;

                if (efLevel == 0) pull *= 0.02f;
                if (efLevel == 1) pull *= 0.05f;
                if (efLevel == 2) pull *= 0.15f;

                Vector2 rayDir = targetPos - origin;
                RaycastHit2D[] hits = Physics2D.RaycastAll(origin, rayDir.normalized, rayDir.magnitude);
                int blockCount = 0;

                foreach (var hit in hits)
                {
                    if (hit.collider != null && hit.collider.transform != col.transform && !hit.collider.transform.IsChildOf(transform) && !hit.collider.isTrigger)
                    {
                        Rigidbody2D hitRb = hit.collider.attachedRigidbody;
                        if (hitRb != null && hitRb.mass > 25f && !hitRb.isKinematic)
                        {
                            blockCount++;
                        }
                        else if (hitRb != null && hitRb.isKinematic)
                        {
                            blockCount += 2;
                        }
                    }
                }

                float blockFactor = Mathf.Clamp01(1f - (blockCount * 0.35f));
                pull *= blockFactor;

                rb.AddForce(new Vector2(dir * pull, 0f));

                float progressY = Mathf.Clamp01((targetPos.y - origin.y) / height);
                float funnelEdge = width * Mathf.Pow(progressY, (efLevel < 3 || efLevel == 7) ? 1.5f : 0.7f);

                if (dist < funnelEdge + 15f)
                {
                    float lift = 0f;
                    float mass = Mathf.Max(effectiveMass, 0.1f);
                    bool isHeavy = mass > 30f;

                    if (efLevel == 0)
                    {
                        lift = isHeavy ? 0 : liftPower * 0.2f * (1f - progressY);
                    }
                    else if (efLevel == 1 || efLevel == 7)
                    {
                        lift = isHeavy ? 0 : liftPower * 0.5f * (1f - progressY);
                    }
                    else if (efLevel == 2)
                    {
                        lift = isHeavy ? liftPower * 0.2f : liftPower * (1.2f - progressY);
                    }
                    else if (efLevel == 3)
                    {
                        lift = isHeavy ? liftPower * 0.5f : liftPower; 
                    }
                    else
                    {
                        lift = liftPower * (1f + Mathf.Min(efLevel, 6) * 0.3f);
                    }

                    lift *= blockFactor * blockFactor;

                    rb.velocity += new Vector2(0f, (lift / mass) * Time.deltaTime);
                    
                    float orbitalResistance = Mathf.Min(pull * 0.8f, lift * 0.3f);
                    rb.AddForce(new Vector2(-dir * orbitalResistance, 0f));

                    if (lift > 2f) rb.AddTorque(rotationSpeed * mass * 0.05f);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            float radius = (maxWidth * 1.2f) + Mathf.Pow(Mathf.Min(efLevel, 6) + 1, 1.2f) * 10f;
            Gizmos.DrawWireCube(transform.position + new Vector3(0f, height / 2f, 0f), new Vector3(radius * 2.5f, height, 1f));
        }
    }

    public class AnemometerBehaviour : MonoBehaviour
    {
        public bool Activated = true;
        private bool isSendingSignal = false;
        public float Speed = 0f;
        public float Threshold = 10f;
        public int SignalMode = 0;
        public int UnitType = 0;

        [SkipSerialisation]
        public TextMeshPro Text;
        [SkipSerialisation]
        public Sprite GlowSprite;

        private Rigidbody2D rb;
        private bool OverLimit = false;
        private LightSprite Light;
        private GameObject Glow = new GameObject("Speed Glow");
        private SpriteRenderer GlowRenderer;

        public void Awake()
        {
            Light = ModAPI.CreateLight(transform, new Color(0f, 0.784313725f, 1f, Mathf.Clamp((Speed / Threshold), 0.01f, 1f) + 0.05f), 1f, 2.5f);
            GlowRenderer = Glow.GetOrAddComponent<SpriteRenderer>();
            Text = GameObject.Instantiate(ModAPI.FindSpawnable("Thermometer").Prefab.GetComponent<ThermometerBehaviour>().TextMesh);
            rb = GetComponent<Rigidbody2D>();
        }

        public string ConvertedText
        {
            get
            {
                float Math = 0f;

                if (UnitType == 0)
                {
                    if (Speed < 200f) Math = 2f;
                    return string.Format("<mspace=0.057>{0} m/s", RoundedSpeed(Math).ToString());
                }
                if (UnitType == 1)
                {
                    if (Speed * 3.6f < 500f) Math = 1f;
                    return string.Format("<mspace=0.057>{0} km/h", (RoundedSpeed(Math, 3.6f)).ToString());
                }
                if (UnitType == 2)
                {
                    if (Speed * 2.236936f < 350f) Math = 1f;
                    return string.Format("<mspace=0.057>{0} mph", (RoundedSpeed(Math, 2.236936f)).ToString());
                }
                return "";
            }
        }

        public float RoundedSpeed(float Power, float Multiplier = 1f)
        {
            float MD = Mathf.Pow(10f, Power);
            float Speedier = Speed * Multiplier;
            float Math = ((Mathf.Round(Speedier * MD)) / MD);
            return (Math > ((1 / MD) * 2)) ? Math : 0f;
        }

        public void Start()
        {
            Transform LightTransform = Light.gameObject.transform;
            LightTransform.localScale = new Vector3(transform.localScale.x * 1.5f, transform.localScale.y);
            LightTransform.localPosition = new Vector3(0f, -0.175f);

            Transform GlowTransform = Glow.transform;
            GlowTransform.SetParent(transform);
            GlowTransform.localPosition = new Vector3(0f, 0f);
            GlowTransform.rotation = transform.rotation;

            GlowRenderer.sprite = GlowSprite;
            GlowRenderer.color = new Color(0f, 0.784313725f, 1f, Mathf.Clamp((Speed / Threshold), 0.01f, 1f) / 5f);
            GlowRenderer.material = ModAPI.FindMaterial("VeryBright");
            GlowRenderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
            GlowRenderer.enabled = false;

            Text.name = "Speed";
            Text.text = ConvertedText;
            Text.sortingLayerID = GetComponent<SpriteRenderer>().sortingLayerID;
            Text.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
            Text.rectTransform.SetParent(transform);
            Text.rectTransform.anchoredPosition = transform.position;
            Text.rectTransform.sizeDelta = new Vector2(Text.rectTransform.sizeDelta.x * 2f, Text.rectTransform.sizeDelta.y);
            Text.rectTransform.transform.localPosition = new Vector3(-0.45f * transform.localScale.x, 0.065f * transform.localScale.y);
            Text.rectTransform.transform.localScale = new Vector2(0.8f * transform.localScale.x, 0.8f * transform.localScale.y);
            Text.rectTransform.transform.rotation = transform.rotation;
            Text.color = Color.white;

            PhysicalBehaviour pb = GetComponent<PhysicalBehaviour>();
            
            pb.ContextMenuOptions.Buttons.Add(new ContextMenuButton("SetSignalSpeed", "Select Signal Speed", "Set threshold for signal", () =>
            {
                DialogBox dialog = (DialogBox)null;
                dialog = DialogBoxManager.TextEntry("Enter wind speed for signal (1 - 200 m/s):\n<size=26>Currently: " + Threshold + " m/s</size>", "Number", new DialogButton("Apply", true, new UnityAction[1]
                {
                    (UnityAction)(() =>
                    {
                        float setrange;
                        if (float.TryParse(dialog.EnteredText, out setrange))
                        {
                            setrange = Mathf.Clamp(setrange, 1f, 200f);
                            Threshold = setrange;
                            ModAPI.Notify("Signal speed set to " + Threshold + " m/s");
                        }
                    })
                }),
                new DialogButton("Cancel", true, (UnityAction)(() => dialog.Close())));
            }));

            pb.ContextMenuOptions.Buttons.Add(new ContextMenuButton("ToggleSignalMode", delegate ()
            {
                if (SignalMode == 0) return "Signal: All";
                if (SignalMode == 1) return "Signal: Only Blue";
                if (SignalMode == 2) return "Signal: Only Red";
                return "Signal: Without Signal";
            }, "Switch signal mode.", new UnityAction[]
             {
                delegate()
                {
                    SignalMode++;
                    if(SignalMode > 3) SignalMode = 0;
                }
            }));

            pb.ContextMenuOptions.Buttons.Add(new ContextMenuButton("ChangeUnit", delegate ()
            {
                if (UnitType == 0) return "Current Unit: m/s";
                if (UnitType == 1) return "Current Unit: km/h";
                return "Current Unit: mph";
            }, "Switch between the units of speed.", new UnityAction[]
             {
                delegate()
                {
                    UnitType++;
                    if(UnitType > 2) UnitType = 0;
                    Text.text = ConvertedText;
                }
            }));

            UpdateUse();
        }

        public void Use()
        {
            if (!this.enabled || isSendingSignal) return; 
            
            Activated = !Activated;
            UpdateUse();
        }

        public void UpdateUse()
        {
            Light.enabled = Activated;
            GlowRenderer.enabled = Activated;
            Text.enabled = Activated;
        }

        private float GetLocalWindSpeed()
        {
            float maxWindDetected = 0f;
            float[] baseSpeeds = { 10f, 35f, 45f, 60f, 74f, 90f, 120f, 40f, 100f };

            foreach (Tornado t in Tornado.ActiveTornadoes)
            {
                if (!t.active) continue;

                float dist = Mathf.Abs(t.transform.position.x - transform.position.x);
                float radius = (t.maxWidth * 1.2f) + Mathf.Pow(Mathf.Min(t.efLevel, 6) + 1, 1.2f) * 10f;
                
                if (dist < radius * 2.5f) 
                {
                    float distanceFactor = Mathf.Pow(Mathf.Clamp01(1f - (dist / radius)), 1.5f);
                    float simulatedWindSpeed = baseSpeeds[t.efLevel] * distanceFactor;
                    
                    if (simulatedWindSpeed > maxWindDetected)
                    {
                        maxWindDetected = simulatedWindSpeed;
                    }
                }
            }
            return maxWindDetected;
        }

        public void FixedUpdate()
        {
            if (!Activated)
            {
                OverLimit = false;
                return;
            }

            float targetSpeed = (rb.velocity.magnitude / 1.2136f) + GetLocalWindSpeed();
            Speed = Mathf.MoveTowards(Speed, targetSpeed, Time.fixedDeltaTime * 5f);

            GlowRenderer.color = new Color(0f, 0.784313725f, 1f, Mathf.Clamp((Speed / Threshold), 0.01f, 1f) / 5f);
            Color color = Light.Color;
            color.a = Mathf.Clamp((Speed / Threshold), 0.01f, 1f) + 0.05f;
            Light.Color = color;
            Text.text = ConvertedText;

            if (!OverLimit && Speed >= Threshold)
            {
                if (SignalMode == 0 || SignalMode == 2)
                {
                    isSendingSignal = true;
                    SendMessage("Use", new ActivationPropagation(transform, 1), SendMessageOptions.DontRequireReceiver);
                    isSendingSignal = false;
                }
                OverLimit = true;
            }
            else if (OverLimit && Speed < Threshold - 0.5f)
            {
                if (SignalMode == 0 || SignalMode == 1)
                {
                    isSendingSignal = true;
                    SendMessage("Use", new ActivationPropagation(transform, 2), SendMessageOptions.DontRequireReceiver);
                    isSendingSignal = false;
                }
                OverLimit = false;
            }
        }
    }
}
