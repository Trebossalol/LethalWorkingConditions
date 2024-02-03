using System.Collections.Generic;
using UnityEngine;

namespace LethalWorkingConditions.Helpers
{
    internal class ObjectFinder
    {
        public static List<T> FindObjectsInRadius<T>(Transform transform, float radius)
        {
            List<T> objects = new List<T>();

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider collider in colliders)
            {
                T obj = collider.GetComponent<T>();

                if (obj != null) objects.Add(obj);
            }

            return objects;
        }

        public static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
        {
            var objects = Object.FindObjectsOfType<T>();
            return objects;
        }
    }
}
