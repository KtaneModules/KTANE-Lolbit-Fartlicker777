using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Lolbit : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMNeedyModule Needy;

   public Material[] Statics;

   bool Stupidthingactive;

   bool Started;
   public GameObject LOL;
   public GameObject[] Lolbits;

   bool[] Typed = new bool[3];

   Coroutine Waiting;
   Coroutine Active;
   Coroutine PleaseStandBy;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () {
      ModuleId = ModuleIdCounter++;

      Needy.OnNeedyActivation += OnNeedyActivation;

      StartCoroutine(Static());
      LOL.SetActive(false);
      for (int i = 0; i < 3; i++) {
         Lolbits[i].SetActive(false);
      }
      Debug.Log("C");
   }

   protected void OnNeedyActivation () {
      Stupidthingactive = true;
      Debug.Log("A");
      Waiting = StartCoroutine(Wait());
   }

   IEnumerator Wait () {
      Debug.Log("L");
      yield return new WaitForSeconds(Rnd.Range(20, 30));
      Started = true;
      Active = StartCoroutine(Activation());
   }

   IEnumerator Activation () {
      
      for (int i = 0; i < 3; i++) {
         yield return new WaitForSeconds(5f);
         Lolbits[i].SetActive(true);
      }
      PleaseStandBy = StartCoroutine(GetLolled());
   }

   IEnumerator GetLolled () {
      for (int i = 0; i < 21; i++) {
         Audio.PlaySoundAtTransform("LOLnoise", transform);
         LOL.SetActive(true);
         yield return new WaitForSeconds(.48f);
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
         Statics[0].SetTextureOffset("_MainTex", new Vector3(a, Rnd.Range(0f, 1f)));
         Statics[1].SetTextureOffset("_MainTex", new Vector3(b, Rnd.Range(0f, 1f)));
         Statics[2].SetTextureOffset("_MainTex", new Vector3(c, Rnd.Range(0f, 1f)));
         yield return new WaitForSeconds(.01f);
      }
   }

   void Update () {
      if (Stupidthingactive) {
         Needy.SetNeedyTimeRemaining(83);
      }
      if (Started) {
         if (Input.GetKeyDown(KeyCode.L) && !Typed[0]) {
            Typed[0] = true;
         }
         else if (Input.GetKeyDown(KeyCode.O) && !Typed[0]) {
            Typed[1] = true;
         }
         else if (Input.GetKeyDown(KeyCode.L) && !Typed[2] && Typed[0]) {
            Typed[2] = true;
         }
         if (Typed[0] && Typed[1] && Typed[2]) {
            if (Active != null) {
               StopCoroutine(Active);
            }
            if (PleaseStandBy != null) {
               StopCoroutine(PleaseStandBy);
            }
            for (int i = 0; i < 3; i++) {
               Lolbits[i].SetActive(false);
            }
            Waiting = StartCoroutine(Wait());
         }
      }
   }
   

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
}
