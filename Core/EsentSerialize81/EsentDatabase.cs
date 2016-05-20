﻿using EsentSerialization.Utils.FileIO;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EsentSerialization
{
	/// <summary>This static utility class implements the most high-level entry-point of the library for a windows store app.</summary>
	public static class EsentDatabase
	{
		/// <summary>Class holding various user-adjustable parameters of the database.</summary>
		public class Settings
		{
			/// <summary>Database folder name, default is "db", located in ApplicationData.Current.LocalFolder</summary>
			public string folderName = "db";

			/// <summary>Max concurrent sessions, default is 2.</summary>
			/// <remarks>Sessions are expensive: each session holds a cursor copy for each table.
			/// Two of them should be enough for most WSA applications: one session for the GUI thread, another one for some background uploading/downloading/processing.</remarks>
			public int maxConcurrentSessions = 2;

			/// <summary>Database location. The default is null, which translates to SpecialFolder.LocalApplicationData on desktop, or ApplicationData.LocalFolder on WinRT.</summary>
			public string folderLocation = null;

			/// <summary>Absolute path of the database folder.</summary>
			public string databasePath
			{
				get
				{
					if( String.IsNullOrWhiteSpace( folderName ) )
						throw new ArgumentNullException( "folderName must not be empty" );  //< 'coz in Database.drop we wipe the whole content of the folder.
					string appData = folderLocation;
					if( String.IsNullOrWhiteSpace( appData ) )
					{
#if NETFX_CORE
						appData = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
						appData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
#endif
					}
					return Path.Combine( appData, folderName );
				}
			}
		}

		static string databasePath;

		/// <summary>Open the database.</summary>
		public static SessionPool open( Settings settings, Type[] rowTypes )
		{
			string path = settings.databasePath;
			databasePath = path;
			return new SessionPool( path, settings.maxConcurrentSessions, rowTypes );
		}

		/// <summary>Open the database.</summary>
		public static SessionPool open( params Type[] rowTypes )
		{
			return open( new Settings(), rowTypes );
		}

		/// <summary>Open the database.</summary>
		public static SessionPool open( Settings settings, Assembly ass )
		{
			string path = settings.databasePath;
			databasePath = path;
			return new SessionPool( path, settings.maxConcurrentSessions, ass );
		}

		/// <summary>Open the database.</summary>
		public static SessionPool open( Assembly ass )
		{
			return open( new Settings(), ass );
		}

		/// <summary>Erase the complete database. The database must be closed before this call.</summary>
		public static void drop()
		{
			string[] files = FindFiles.EnumerateAll( databasePath ).ToArray();
			foreach( string f in files )
				File.Delete( f );
		}
	}
}