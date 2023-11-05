// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

#if UNITY_EDITOR
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Editor.Tools.IDE
{
	/// <summary>
	///     Will launch the IDE associated with .sln files when the Unity project is opened.
	/// </summary>
	[InitializeOnLoad] [ExcludeFromCodeCoverage]
	public class OpenSolutionOnStartup
	{
		private const String SessionStateKey = "CodeSmile.Editor.Tools.IsProjectLaunching";
		[NotNull] private static String ProjectRootPath => $"{Application.dataPath}/..";

		[MenuItem("CodeSmile/Open Solution")]
		public static void OpenSolutionFromMenu() => TryOpenSolution();

		private static void TryOpenSolution()
		{
			var solutionUrl = TryGetSolutionPath();
			if (File.Exists(solutionUrl))
			{
				Debug.Log($"Welcome {Environment.UserName}! Your IDE should now open the project's solution.\n" +
				          $"Solution path: {solutionUrl}");
				Application.OpenURL(solutionUrl);
			}
		}

		private static String TryGetSolutionPath()
		{
			var solutions = Directory.GetFiles(ProjectRootPath, "*.sln");
			return solutions.Length > 0 ? solutions[0] : null;
		}

		private static Boolean IsProjectLaunching()
		{
			var launching = SessionState.GetBool(SessionStateKey, true);
			if (launching)
				SessionState.SetBool(SessionStateKey, false);

			return launching;
		}

		private static void OnDelayCall()
		{
			EditorApplication.delayCall -= OnDelayCall; // not necessary but it's a good habit

			if (IsProjectLaunching())
				TryOpenSolution();
		}

		// delayed in case .sln file has not been generated yet
		static OpenSolutionOnStartup() => EditorApplication.delayCall += OnDelayCall;
	}
}
#endif
