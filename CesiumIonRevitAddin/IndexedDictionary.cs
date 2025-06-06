﻿using System;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    internal class IndexedDictionary<T>
    {
        public List<T> List { get; } = new List<T>();
        public string CurrentKey { get; private set; }

        public bool Contains(string uuid)
        {
            return dict.ContainsKey(uuid);
        }
        public int CurrentIndex
        {
            get { return dict[CurrentKey]; }
        }

        public T CurrentItem => this.List[this.dict[this.CurrentKey]];

        public T GetElement(string uuid)
        {
            int index = GetIndexFromUUID(uuid);
            return this.List[index];
        }

        public Dictionary<string, T> Dict
        {
            get
            {
                output.Clear();
                foreach (var kvp in this.dict)
                {
                    output.Add(kvp.Key, this.List[kvp.Value]);
                }

                return output;
            }
        }

        public void Reset()
        {
            dict.Clear();
            this.List.Clear();
            Dict.Clear();
            CurrentKey = string.Empty;
        }

        public bool AddOrUpdateCurrent(string uuid, T elem)
        {
            if (!dict.ContainsKey(uuid))
            {
                this.List.Add(elem);
                dict.Add(uuid, this.List.Count - 1);
                CurrentKey = uuid;
                return true;
            }

            CurrentKey = uuid;
            return false;
        }

        public int GetIndexFromUuid(string uuid)
        {
            if (dict.TryGetValue(uuid, out int index))
            {
                return index;
            }

            throw new KeyNotFoundException($"Specified item with UUID '{uuid}' could not be found.");
        }

        public bool AddOrUpdateCurrentMaterial(string uuid, T elem, bool doubleSided)
        {
            if (!dict.ContainsKey(uuid))
            {
                List.Add(elem);
                dict.Add(uuid, List.Count - 1);
                CurrentKey = uuid;
                return true;
            }

            CurrentKey = uuid;

            if (GetElement(uuid) is GltfMaterial mat)
            {
                mat.DoubleSided = doubleSided;
            }

            return false;
        }

        private readonly Dictionary<string, int> dict = new Dictionary<string, int>();
        private readonly Dictionary<string, T> output = new Dictionary<string, T>();

        private int GetIndexFromUUID(string uuid)
        {
            try
            {
                return dict[uuid]; // ignore Intellisense
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Log("Specified item could not be found in IndexedDictionary: " + uuid);
            }

            catch (Exception ex)
            {
                Logger.Instance.Log("Error getting the specified item " + uuid + " in IndexedDictionary: " + ex.Message);
            }

            return -1;
        }
    }
}
