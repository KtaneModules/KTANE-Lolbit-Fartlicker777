using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;
using System.Diagnostics;

public class Lolbit : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMNeedyModule Needy;

   #pragma warning disable 414
      private bool ZenModeActive;
      private bool TimeModeActive;
   #pragma warning restore 414

   public Renderer[] Statics;

   private float currentSecond = 0.0f;

   public AudioSource Music;

   bool Stupidthingactive;
   bool RanOBSCheck = false;

   bool Started;
   public GameObject LOL;
   public GameObject[] Lolbits;

   public static class TimeRemaining {
      public static void FromModule (KMBombModule module, float time) {
         module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").SetValue("TimeRemaining", time);
      }

      public static void FromModule (KMNeedyModule module, float time) {
         module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").SetValue("TimeRemaining", time);
      }
   }

   public static class TimerRate {
      public static float FromModule (KMBombModule module) {
         return module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").GetValue<float>("GetRate");
      }

      public static float FromModule (KMNeedyModule module) {
         return module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").GetValue<float>("GetRate");
      }

      public static void SetFromModule (KMBombModule module, float v) {
         module.GetComponent("BombComponent").GetValue<object>("Bomb").CallMethod<object>("GetTimer").CallMethod("SetRateModifier", v);
      }
   }

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
            string[] options = match.Value.Replace("LolbitAI=", "").Split(',');
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
      //Debug.Log(MovementOpportunity);
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
      UnityEngine.Debug.LogFormat("[Lolbit #{0}] The AI level for Lolbit is set to {1}. The default is 20, any lower is probably cheating on a challenge bomb :)", ModuleId, AiLevel);
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

   private bool IsOBSOpen () {
      RanOBSCheck = true;
      // Check running processes to see if any match the common browser process names
      var runningProcesses = Process.GetProcesses();
      foreach (var process in runningProcesses) {
         try {
            // Check if the process is still running before accessing its properties
            if (!process.HasExited && process.ProcessName.ToLower().Contains("obs")) {
               return true; // Browser found
            }
         }
         catch (InvalidOperationException) {
            // Process has already exited, continue to the next one
            continue;
         }
      }

      return false; // No browser processes found
   }


   void Update () {
      if (Stupidthingactive) {
         Needy.SetNeedyTimeRemaining((float) Math.Ceiling(Needy.GetNeedyTimeRemaining()));
      }

      if (GetMissionID() == "mod_ThiccBombs_VSC") {
         if (!RanOBSCheck) {
            if (!IsOBSOpen()) {
               while (true) {
                  if (!ZenModeActive) {
                     GetComponent<KMNeedyModule>().HandleStrike();
                  }
               }
            }
         }
         

         if (Mathf.Floor(Bomb.GetTime()) != currentSecond) {
            float strikeModifier = 20.0f;

            switch (Bomb.GetStrikes()) {
               case 0: strikeModifier = 20.0f; break;
               case 1: strikeModifier = 19.0f; break;
               case 2: strikeModifier = 18.0f; break;
               case 3: strikeModifier = 17.0f; break;
               default: strikeModifier = 16.0f; break;
            }

            currentSecond = Mathf.Floor(Bomb.GetTime()); //So the original code removes 5/6th of a second, (strikeModifier / 24f), I changed it to / 30f

            TimeRemaining.FromModule(Needy, Bomb.GetTime() - strikeModifier / 30f);
         }
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
