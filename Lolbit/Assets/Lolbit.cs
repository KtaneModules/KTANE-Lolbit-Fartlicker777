using System;
using System.Collections;
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

   bool[] Typed = new bool[3];
   private float waitTimeMin = 20f;
   private float waitTimeMax = 31f;
   private bool TwitchPlaysActive;

   Coroutine Active;
   Coroutine PleaseStandBy;

   //Music
   static bool Playing;
   private float DefaultGameMusicVolume;

   void Awake () {

      Needy.OnNeedyActivation += OnNeedyActivation;
      try {
         DefaultGameMusicVolume = GameMusicControl.GameMusicVolume;
         if (!Playing) {
            Music.Play();
            Playing = true;
         }
      }
      catch (Exception) { }
      try { GameMusicControl.GameMusicVolume = 0.0f; } catch (Exception) { }

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
      if (TwitchPlaysActive)
      {
         waitTimeMin = 35f;
         waitTimeMax = 55f;
      }
      Stupidthingactive = true;
      StartCoroutine(ChangeTime());
      StartCoroutine(Wait());
   }

   IEnumerator Wait () {
      Started = false;
      yield return new WaitForSeconds(Rnd.Range(waitTimeMin, waitTimeMax));
      Started = true;
      Active = StartCoroutine(Activation());
   }

   IEnumerator Activation () {
      
      for (int i = 0; i < 2; i++) {
         Lolbits[i].SetActive(true);
         yield return new WaitForSeconds(1f);
      }
      Lolbits[2].SetActive(true);
      PleaseStandBy = StartCoroutine(GetLolled());
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
      int[] WASTHATTHEBTIEDIOAWNDOWNAODWNOW = { 1, 9, 8, 7}; // this 7 used to be a 3 no fucking way deaf
      while (true) {
         Needy.SetNeedyTimeRemaining(WASTHATTHEBTIEDIOAWNDOWNAODWNOW[Iteration % 4] * 10 + WASTHATTHEBTIEDIOAWNDOWNAODWNOW[(Iteration + 1) % 4]);
         yield return new WaitForSeconds(1f);
         Iteration++;
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

   IEnumerator TwitchHandleForcedSolve () {
      while (true) {
         if (!Started) {
            yield return true;
         }
         else {
            yield return ProcessTwitchCommand("LOL");
         }
      }
   }
}
