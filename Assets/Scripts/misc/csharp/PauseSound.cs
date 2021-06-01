using UnityEngine;

public class PauseSound : MonoBehaviour
{
    private bool _paused;
    private AudioSource[] _audioSources;
    public bool ZeroVolume = false;

    private float[] _currentVolume;

    void Start()
    {
        _paused = false;

        var c = gameObject.GetComponents<AudioSource>();

        if (c == null || c.Length <= 0)
        {
            if (audio != null)
            {
                _audioSources = new AudioSource[1];
                _currentVolume = new float[1];
                _audioSources[0] = audio;
            }
            else
            {
                Destroy(this);
            }
        }
        else
        {
            _audioSources = new AudioSource[c.Length];
            _currentVolume = new float[c.Length];

            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == null) continue;

                _audioSources[i] = c[i];
                _currentVolume[i] = _audioSources[i].volume;
            }
        }
    }

    void Update()
    {
        int i;

        if (GameManager.pause)
        {
            if (!_paused)
            {
                _paused = true;

                for (i = 0; i < _audioSources.Length; i++)
                {
                    if (_audioSources[i] == null) continue;

                    if (!ZeroVolume)
                    {
                        _audioSources[i].Pause();
                    }
                    else
                    {
                        _audioSources[i].volume = 0.0f;
                    }
                }
            }
        }
        else
        {
            if (_paused)
            {
                _paused = false;

                for (i = 0; i < _audioSources.Length; i++)
                {
                    if (_audioSources[i] == null) continue;

                    if (!ZeroVolume)
                    {
                        _audioSources[i].Play();
                    }
                    else
                    {
                        _audioSources[i].volume = _currentVolume[i];
                    }
                }
            }
        }
    }
}
