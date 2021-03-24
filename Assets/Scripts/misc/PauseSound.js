#pragma strict

class PauseSound extends MonoBehaviour
{
    private var _paused : boolean;
    private var _audioSources : AudioSource[];
    public var ZeroVolume : boolean = false;

    private var _currentVolume : float[];

    function Start()
    {
        _paused = false;

        var c : Component[] = gameObject.GetComponents(AudioSource) as Component[];
        
        if(c == null || c.Length <= 0)
        {
            if(audio != null)
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

            for(var i : int = 0; i < c.Length; i++)
            {
                if(c[i] == null) continue;

                _audioSources[i] = c[i] as AudioSource;    
                _currentVolume[i] = _audioSources[i].volume;
            }
        }
    }

    function Update()
    {
        var i : int;

        if(GameManager.pause)
        {
            if(!_paused)
            {
                _paused = true;

                for(i = 0; i < _audioSources.Length; i++)
                {
                    if(_audioSources[i] == null) continue;

                    if(!ZeroVolume)
                    {
                        _audioSources[i].Pause();
                    }
                    else
                    {
                        _audioSources[i].volume = 0.0;
                    }
                }
            }
        }
        else
        {
            if(_paused)
            {
                _paused = false;

                for(i = 0; i < _audioSources.Length; i++)
                {
                    if(_audioSources[i] == null) continue;

                    if(!ZeroVolume)
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