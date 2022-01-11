using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class pieFlashScript : MonoBehaviour {

	private string pi = "31415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461284756482337867831652712019091456485669234603486104543266482133936072602491412737245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094330572703657595919530921861173819326117931051185480744623799627495673518857527248912279381830119491";

	public KMBombModule Module;
    public KMAudio Audio;
    public KMBombInfo Bomb;
	public KMSelectable[] buttons = new KMSelectable[5];
	public TextMesh[] buttonTexts = new TextMesh[5];

	// Logging
	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved = false;

	private int displayAmt = 3;										// Number of codes generated and displayed per flash
	private int[] codePlaces = new int[] { -1, -1, -1 };	// A field initializer cannot reference the nonstatic field...
	private string[] codes = new string[3];
	private int[] intCodes = new int[3];					// ... so I can't use displayAmt for these.
	private int[] solution;						// set to "new int[] { -1, -1, -1, -1, -1 };" in FindSolution() for readability
	private int solutionIndex;
	private int stage = 1;
	private int[] userAnswer = new int[] { -1, -1, -1, -1, -1 };
    private string[] sounds = { "G", "F", "E", "D", "C" };
	private int buttonIndex;
	private bool flashing = false;
	private int x;
	private int placesSum;
	private int codesAvg;
	private int y;
	private int z;
	private int tempModulo;
	private int[] primesUnderHundred = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
	private int[] primesUnderTen = new int[] { 2, 3, 5, 7 };
	private int leftToRight;


	private void Awake ()
	{
		//Debug.Log("Entered Awake method.");
		_moduleId = _moduleIdCounter++;
		foreach (KMSelectable button in buttons)
		{
			KMSelectable pushedButton = button;
			button.OnInteract += delegate () { PushButton(pushedButton); return false; };
		}
	}
	void Start ()
	{
		//Debug.Log("Entered Start method.");
		PickNumbers();
		FindX();
		FindY();
		FindZ();
		FindSolution();
        Debug.LogFormat("[Pie Flash #{0}] Displays are {1}, {2}, and {3}", _moduleId, intCodes[0], intCodes[1], intCodes[2]/*, intCodes[3], intCodes[4]*/);
        Debug.LogFormat("[Pie Flash #{0}] Positions in pi: {1}, {2}, and {3}", _moduleId, codePlaces[0], codePlaces[1], codePlaces[2]/*, codePlaces[3], codePlaces[4]*/);
        Debug.LogFormat("[Pie Flash #{0}] X is {1}, Y is {2}, Z is {3}", _moduleId, x, y, z);
        Debug.LogFormat("[Pie Flash #{0}] Correct button order: {1} {2} {3} {4} {5}", _moduleId, solution[0], solution[1], solution[2], solution[3], solution[4]);
		StartCoroutine(FlashNums());
	}

	void PushButton (KMSelectable button)
	{
		if (_moduleSolved || flashing) return;
		//Debug.LogFormat("Pushed button label {0}", button.GetComponentInChildren<TextMesh>().text);
		buttonIndex = Array.IndexOf(buttons, button);
		if (userAnswer.Contains(buttonIndex + 1)) return;
		Audio.PlaySoundAtTransform(sounds[stage - 1], Module.transform);
		buttonTexts[buttonIndex].color = Color.gray;
		for (int i = 0; i < 5; i++)
		{
			if (userAnswer[i] == -1)
			{
				userAnswer[i] = buttonIndex + 1;
				//Debug.LogFormat("Pushed button position {0}", buttonIndex + 1);
				break;
			}
		}
		stage++;
		if (stage > 5)
		{
			Debug.LogFormat("[Pie Flash #{0}] Entered {1} {2} {3} {4} {5}, expected {6} {7} {8} {9} {10}", _moduleId,
				userAnswer[0], userAnswer[1], userAnswer[2], userAnswer[3], userAnswer[4],
				solution[0], solution[1], solution[2], solution[3], solution[4]);
			if (userAnswer.SequenceEqual(solution))
			{
				_moduleSolved = true;
				StartCoroutine(SetSolvedDisplay());
				StartCoroutine(PlaySolveSound()); // Also handles pass.
			}
			else
			{
				flashing = true;
				stage = 1;
				for (int i = 0; i < 5; i++) 
				{
					userAnswer[i] = -1;
				}
                Debug.LogFormat("[Pie Flash #{0}] Strike!", _moduleId);
				//Debug.LogFormat("STRIKE!!!!");
				Module.HandleStrike();
				StartCoroutine(SetStrikeDisplay());
			}
		} 
	}

	private IEnumerator SetSolvedDisplay ()
	{
		yield return new WaitForSeconds(.5f);
		buttonTexts[0].text = "F";
		foreach (TextMesh buttonText in buttonTexts)
		{
			buttonText.color = Color.green;            
		}
		yield return new WaitForSeconds(.2f);
		buttonTexts[1].text = "L";
		foreach (TextMesh buttonText in buttonTexts)
		{
			buttonText.color = Color.gray;
		}
		yield return new WaitForSeconds(.2f);
		buttonTexts[2].text = "A";
		foreach (TextMesh buttonText in buttonTexts)
		{
			buttonText.color = Color.green;
		}
		yield return new WaitForSeconds(.2f);
		buttonTexts[3].text = "S";
		foreach (TextMesh buttonText in buttonTexts)
		{
			buttonText.color = Color.gray;
		}
		yield return new WaitForSeconds(.2f);
		buttonTexts[4].text = "H";
		foreach (TextMesh buttonText in buttonTexts)
		{
			buttonText.color = Color.green;
		}
	}
	private IEnumerator PlaySolveSound ()
	{
		yield return new WaitForSeconds(.5f);
        Audio.PlaySoundAtTransform("C", Module.transform);
        yield return new WaitForSeconds(.2f);
        Audio.PlaySoundAtTransform("E", Module.transform);
        yield return new WaitForSeconds(.2f);
        Audio.PlaySoundAtTransform("D", Module.transform);
        yield return new WaitForSeconds(.2f);
        Audio.PlaySoundAtTransform("F", Module.transform);
        yield return new WaitForSeconds(.2f);
        Audio.PlaySoundAtTransform("G", Module.transform);
		Module.HandlePass();
		//Debug.LogFormat("SWAG!!!!");
        Debug.LogFormat("[Pie Flash #{0}] Module passed!", _moduleId);
	}

	private IEnumerator SetStrikeDisplay ()
	{
		foreach (TextMesh buttonText in buttonTexts)
		{
			buttonText.color = Color.red;            
		}
		yield return new WaitForSeconds(1);
		foreach (TextMesh buttonText in buttonTexts)
		{
			buttonText.color = Color.green;            
		}
		flashing = false;
	}

	void PickNumbers()
    {
		for (int i = 0; i < displayAmt; i++)
		{
			int num = UnityEngine.Random.Range(0, 496);
			while (codePlaces.Contains(num + 1))
			{
				num = UnityEngine.Random.Range(0, 496);
			}
			codePlaces[i] = num + 1; // Keeps track of places.
			placesSum += num + 1;
			codes[i] = pi.Substring(num, 5);
			if (int.TryParse(codes[i], out intCodes[i]))
			{
				codesAvg += intCodes[i];
				//Debug.LogFormat("Value {0}: {1}", i, codes[i]);
				//Debug.LogFormat("Position {0}: {1}", i, codePlaces[i]);
			}
			else
			{
				Debug.LogFormat("Unable to successfully pick value {0}", i);
			}
		}
		codesAvg /= displayAmt;
		//Debug.LogFormat("placesSum: {0}", placesSum);
		//Debug.LogFormat("codesAvg: {0}", codesAvg);
    }

	private void FindX ()
	{
		x = (placesSum + codesAvg) % 100;
		//Debug.LogFormat("X: {0}", x);
	}

	private void FindY ()
	{
		y += (intCodes[0] % 1000) / 100;
		y += intCodes[1] / 10000;
		y += (intCodes[2] % 100) / 10;
		//y += intCodes[3] / 10000;
		//y += intCodes[4] % 10;	
		/*for (int i = 0; i < displayAmt; i++)	// Used to just add every digit lol
		{
			y += intCodes[i] / 10000;
			y += (intCodes[i] % 10000) / 1000;
			y += (intCodes[i] % 1000) / 100;
			y += (intCodes[i] % 100) / 10;
			y += intCodes[i] % 10;	
		}*/
		y %= 10;
		//Debug.LogFormat("Y: {0}", y);
	}

	private void FindZ ()
	{
		for (int i = 0; i < displayAmt; i++)
		{
			tempModulo = codePlaces[i] % 9;
			z += (tempModulo == 0 ? 9 : tempModulo);
		}
		//Debug.LogFormat("Z: {0}", z);
	}

	private void FindSolution()
	{
		solutionIndex = 0;
		solution = new int[] { -1, -1, -1, -1, -1 };

		// If X and Y are both prime or both composite
		if (primesUnderHundred.Contains(x) == primesUnderTen.Contains(y))
		{
			solution[solutionIndex] = 5;
			solutionIndex++;
		}

		// If X and Z are both even or both odd
		if ((x + z) % 2 == 0)
        {
			solution[solutionIndex] = 4;
			solutionIndex++;
        }

		// If Y and Z are multiples of three
		if (y % 3 == 0 && z % 3 == 0)
		{
			solution[solutionIndex] = 3;
			solutionIndex++;
		}

		// If Z > 10 and X is divisible by Y
		if (z > 10 && y != 0 && x % y == 0)
		{
			solution[solutionIndex] = 2;
			solutionIndex++;
		}

		// Fill the rest of the solution left to right with 1 to 5
		leftToRight = 1;
		while(solutionIndex < 5)
		{
			while (solution.Contains(leftToRight))
			{
				leftToRight++;
			}
			solution[solutionIndex] = leftToRight;
			leftToRight++;
			solutionIndex++;
		}

		/*Debug.Log("The correct button order is...");
		for (int i = 0; i < 5; i++)
		{
			Debug.LogFormat("Push the button in position {0}.", solution[i]);
		}*/
	}
	private IEnumerator FlashNums()
    {
        while (!_moduleSolved)
        {
            for (int i = 0; i < displayAmt; i++)
            {
				if (!_moduleSolved)
				{
					for (int j = 0; j < 5; j++)
					{
						buttonTexts[j].text = codes[i].Substring(j, 1);
					}
					yield return new WaitForSeconds(2.0f);
				}
            }
			if (!_moduleSolved)
			{
				for (int i = 0; i < 5; i++)
				{
					buttonTexts[i].text = "";
				}
				yield return new WaitForSeconds(3.0f);
			}
        }
    }


	// Twitch Plays support
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} 5 2 1 3 4 to press the buttons in a certain order.";
#pragma warning restore 414

    private static string[] supportedTwitchCommands = new[] { "press ", "click ", "submit " };

    private IEnumerator ProcessTwitchCommand(string command)
    {
		for (int i = 0; i < supportedTwitchCommands.Length; i++)
		{
			if (command.Contains(supportedTwitchCommands[i]))
			{
				command = command.Remove(command.IndexOf(supportedTwitchCommands[i]), supportedTwitchCommands[i].Length);
				break;
			}
		}
		var match = Regex.Match(command, @"^\s*([1-5])\s+([1-5])\s+([1-5])\s+([1-5])\s+([1-5])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (match.Success)
		{
			if (command.Contains('1') 
			 && command.Contains('2') 
			 && command.Contains('3') 
			 && command.Contains('4') 
			 && command.Contains('5'))	// Probably inefficient
			{
				yield return null;

				// Determine if input is solve or strike
				int [] intMatches = new int[] { -1, -1, -1, -1, -1 };
				for (int i = 0; i < 5; i++)
				{
					intMatches[i] = int.Parse(match.Groups[i + 1].Value);
				}
				if (intMatches.SequenceEqual(solution))
				{
					yield return "solve";
				}
				else
				{
					yield return "strike";
				}

				// Input the input
				for (int i = 0; i <= 3; i++)
				{
					buttons[intMatches[i] - 1].OnInteract();
					yield return new WaitForSeconds(.2f);
				}
				buttons[intMatches[4] - 1].OnInteract();
			}
			else
			{
				yield return "sendtochaterror Ypu must push every button exactly once in one command.";
			}
		}
    }

    private void TwitchHandleForcedSolve()
    {
		StartCoroutine(Solver());
    }

	private IEnumerator Solver()
    {
		stage = 1;
		for (int i = 0; i < 5; i++) 
		{
			userAnswer[i] = -1;
		}
        for (int i = 0; i <= 3; i++)
		{
			buttons[solution[i] - 1].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
		buttons[solution[4] - 1].OnInteract();
	}
}
