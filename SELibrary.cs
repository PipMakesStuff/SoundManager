using System.Collections;
using UnityEngine;

/// SELibrary.Play() takes either the index where the soundEffect is store, or the name and then Plays it on a new object instance.
/// This new object is then destroyed at the end of the sound clip duration. 'index' or 'clipname' MUST be passed in, but pitch doesn't need to be modified.
/// 
/// SELibrary.PlayVariance() does much the same, but instead will also take pitch variance amplitude with a default variance of .5;

[AddComponentMenu("Audio/ Sound Effect Library")]
public class SELibrary : MonoBehaviour
{
    [Range(0,1)]
    public float SEVolume = 1;
    public AudioClip[] soundEffects;
    

    #region Singleton
    public static SELibrary instance;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

    }
    #endregion  

    #region Play()
    public static void Play(int index)
    {
        AudioSource audio = instance.CreateTemp();
        instance.StartCoroutine(DelayedPlay(audio, index));
    }
    
    public static void Play(int index, float pitch = 1)
    {
        AudioSource audio = instance.CreateTemp();
        instance.StartCoroutine(DelayedPlay(audio, index, pitch));       
    }
    public static void Play(string clipName)
    {
        AudioSource audio = instance.CreateTemp();
        for (int i = 0; i < instance.soundEffects.Length; i++)
        {
            if (instance.soundEffects[i].name == clipName)
            {
                instance.StartCoroutine(DelayedPlay(audio, i));
            }
        }
    }
    public static void Play(string clipName, float pitch = 1)
    {
        AudioSource audio = instance.CreateTemp();
        for (int i = 0; i < instance.soundEffects.Length; i++)
        {
            if (instance.soundEffects[i].name == clipName)
            {
                instance.StartCoroutine(DelayedPlay(audio, i, pitch));
            }
        }        
    }
    private AudioSource CreateTemp()
    {
        GameObject temp = new GameObject();
        temp.name = "TEMP_SOUND GAMEOBJECT";
        return temp.AddComponent<AudioSource>();
    }

    static IEnumerator DelayedPlay(AudioSource audio, int i, float pitch=1)
    {
        //Debug.Log("Getting ready to fire " + audio + "placing "+ soundEffects[i]+ " as the clip");
        yield return new WaitForSeconds(0.1f);
        audio.clip = instance.soundEffects[i];
        audio.pitch = pitch;
        audio.volume = instance.SEVolume;
        audio.Play();
        //Debug.Log("Fired");
        instance.StartCoroutine(DestroyAtEnd(instance.soundEffects[i], audio.gameObject));
    }

    static IEnumerator DestroyAtEnd(AudioClip clip, GameObject gameObject)
    {
        yield return new WaitForSeconds(clip.length);
        Destroy(gameObject);
    }
    #endregion

    #region PlayWithVariance()
    public static void PlayWithVariance(int i)
    {
        Play(i, Random.Range(-.5f, .5f) + 1);
    }
    public static void PlayWithVariance(int i, float amplitude)
    {
        Play(i, Random.Range(-amplitude, amplitude) + 1);
    }
    public static void PlayWithVariance(string clipname)
    {
        Play(clipname, Random.Range(-.5f, .5f) + 1);
    }
    public static void PlayWithVariance(string clipname, float amplitude)
    {
        Play(clipname, Random.Range(-amplitude, amplitude) + 1);
    }
    #endregion
}
