using UnityEngine;

namespace haw.pd20.training_modules
{
    [CreateAssetMenu(fileName = "HandState", menuName = "PD20/HandState", order = 1)]
    public class HandState : ScriptableObject
    {
        public string Name;
        public Material Material;
    }
}