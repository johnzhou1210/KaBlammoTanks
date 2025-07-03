using UnityEngine;

public class ExplosionCue : MonoBehaviour
{
    public void DoExplodeSound(){
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/Explosion"), 1f);
    }
}
