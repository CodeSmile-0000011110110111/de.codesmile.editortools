// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CodeSmileEditor.Tools.Publishing
{
	/// <summary>
	///     Copies CodeSmile packages into Assets and back out. Switches between publishing the asset vs development.
	/// </summary>
	/// <remarks>
	///     Asset Store Publishing Tools unfortunately are unable to automatically include references packages,
	///     not even if they are embedded.
	/// </remarks>
	internal class CodeSmileAssetPublishUtility : Editor
	{
		private const String PackageNameFilter = "de.codesmile.";
		private const String FileIdentifier = "\"file:";

		private static readonly String BackupExtension = ".backup";
		private static readonly String ManifestPath = Application.dataPath + "/../Packages/manifest.json";
		private static readonly String ManifestBackupPath =
			Application.dataPath + "/../Packages/manifest.json" + BackupExtension;
		private static readonly String AssetPackagesPath = Application.dataPath + "/CodeSmile/Packages/";
		private static readonly String PackagesSavePath =
			Path.GetFullPath(Application.persistentDataPath + "CodeSmilePackagePaths.txt");

		private static readonly ImportAssetOptions ImportOptions =
			ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport;

		private static readonly String ContinueMoveToAssetsKey = "Continue_MovePackagesToAssets";

		[MenuItem("Prepare for Publishing", menuItem = "Window/CodeSmile/DEV: Move my Packages to Assets", priority = 9998)]
		private static void MoveToAssets()
		{
			if (UserCheck.IsCurrentUserCodeSmile)
			{
				BackupManifest();
				var packagePaths = RemoveCodeSmilePackagesFromManifest();
				SavePackagePaths(packagePaths);
				SessionState.SetBool(ContinueMoveToAssetsKey, true);

				Client.Resolve();
				EditorUtility.RequestScriptReload();
			}
			else
				Debug.Log("This menu item only works for CodeSmile.");
		}

		[MenuItem("Unprepare for Publishing", menuItem = "Window/CodeSmile/DEV: Remove my Packages from Assets",
			priority = 9999)]
		private static void RemoveFromAssets()
		{
			if (UserCheck.IsCurrentUserCodeSmile)
			{
				DeletePackagesInAssets();
				ReplaceManifestWithBackup();
				AssetDatabase.ImportAsset(ToRelativePath(AssetPackagesPath), ImportOptions);

				Client.Resolve();
				EditorUtility.RequestScriptReload();
			}
			else
				Debug.Log("This menu item only works for CodeSmile.");
		}

		[InitializeOnLoadMethod]
		private static void TryContinueMoveToAssets()
		{
			var continueMove = SessionState.GetBool(ContinueMoveToAssetsKey, false);
			if (continueMove)
			{
				SessionState.EraseBool(ContinueMoveToAssetsKey);
				Debug.Log("Continue move to assets ...");

				EditorApplication.delayCall += () =>
				{
					CopyPackagesToAssets(LoadPackagePaths());
					AssetDatabase.ImportAsset(ToRelativePath(AssetPackagesPath), ImportOptions);
				};
			}
		}

		private static String[] RemoveCodeSmilePackagesFromManifest()
		{
			var manifest = File.ReadAllLines(ManifestPath).ToList();
			var packagePaths = new List<String>();

			for (var i = manifest.Count - 1; i >= 0; i--)
			{
				var line = manifest[i];
				if (line.Contains(PackageNameFilter))
				{
					var pathStart = line.LastIndexOf(FileIdentifier) + FileIdentifier.Length;
					var pathEnd = line.IndexOf("\"", pathStart) - pathStart;
					var path = line.Substring(pathStart, pathEnd);

					packagePaths.Add(path);
					manifest.RemoveAt(i);
				}
			}

			File.WriteAllLines(ManifestPath, manifest);

			return packagePaths.ToArray();
		}

		private static void BackupManifest()
		{
			if (File.Exists(ManifestBackupPath))
				throw new InvalidOperationException("backup manifest already exists");

			FileUtil.CopyFileOrDirectory(ManifestPath, ManifestBackupPath);
		}

		private static void ReplaceManifestWithBackup()
		{
			if (File.Exists(ManifestBackupPath) == false)
				throw new InvalidOperationException("backup manifest does not exist");

			File.Delete(ManifestPath);
			FileUtil.CopyFileOrDirectory(ManifestBackupPath, ManifestPath);
			File.Delete(ManifestBackupPath);
		}

		private static void CopyPackagesToAssets(String[] packagePaths)
		{
			foreach (var path in packagePaths)
			{
				var destPath = AssetPackagesPath + path.Substring("P:/".Length);
				Debug.Log($"Copying '{path}' to '{destPath}'");

				if (Directory.Exists(destPath))
					throw new InvalidOperationException($"Target dir already exists, aborting: {destPath}");

				Directory.CreateDirectory(destPath);

				// files
				CopyFileOrDirWithMeta(path, destPath, "package.json");
				CopyFileOrDirWithMeta(path, destPath, "CHANGELOG.md");
				CopyFileOrDirWithMeta(path, destPath, "GETTING STARTED.md");
				CopyFileOrDirWithMeta(path, destPath, "README.md");
				CopyFileOrDirWithMeta(path, destPath, "TODO.md");

				// directories
				CopyFileOrDirWithMeta(path, destPath, "Editor");
				CopyFileOrDirWithMeta(path, destPath, "Runtime");
			}
		}

		private static void CopyFileOrDirWithMeta(String path, String destPath, String fileName)
		{
			var source = $"{path}/{fileName}";
			if (File.Exists(source) || Directory.Exists(source))
			{
				var dest = $"{destPath}/{fileName}";
				FileUtil.CopyFileOrDirectory(source, dest);
				FileUtil.CopyFileOrDirectory($"{source}.meta", $"{dest}.meta");
			}
		}

		private static void DeletePackagesInAssets()
		{
			var fullPath = Path.GetFullPath(AssetPackagesPath);
			Directory.Delete(Path.GetFullPath(AssetPackagesPath), true);
			Directory.CreateDirectory(fullPath);

			// remove the .meta since Unity still recognizes that the folder had been deleted
			File.Delete(Path.GetFullPath(AssetPackagesPath.TrimEnd('/') + ".meta"));
		}

		private static String ToRelativePath(String absolutePath)
		{
			absolutePath = absolutePath.Replace("\\", "/");
			if (absolutePath.StartsWith(Application.dataPath))
				return $"Assets{absolutePath.Substring(Application.dataPath.Length)}";

			throw new ArgumentException("failed to make path relative: " + absolutePath);
		}

		private static String[] LoadPackagePaths()
		{
			if (File.Exists(PackagesSavePath))
				return File.ReadAllLines(PackagesSavePath);

			throw new FileNotFoundException(PackagesSavePath);
		}

		private static void SavePackagePaths(String[] packagePaths)
		{
			if (File.Exists(PackagesSavePath))
				File.Delete(PackagesSavePath);

			File.WriteAllLines(PackagesSavePath, packagePaths);
		}
	}
}
