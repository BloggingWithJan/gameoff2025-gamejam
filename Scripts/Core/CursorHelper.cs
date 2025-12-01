using UnityEngine;
using System.Collections.Generic;

namespace GameJam.Core
{
    public class CursorHelper : MonoBehaviour
    {
        public static CursorHelper Instance { get; private set; }

        public enum CursorType
        {
            Default,
            Pointer,
            Attack,
            Move,
            InteractWithBuilding,
            NoInteractionPossible,
        }

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField]
        CursorMapping[] cursorMappings = null;

        // Cache scaled textures so they aren't rebuilt every time
        private readonly Dictionary<CursorType, Texture2D> scaledCache = new();
        private readonly Dictionary<CursorType, Vector2> hotspotCache = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            SetCursor(CursorType.Default);
        }

        public void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);

            Texture2D texture;
            Vector2 hotspot;

            // Scale only once, store in cache
            if (scaledCache.ContainsKey(type))
            {
                texture = scaledCache[type];
                hotspot = hotspotCache[type];
            }
            else
            {
                float scale = GetCursorScale();

                texture = ScaleTexture(mapping.texture, scale);
                hotspot = mapping.hotspot * scale;

                scaledCache[type] = texture;
                hotspotCache[type] = hotspot;
            }

            Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        private float GetCursorScale()
        {
            return Screen.height / 1080f;
        }

        private Texture2D ScaleTexture(Texture2D source, float scale)
        {
            int width = Mathf.RoundToInt(source.width * scale);
            int height = Mathf.RoundToInt(source.height * scale);

            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }
    }
}
