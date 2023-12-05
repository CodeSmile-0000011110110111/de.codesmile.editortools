// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

#if UNITY_EDITOR
using System;
using System.Diagnostics.CodeAnalysis;
using Unity.CodeEditor;
using UnityEditor;

namespace CodeSmileEditor
{
	/// <summary>
	///     Will launch the IDE associated with .sln files when the Unity project is opened.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class OpenSolutionOnStartup
	{
		private const String SessionStateKey = "CodeSmileEditor.Tools.IsProjectLaunching";

		[InitializeOnLoadMethod]
		private static void OnLoad()
		{
			if (IsProjectLaunching())
				TryOpenSolution();
		}

		[MenuItem("Window/CodeSmile/Open Solution", priority = 2999)]
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
