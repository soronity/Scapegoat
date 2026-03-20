using UnityEngine;
using System.Collections.Generic;

public class PortalManager : MonoBehaviour, ISave
{
    private Dictionary<string, bool> _portalStates = new Dictionary<string, bool>();

    public bool IsPortalUnlocked(string portalId)
    {
        if (string.IsNullOrEmpty(portalId))
        {
            Debug.LogWarning("Checking unlock status with null or empty portalId");
            return false;
        }
        return _portalStates.ContainsKey(portalId) && _portalStates[portalId];
    }

    public void UnlockPortal(string portalId)
    {
        if (string.IsNullOrEmpty(portalId))
        {
            Debug.LogWarning("Attempting to unlock with null or empty portalId");
            return;
        }
        _portalStates[portalId] = true;
    }

    public void LoadData(GameData data)
    {
        if (data.portalsUnlocked == null)
        {
            data.portalsUnlocked = new SerializableDictionary<string, bool>();
        }

        _portalStates.Clear();
        foreach (var kvp in data.portalsUnlocked)
        {
            _portalStates[kvp.Key] = kvp.Value;
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.portalsUnlocked == null)
        {
            data.portalsUnlocked = new SerializableDictionary<string, bool>();
        }

        data.portalsUnlocked.Clear();
        foreach (var kvp in _portalStates)
        {
            data.portalsUnlocked[kvp.Key] = kvp.Value;
        }
    }
}