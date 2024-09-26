using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Lolbit : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMNeedyModule Needy;

   public Renderer[] Statics;

   public AudioSource Music;

   bool Stupidthingactive;

   bool Started;
   public GameObject LOL;
   public GameObject[] Lolbits;

   LolbitSettings Settings = new LolbitSettings();

   bool[] Typed = new bool[3];
   private float waitTimeMin = 20f;
   private float waitTimeMax = 31f;
   private bool TwitchPlaysActive;

   Coroutine Active;
   Coroutine PleaseStandBy;

   //Music
   static bool Playing;
   private float DefaultGameMusicVolume;

   int AiLevel = 20;

   static int ModuleIdCounter = 1;
   int ModuleId;

   class LolbitSettings {
      public bool PlayVentablack = true;
      public int AiLevel = 20;
   }

   static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
   {
      new Dictionary<string, object>
      {
         { "Filename", "LolbitSettings.json" },
         { "Name", "Lolbit Settings" },
         { "Listing", new List<Dictionary<string, object>>{
            new Dictionary<string, object>
            {
               { "Key", "Ventablack" },
               { "Text", "The module will play Ventablack instead of the KTANE OST when Lolbit is on the bomb." },
               
            },
            new Dictionary<string, object>
            {
               { "Key", "AI Level" },
               { "Text", "Adjusts Lolbit's AI level, goes from 1-50 with a default of 20. Higher AI level means more of a chance to appear." }

            }
         } }
      }
   };

   void OnDestroy()
   {
      Playing = false;
      try
      {
          Music.Stop();
      }
      catch
      {
          throw new Exception("YOU MUST CONSTRUCT ADDITIONAL PYLONS");
      }
      finally
      {
          GameMusicControl.GameMusicVolume = DefaultGameMusicVolume;
      }
    }

   void Awake () {

      ModuleId = ModuleIdCounter++;




      if (!Application.isEditor) {
         ModConfig<LolbitSettings> modConfig = new ModConfig<LolbitSettings>("Lolbit");
         //Read from the settings file, or create one if one doesn't exist
         Settings = modConfig.Settings;
         //Update the settings file in case there was an error during read
         modConfig.Settings = Settings;
         AiLevel = Settings.AiLevel;
      }

      string missionDesc = KTMissionGetter.Mission.Description;
      if (missionDesc != null) {
         Regex regex = new Regex(@"\^LolbitAI=$(true|false)");
         var match = regex.Match(missionDesc);
         if (match.Success) {
            string[] options = match.Value.Replace("[LolbitAI=] ", "").Split(',');
            int value = 20;
            int.TryParse(options[0], out value);

            Settings.AiLevel = value;
         }
      }

      if (AiLevel < 1) {
         AiLevel = 1;
      }
      else if (AiLevel > 50) {
         AiLevel = 50;
      }

      Needy.OnNeedyActivation += OnNeedyActivation;
      try {
         DefaultGameMusicVolume = GameMusicControl.GameMusicVolume;
         if (Settings.PlayVentablack || Application.isEditor) {
            try { GameMusicControl.GameMusicVolume = 0.0f; } catch (Exception) { }
            if (!Playing) {
               Music.Play();
               Playing = true;
            }
         }
      }
      catch (Exception) { }
      
      GetComponent<KMGameInfo>().OnStateChange += state => {
         if (state == KMGameInfo.State.Transitioning) {
            Playing = false;
            try {
               if (Music.isPlaying)
               Music.Stop();
            }
            catch {
               throw new Exception("YOU MUST CONSTRUCT ADDITIONAL PYLONS");
            }
            finally {
               GameMusicControl.GameMusicVolume = DefaultGameMusicVolume;
            }
         }
      };

      StartCoroutine(Static());
      LOL.SetActive(false);
      for (int i = 0; i < 3; i++) {
         Lolbits[i].SetActive(false);
      }
   }

   protected void OnNeedyActivation () {
      /*if (TwitchPlaysActive) tf was this quinn
      {
         waitTimeMin = 35f;
         waitTimeMax = 55f;
      }*/
      Stupidthingactive = true;
      StartCoroutine(ChangeTime());
      StartCoroutine(Wait());
   }

   IEnumerator Wait () {
      Started = false;
      int randTime = Rnd.Range(7, 10);
      //Debug.Log(AiLevel);
      /*if (Application.isEditor) {
         randTime = 10;
      }*/
      int MovementOpportunity = Rnd.Range(1, 51);
      yield return new WaitForSeconds(randTime);
      Debug.Log(MovementOpportunity);
      if (MovementOpportunity <= AiLevel) {
         Started = true;
         Active = StartCoroutine(Activation());
      }
      else {
         Reset();
      }
   }

   IEnumerator Activation () {
      for (int i = 0; i < 2; i++) {
         Lolbits[i].SetActive(true);
         for (int j = 0; j < 150 - AiLevel * 3; j++) {
            yield return new WaitForSeconds(.01f); //1ish frame
         }
      }
      Lolbits[2].SetActive(true);
      PleaseStandBy = StartCoroutine(GetLolled());
      yield return new WaitForSeconds(6.67f); //400ish frames
      Reset();
      StopCoroutine(PleaseStandBy);
   }

   IEnumerator GetLolled () {
      LOL.SetActive(true);
      while (true) {
         Audio.PlaySoundAtTransform("LOLnoise", transform);
         yield return new WaitForSeconds(.17f);
      }
   }

   IEnumerator Static () {
      float a = Rnd.Range(0f, 1f);
      float b = Rnd.Range(0f, 1f);
      float c = Rnd.Range(0f, 1f);
      while (true) {
         a += .01f;
         b += .01f;
         c += .01f;
         Statics[0].material.SetTextureOffset("_MainTex", new Vector3(a, Rnd.Range(0f, 1f)));
         Statics[1].material.SetTextureOffset("_MainTex", new Vector3(b, Rnd.Range(0f, 1f)));
         Statics[2].material.SetTextureOffset("_MainTex", new Vector3(c, Rnd.Range(0f, 1f)));
         yield return new WaitForSeconds(.03f);
      }
   }

   IEnumerator ChangeTime () {
      int Iteration = 0;
      int[] WASTHATTHEBTIEDIOAWNDOWNAODWNOW = { 1, 9, 8, 3}; // this 7 used to be a 3 no fucking way deaf IT'S FUCKING 1983 YOU ACTUAL RETARD
      while (true) {
         Needy.SetNeedyTimeRemaining(WASTHATTHEBTIEDIOAWNDOWNAODWNOW[Iteration % 4] * 10 + WASTHATTHEBTIEDIOAWNDOWNAODWNOW[(Iteration + 1) % 4]);
         yield return new WaitForSeconds(1f);
         Iteration++;
      }
   }

   void Start () {
      if (GetMissionID() == "mod_ThiccBombs_the47better") {
         Audio.PlaySoundAtTransform("Freddy_Intro", transform);
      }
      Debug.LogFormat("[Lolbit #{0}] The AI level for Lolbit is set to {1}. The default is 20, any lower is probably cheating on a challenge bomb :)", ModuleId, AiLevel);
   }

   private string GetMissionID () {
      try {
         Component gameplayState = GameObject.Find("GameplayState(Clone)").GetComponent("GameplayState");
         Type type = gameplayState.GetType();
         FieldInfo fieldMission = type.GetField("MissionToLoad", BindingFlags.Public | BindingFlags.Static);
         return fieldMission.GetValue(gameplayState).ToString();
      }

      catch (NullReferenceException) {
         return "undefined";
      }
   }

   void Update () {
      if (Stupidthingactive) {
         Needy.SetNeedyTimeRemaining((float) Math.Ceiling(Needy.GetNeedyTimeRemaining()));
      }
      if (Started) {
         if (Input.GetKeyDown(KeyCode.L) && !Typed[0]) {
            Typed[0] = true;
            Audio.PlaySoundAtTransform("LType", transform);
         }
         else if (Input.GetKeyDown(KeyCode.O) && Typed[0] && !Typed[1]) {
            Typed[1] = true;
            Audio.PlaySoundAtTransform("OType", transform);
         }
         else if (Input.GetKeyDown(KeyCode.L) && !Typed[2] && Typed[0] && Typed[1]) {
            Typed[2] = true;
            Audio.PlaySoundAtTransform("LType", transform);
         }
         if (Typed[0] && Typed[1] && Typed[2]) {
            Reset();
         }
      }
   }

   void Reset () {
      if (Active != null) {
         StopCoroutine(Active);
      }
      if (PleaseStandBy != null) {
         StopCoroutine(PleaseStandBy);
      }
      for (int i = 0; i < 3; i++) {
         Lolbits[i].SetActive(false);
         Typed[i] = false;
      }
      LOL.SetActive(false);
      StartCoroutine(Wait());
   }
   

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} LOL. Think next time before you use !{0} help.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim().ToUpper();
      yield return null;
      if (Command == "LOL" && Started) {
         Audio.PlaySoundAtTransform("LType", transform);
         yield return new WaitForSeconds(.1f);
         Audio.PlaySoundAtTransform("OType", transform);
         yield return new WaitForSeconds(.1f);
         Audio.PlaySoundAtTransform("LType", transform);
         yield return new WaitForSeconds(.1f);
         Reset();
      }
      else {
         yield return "sendtochaterror How the fuck.";
      }
   }

   void TwitchHandleForcedSolve () {
      StartCoroutine(DoTheThugShaker());
   }

   IEnumerator DoTheThugShaker () {
      while (true) {
         if (!Started) {
            yield return null;
         }
         else {
            yield return ProcessTwitchCommand("LOL");
         }
      }
   }
}
