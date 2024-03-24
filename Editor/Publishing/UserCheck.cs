// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Security.Principal;
using UnityEditor;
using UnityEngine;

namespace CodeSmileEditor.Tools.Publishing
{
	public static class UserCheck
	{
		public static Boolean IsCurrentUserCodeSmile => WindowsIdentity.GetCurrent().Name.Equals("CodeSmile-PC\\CodeSmile");
	}
}
