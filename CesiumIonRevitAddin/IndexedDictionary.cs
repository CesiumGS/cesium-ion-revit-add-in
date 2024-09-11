using System;
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

        public T CurrentItem
        {
            get { return this.List[this.dict[this.CurrentKey]]; }
        }

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
            try
            {
                return dict[uuid];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Specified item could not be found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting the specified item {ex.Message}");
            }
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

        readonly Dictionary<string, int> dict = new Dictionary<string, int>();
        readonly Dictionary<string, T> output = new Dictionary<string, T>();

        int GetIndexFromUUID(string uuid)
        {
            try
            {
                return dict[uuid]; // ignore Intellisense
            }
            catch (KeyNotFoundException)
            {
                // TODO: handle error better
                Autodesk.Revit.UI.TaskDialog.Show("IndexedDictionary.h", "Specified item could not be found.");
            }

            catch (System.Exception ex)
            {
                // TODO: handle error better
                Autodesk.Revit.UI.TaskDialog.Show("IndexedDictionary.h", "Error getting the specified item: " + ex.Message);
            }

            return -1;
        }
    }
}
