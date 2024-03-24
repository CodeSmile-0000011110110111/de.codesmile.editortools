// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Diagnostics.CodeAnalysis;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace CodeSmileEditor.Tools.IDE
{
	/// <summary>
	///     Will launch the IDE associated with .sln files when the Unity project is opened.
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal static class OpenSolutionOnStartup
	{
		private const String SessionStateKey = "CodeSmileEditor.Tools.IsProjectLaunching";

		[InitializeOnLoadMethod] private static void InitializeOnLoad()
		{
			// disabled auto-load because quitting Unity while leaving Rider open stacks the open request
			// thus when closing Rider, it re-opens as many times as I had quit Unity ... ugh.
			// no time atm to implement a workaround (check process executable already running)

			// if (IsProjectLaunching())
			// 	TryOpenSolution();
		}

		[MenuItem("Window/CodeSmile/Open Solution", priority = 2999)]
		private static void TryOpenSolution() => CodeEditor.CurrentEditor.OpenProject();

		private static Boolean IsProjectLaunching()
		{
			var launching = SessionState.GetBool(SessionStateKey, true);
			if (launching)
				SessionState.SetBool(SessionStateKey, false);

			return launching;
		}
	}
}
