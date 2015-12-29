/** some utilities that should be there anyway */
using UnityEngine;

namespace CSG {
    public struct TransformData {
        public Vector3 position;
        public Quaternion rotation;
        
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;
        
    }

    public static class TransformUtils {
        
        public static TransformData SaveData(this Transform transform) {
            TransformData tl = new TransformData();
            Transform tr = transform;
            
            tl.position = tr.position;
            tl.localPosition = tr.localPosition;
            
            tl.rotation = tr.rotation;
            tl.localRotation = tr.localRotation;
            
            tl.localScale = tr.localScale;

            return tl;
        }

        public static void RestoreData(this Transform transform, TransformData tr) {

            Transform tl = transform;
            
            tl.position = tr.position;
            tl.localPosition = tr.localPosition;
            
            tl.rotation = tr.rotation;
            tl.localRotation = tr.localRotation;
            
            tl.localScale = tr.localScale;


        }

    }
}
