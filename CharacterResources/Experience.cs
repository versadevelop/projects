using System;
using Tears_Of_Void.Saving;
using UnityEngine;

namespace Tears_Of_Void.Resources
{
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] float experiencePoints = 0;

        public event Action onExperienceGained;

        ExperienceBar UpdateExperienceBar;

        private void Awake() {
            UpdateExperienceBar = GetComponent<ExperienceBar>();
        }
        public void GainExperience(float experience)
        {
            experiencePoints += experience;
            onExperienceGained();
            
            UpdateExperienceBar.UpdateExperienceBar(experience);
        }

        public float GetExperiencePoints()
        {
            return experiencePoints;
        }

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void RestoreState(object state)
        {
            experiencePoints = (float)state;
        }
    }
}

