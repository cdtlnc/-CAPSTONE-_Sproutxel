// photon RPC can only pass primitive types, they cannot pass sprite objects directly.
// this registry maps sprite names to actual sprite assets so GhostFarmView
// can look up the right sprite from just a string name sent over the network.

using System.Collections.Generic;
using UnityEngine;

public class SpriteRegistry : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    private static Dictionary<string, Sprite> _lookup;

    private void Awake()
    {
        _lookup = new Dictionary<string, Sprite>();
        foreach (var sprite in sprites)
        {
            Debug.Log($"[SpriteRegistry] Registered: '{sprite.name}'");
            if (sprite != null && !_lookup.ContainsKey(sprite.name))
                _lookup[sprite.name] = sprite;
        }
        Debug.Log($"[SpriteRegistry] Registered {_lookup.Count} sprites.");
    }

    public static Sprite Get(string spriteName)
    {
        if (_lookup == null)
        {
            Debug.LogError("[SpriteRegistry] _lookup is null — Awake() hasn't run yet or SpriteRegistry is inactive.");
            return null;
        }
        _lookup.TryGetValue(spriteName, out Sprite sprite);
        if (sprite == null)
            Debug.LogWarning($"[SpriteRegistry] '{spriteName}' not found. Registered count: {_lookup.Count}");
        return sprite;
    }
}