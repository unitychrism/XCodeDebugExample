using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class Counter : MonoBehaviour {

	int lastSecond;
	Text counterText;

	// Use this for initialization
	void Start () {
		lastSecond = 0;
		counterText = GameObject.Find ("CounterText").GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		int newSecond = (int)Time.timeSinceLevelLoad;

		if (newSecond > lastSecond) {
			// Display and log count
			SetCount(newSecond);

			// Throw exception and change bg color every 5 seconds
			if (newSecond % 5 == 0) {
				Debug.Log ("Seconds: " + newSecond);
			}

			lastSecond = newSecond;
		}
	}

	public void SetCount(int count) {
		Debug.Log (string.Format ("Tick: {0}", count));
		counterText.text = count.ToString();
	}
}
