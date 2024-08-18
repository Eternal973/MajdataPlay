using MajdataPlay.Game.Notes;
using MajdataPlay.IO;
using MajSimaiDecode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance;

    AudioSampleWrap audioSample;
    SimaiProcess Chart;
    SongDetail song;
    SettingManager settingManager => SettingManager.Instance;

    NoteLoader noteLoader;

    Text ErrorText;

    public GameObject notesParent;
    public GameObject tapPrefab;

    public float noteSpeed = 9f;
    public float touchSpeed = 7.5f;

    public float AudioTime = 0f;
    public bool isStart => audioSample.GetPlayState();
    public float CurrentSpeed = 1f;

    private float AudioStartTime = -114514f;

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        print(GameManager.Instance.selectedIndex);
        song = GameManager.Instance.songList[GameManager.Instance.selectedIndex];
    }

    private void OnPauseButton(object sender,InputEventArgs e)
    {
        if (e.IsButton && e.IsClick && e.Type== MajdataPlay.Types.SensorType.P1) {
            print("Pause!!");
            BackToList();
        }
    }

    void Start()
    {
        IOManager.Instance.BindAnyArea(OnPauseButton);
        audioSample = AudioManager.Instance.LoadMusic(song.TrackPath);
        ErrorText = GameObject.Find("ErrText").GetComponent<Text>();
        try
        {
            var maidata = song.InnerMaidata[GameManager.Instance.selectedDiff];
            if (maidata == "" || maidata == null) {
                BackToList();
                return;
            }
                
            Chart = new SimaiProcess(maidata);
            if (Chart.notelist.Count == 0)
            {
                BackToList();
                return;
            }
            else
            {
                StartCoroutine(DelayPlay());
            }
        }
        catch (Exception ex)
        {
            ErrorText.text = "分割note出错了哟\n+" + ex.Message;
            Debug.LogError(ex);
        }
    }

    IEnumerator DelayPlay()
    {
        var settings = settingManager.SettingFile;

        yield return new WaitForEndOfFrame();
        var BGManager = GameObject.Find("Background").GetComponent<BGManager>();
        if (song.VideoPath != null)
        {
            BGManager.SetBackgroundMovie(song.VideoPath);
        }
        else
        {
            BGManager.SetBackgroundPic(song.SongCover);
        }
        BGManager.SetBackgroundDim(settings.BackgroundDim);

        yield return new WaitForEndOfFrame();
        noteLoader = GameObject.Find("NoteLoader").GetComponent<NoteLoader>();
        noteLoader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(settings.TapSpeed + 0.9975f, -0.985558604f)));
        noteLoader.touchSpeed = settings.TouchSpeed;
        try
        {
            noteLoader.LoadNotes(Chart);
        }
        catch (Exception ex)
        {
            ErrorText.text = "解析note出错了哟\n+" + ex.Message;
            Debug.LogError(ex);
            StopAllCoroutines();
        }


        yield return new WaitForEndOfFrame();

        GameObject.Find("Notes").GetComponent<NoteManager>().Refresh();
        yield return new WaitForSeconds(2);
        audioSample.Play();
        AudioStartTime = Time.unscaledTime + (float)audioSample.GetCurrentTime();
    }

    private void OnDestroy()
    {
        audioSample = null;
    }
    int i = 0;
    // Update is called once per frame
    void Update()
    {
        if (audioSample == null) return;
        //Do not use this!!!! This have connection with sample batch size
        //AudioTime = (float)audioSample.GetCurrentTime();
        if (AudioStartTime != -114514f)
        {
            AudioTime = Time.unscaledTime - AudioStartTime - (float)song.First - settingManager.SettingFile.DisplayOffset;
            //print((float)audioSample.GetCurrentTime() - (Time.unscaledTime - AudioStartTime));
        }
        if (i >= Chart.notelist.Count)
            return;
        var delta = Math.Abs(AudioTime - (Chart.notelist[i].time) + settingManager.SettingFile.DisplayOffset - settingManager.SettingFile.AudioOffset);
        if( delta < 0.01) {
            AudioManager.Instance.PlaySFX("answer.wav");
            //print(Chart.notelist[i].time);
            i++;
            
        }
        //TODO: Render notes
    }

    public float GetFrame()
    {
        var _audioTime = AudioTime * 1000;

        return _audioTime / 16.6667f;
    }

    public void BackToList()
    {
        StopAllCoroutines();
        audioSample.Pause();
        audioSample = null;
        IOManager.Instance.UnbindAnyArea(OnPauseButton);
        StartCoroutine(delayBackToList());

    }
    IEnumerator delayBackToList()
    {
        yield return new WaitForEndOfFrame();
        GameObject.Find("Notes").GetComponent<NoteManager>().DestroyAllNotes();
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(1);
    }


    public void EndGame(float acc)
    {
        GameManager.Instance.lastGameResult = acc;
        print("GameResult: "+acc);
        StartCoroutine(delayEndGame());
    }

    IEnumerator delayEndGame()
    {
        yield return new WaitForSeconds(2f);
        audioSample.Pause();
        audioSample = null;
        IOManager.Instance.UnbindAnyArea(OnPauseButton);
        SceneManager.LoadScene(3);
    }

}
