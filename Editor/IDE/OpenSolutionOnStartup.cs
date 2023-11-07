// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

#if UNITY_EDITOR
using System;
using System.Diagnostics.CodeAnalysis;
using Unity.CodeEditor;
using UnityEditor;

namespace CodeSmile.Editor.Tools.IDE
{
	/// <summary>
	///     Will launch the IDE associated with .sln files when the Unity project is opened.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class OpenSolutionOnStartup
	{
		private const String SessionStateKey = "CodeSmile.Editor.Tools.IsProjectLaunching";

		[InitializeOnLoadMethod]
		private static void OnLoad()
		{
			if (IsProjectLaunching())
				TryOpenSolution();
		}

		[MenuItem("CodeSmile/Open Solution")]
		private static void TryOpenSolution() => CodeEditor.Editor.CurrentCodeEditor.OpenProject();

		private static Boolean IsProjectLaunching()
		{
			var launching = SessionState.GetBool(SessionStateKey, true);
			if (launching)
				SessionState.SetBool(SessionStateKey, false);

			return launching;
		}
	}
}
#endif