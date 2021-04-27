using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuadTerrain
{
    public class SceneCamera : MonoBehaviour
    {
        private static Transform m_CameraTransform;
        private static Camera m_Camera;
        private static Transform m_LightTransform;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Clear();
            GameObject go = new GameObject();
            go.AddComponent<SceneCamera>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Clear();
        }

        private static void Clear()
        {
            m_LightTransform = null;
            m_CameraTransform = null;
            m_Camera = null;
        }

        private void OnEnable()
        {
            Clear();
        }

        private void OnDisable()
        {
            Clear();
        }

#if UNITY_EDITOR
        private static Camera GetCurrentSceneViewCamera()
        {
            Camera[] sceneviewCameras = UnityEditor.SceneView.GetAllSceneCameras();
            return sceneviewCameras.Length > 0 ? sceneviewCameras[0] : null;
        }
#endif

        public static Transform LightTransform
        {
            get
            {
                if (m_LightTransform == null)
                {
                    var light = RenderSettings.sun;
                    if (light != null)
                    {
                        m_LightTransform = light.transform;
                    }
                }
                return m_LightTransform;
            }
        }

        public static Camera Camera
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (m_Camera == null)
                    {
                        m_Camera = GetCurrentSceneViewCamera();
                    }
                    return m_Camera;
                }

#endif

                if (m_Camera == null)
                {
                    m_Camera = Camera.main;
                }
                return m_Camera;
            }
        }

        public static Transform Transform
        {
            get
            {
                if (m_CameraTransform == null)
                {
                    m_CameraTransform = Camera.transform;
                }
                return m_CameraTransform;
            }
        }


    }
}
