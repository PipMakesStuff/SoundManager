using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Audio/ Music Manager")]
public class MusicManager : MonoBehaviour
{
    /// <summary>
    /// 
    /// Usable Public Functions:
    /// 
    /// MusicManager.SetVolume(value) <- This will set the game's volume.
    /// 
    /// MusicManager.Mute() <- This will mute the game.
    /// 
    ///                                               This changes the track to the value given 
    /// MusicManager.ChangeTrack(value) <-             && activates the onTrackChange() 
    ///                                               event if any methods are subbed to it.
    /// 
    /// MusicManager.ToggleLayer(int layer, int track, float intendedVol) <- take a layer, from a certain track and enables it if it's disabled and vise versa. 
    ///                                                                      The function is overloaded to that at minimum it only needs the layer number.
    /// </summary>

    #region Singleton

    public static MusicManager instance;

    private void Awake()
    {
        if (!instance)
        {

            Initialize();
            if (persist)
            {
                DontDestroyOnLoad(this.gameObject);
            }
            instance = this;
        }
        else
        {
            Destroy(this);
        }

    }

    #endregion

    #region Variables

    [Header("NOTE: Do NOT change this in runtime.")]
    public int index;
    private int previousIndex;
    [Tooltip("Allows music to persist between scene transitions")]
    public bool persist;
    [Range(0,1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;

    public Slider masterVolumeSlider, musicVolumeSlider;

    private List<GameObject> layers = new List<GameObject>();

    [Header("LayerPrefab")]
    public GameObject layer;

    #endregion

    #region Audio Structure
    [Serializable]
    public struct Layer
    {
        public string name;
        [Range(0,1)]
        public float volume;
        [Tooltip("A rate of 0 is instant change to whatever value volume is set while anything above that slowly lerps towards the intended value with the higher the rate the faster the transition.")]
        public float rate;
        [HideInInspector]
        public AudioSource audioSource;
        [Tooltip("This is the actual sound the object will play.")]
        public AudioClip clip;
    }

    [Serializable]
    public struct Audio
    {
        public string name;
        [Range(0, 1)]
        public float trackVolume;
        public float trackRate;
        public Layer[] layers;
    }
    public Audio[] audioTracks;
    #endregion

    private void Start()
    {      
        onTrackChange += ChangeTrack;
        onLayerToggle += ToggleLayer;
        Cycle(index, 0);
        //ChangeTrack(index);
    }

    private void Update()
    {
        Cycle(index, 0);
        SetVolume(masterVolume);
        if (masterVolumeSlider!=null)
        {
            SetVolume(masterVolumeSlider.value);
        }
        if (musicVolumeSlider!=null)
        {
            musicVolume = musicVolumeSlider.value;
        }
    }

    private void OnDestroy()
    {
        onTrackChange -= ChangeTrack;
        onLayerToggle -= ToggleLayer;
    }
    

    #region Private Functions
    [ContextMenu("Generate")]    
    private void Initialize()
    {
        Clear();
       

        for (int i = 0; i < audioTracks.Length; i++)
        {
            AssembleLayers(i);
        }
        
        void AssembleLayers(int i)
        {
            for (int y = 0; y < audioTracks[i].layers.Length; y++)
            {
                MakeLayer(i, y);
            }
        }

        void MakeLayer(int i, int y)
        {
            GameObject _layer = Instantiate(layer, transform.position, Quaternion.identity);
            _layer.name = "Layer_" + i + "_" + y;
            _layer.transform.parent = this.transform;
            layers.Add(_layer);
            audioTracks[i].layers[y].audioSource = _layer.GetComponent<AudioSource>();
            audioTracks[i].layers[y].audioSource.clip = audioTracks[i].layers[y].clip;
            audioTracks[i].layers[y].audioSource.volume = 0;
            audioTracks[i].layers[y].audioSource.Stop();
            audioTracks[i].layers[y].audioSource.Play();
        }        
    }

    [ContextMenu("Clear Generation")]
    void Clear()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            DestroyImmediate(layers[i]);
        }
        layers.Clear();
    }

    private void Cycle(int _index, int? rate = null, float _volumeOveride = 1)
    {
        if (rate != null)
        {
            for (int i = 0; i < audioTracks[_index].layers.Length; i++)
            {
                AdjustVolume(audioTracks[_index].layers[i].volume * audioTracks[index].trackVolume * musicVolume * _volumeOveride, audioTracks[_index].layers[i].audioSource, audioTracks[_index].layers[i].rate* audioTracks[_index].trackRate);

            }
        }
        else
        {

            for (int i = 0; i < audioTracks[_index].layers.Length; i++)
            {
                AdjustVolume(audioTracks[_index].layers[i].volume * audioTracks[index].trackVolume * musicVolume * _volumeOveride, audioTracks[_index].layers[i].audioSource, 0);

            }

        }
               
    }
    private void ChangeTrack()
    {
        for (int i = 0; i < audioTracks[previousIndex].layers.Length; i++)
        {
            AdjustVolume(0, audioTracks[previousIndex].layers[i].audioSource, 0);

        }                  
    }
    private void AdjustVolume(float _target, AudioSource _audio, float _rate)
    {
        float temp = _audio.volume;
        if (_rate > 0)
        {
            if (temp != _target*musicVolume)
            {              
                temp = Mathf.Lerp(_audio.volume, _target*musicVolume, Time.deltaTime * _rate);               
            }
        }
        else
        {
            if (temp != _target*musicVolume)
            {
                temp = _target*musicVolume;
                
            }
        }

        _audio.volume = temp;
    }
   
    #endregion

    #region Public Functions
    public static void SetVolume(float value)
    {
        if (instance.masterVolume!=value)
        {
            instance.masterVolume = value;
        }        
        AudioListener.volume = value;
        
    }
    public static void Mute()
    {
        instance.masterVolume = 0;
        AudioListener.volume = 0;
    }

    public event Action onTrackChange;
    public static void ChangeTrack(int i)
    {
        instance.previousIndex = instance.index;
        instance.index = i;
        //Debug.Log(i + " " + index);
        if (instance.onTrackChange !=null)
        {
            instance.onTrackChange();
        }
    }
    
    public static void NextTrack()
    {
        if (instance.index < instance.audioTracks.Length-1)
        {
            //Debug.Log("increased index");
            ChangeTrack(instance.index +1);
        }
        else
        {
            //Debug.Log("Index Back to 0");
            ChangeTrack(0);
        }
        
    }

    #region Toggle Layer

    public event Action<int, int, float, string> onLayerToggle;
    public static void ToggleLayerTrigger(int layer, int track, float intendedVol, string ID)
    {        
        if (instance.onLayerToggle != null)
        {
            instance.onLayerToggle(layer,  track,  intendedVol,  ID);
        }
    }
    public static void ToggleLayer(int layer)
    {
        if (layer <= instance.audioTracks[instance.index].layers.Length)
        {
            if (instance.audioTracks[instance.index].layers[layer].volume == 0)
            {
                instance.audioTracks[instance.index].layers[layer].volume = 1;
            }
            else
            {
                instance.audioTracks[instance.index].layers[layer].volume = 0;
            }
        }
        
    }

    public static void ToggleLayer(int layer, int track)
    {
        if (instance.audioTracks[track].layers[layer].volume == 0)
        {
            instance.audioTracks[track].layers[layer].volume = 1;
        }
        else
        {
            instance.audioTracks[track].layers[layer].volume = 0;
        }
    }

    public static void ToggleLayer(int layer, int track, float intendedVol)
    {
        if (instance.audioTracks[track].layers[layer].volume == 0)
        {
            instance.audioTracks[track].layers[layer].volume = intendedVol;
        }
        else
        {
            instance.audioTracks[track].layers[layer].volume = 0;
        }
    }

    public static void ToggleLayer(int layer, int track, float intendedVol, string ID)
    {
        if (instance.audioTracks[track].layers[layer].name == ID)
        {
            if (instance.audioTracks[track].layers[layer].volume == 0)
            {
                instance.audioTracks[track].layers[layer].volume = intendedVol;
            }
            else
            {
                instance.audioTracks[track].layers[layer].volume = 0;
            }
        }
        
    }
    #endregion


    #endregion
}
