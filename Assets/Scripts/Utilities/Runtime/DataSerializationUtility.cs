#region Namespaces

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

#endregion

namespace Utilities
{
	public class DataSerializationUtility<T> where T : class
	{
		#region Variables

		private readonly string path;
		private readonly bool useResources;
		private readonly bool bypassExceptions;

		#endregion

		#region Methods

		#region Utilities

		public bool SaveOrCreate(T data)
		{
			CheckValidity();

			FileStream stream = null;

			try
			{
				string newPath = useResources ? Path.Combine("Assets", "Resources", path) : path;

				if (!Directory.Exists(Path.GetDirectoryName(newPath)))
					Directory.CreateDirectory(Path.GetDirectoryName(newPath));

				stream = File.Open($"{newPath}{(useResources ? ".bytes" : "")}", FileMode.OpenOrCreate);

				BinaryFormatter formatter = new BinaryFormatter();

				formatter.Serialize(stream, data);

				return true;
			}
			catch (Exception e)
			{
				if (!bypassExceptions)
					throw e;
				else
					return false;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
		}
		public T Load()
		{
			CheckValidity();

			if (useResources && !Resources.Load<TextAsset>(path) || !useResources && !File.Exists(path))
			{
				if (bypassExceptions)
					return null;
				else
					throw new ArgumentException($"The file ({path}) doesn't exist");
			}

			Stream stream = null;

			try
			{
				if (useResources)
					stream = new MemoryStream(Resources.Load<TextAsset>(path).bytes);
				else
					stream = File.Open(path, FileMode.OpenOrCreate);

				BinaryFormatter formatter = new BinaryFormatter();
				T data = formatter.Deserialize(stream) as T;

				return data;
			}
			catch (Exception e)
			{
				if (bypassExceptions)
					return null;
				else
					throw e;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
		}
		public bool Delete()
		{
			CheckValidity();

			if (useResources)
			{
				Debug.LogError("You can't delete a resource file, use normal path finding instead!");

				return false;
			}

			if (!File.Exists(path))
			{
				if (!bypassExceptions)
					throw new FileNotFoundException($"We couldn't delete ({path}), as it doesn't exist!");

				return false;
			}

			try
			{
				File.Delete(path);

				string metaFilePath = $"{path}.meta";

				if (File.Exists(metaFilePath))
					File.Delete(metaFilePath);
			}
			catch (Exception e)
			{
				if (!bypassExceptions)
					throw e;

				return false;
			}

			return true;
		}

		private void CheckValidity()
		{
			if (useResources)
				return;

			if (!Directory.Exists(Path.GetDirectoryName(path)))
				Directory.CreateDirectory(Path.GetDirectoryName(path));

			FileAttributes fileAttributes = File.GetAttributes(Path.GetDirectoryName(path));

			if (!fileAttributes.HasFlag(FileAttributes.Directory))
				throw new DirectoryNotFoundException($"The `path` argument of value \"{Path.GetDirectoryName(path)}\" must be a valid directory");
		}

		#endregion

		#region Constructors & Operators

		#region Constructors

		public DataSerializationUtility(string path, bool useResources, bool bypassExceptions = false)
		{
			this.path = path;
			this.useResources = useResources;
			this.bypassExceptions = bypassExceptions;

			CheckValidity();
		}

		#endregion

		#region Operators

		public static implicit operator bool(DataSerializationUtility<T> serializationUtility) => serializationUtility != null;

		#endregion

		#endregion

		#endregion
	}
}
