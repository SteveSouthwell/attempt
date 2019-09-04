/* File:        attempt.cs  -> attempt.exe
 * Author:      S.E. Southwell - BravePoint, Inc
 * Created:     8/2/2005
 * Purpose:     Command-line utility to put a hard time limit on calls to other command-line programs.
 * Usage:       attempt.exe 30 mycommand                     // Runs mycommand for a max of 30 seconds
 *              attempt.exe 10 somecommand param1 param2 ... // Runs somecommand for 10 seconds, passing in params
 *              attempt.exe --help                           // Provides help
 * Updates: 
 * 9/4/2019 - S.E. Southwell - Progress Software Corp. - Imported into VS 2019 and did some cleanup
 * 
 * */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;



namespace attempt
{
	/// <summary>
	/// Attempt to run the command passed in on 2nd parameter within number of seconds passed in on first parameter
	/// </summary>
	class attempt
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			int  waitTime  = 0;
			string myCMDPath = "";
			string cmdPath = "";
			SortedList argList = new SortedList();
			string myArgs = "";

			if (args.Length > 0) 
			{
				// Console.Out.WriteLine("Called with arguments: {0}", String.Join(", ", args));
				for(int i=0; i<args.Length ; i++ ) 
				{
					argList.Add(args[i],args[i]);
				}
				// See if the user is asking for help
				if (argList.Contains("/?") || argList.Contains("--help") || argList.Contains("-h") || argList.Contains("-?")) 
				{
					ShowHelp();
					return 1;
				} // Help


				// Less than 2 arguments is bad syntax
				else if (args.Length < 2) 
				{
					Console.Error.WriteLine("attempt: Bad Syntax");
					ShowHelp();
					return 1;
				}
				else 
				{
					// Get argument 0 into an integer
					try { waitTime = Convert.ToInt32(args[0].ToString());} 
					catch  { 
						Console.Error.WriteLine("attempt: First parameter must be a number.");
						ShowHelp();
						return 1;
					}

					// Get the command and find it in the PATH
					myCMDPath = args[1].ToString();
					// If the file is fully qualified, or in CWD, then use it as-is.
					string[] paths = Environment.GetEnvironmentVariable("PATH").Split(new Char[] {';'});
					foreach (string pathItem in paths) 
					{
						if (File.Exists(pathItem + "\\cmd.exe")) 
						{
							cmdPath = pathItem + "\\cmd.exe";
							break;
						} // File exists
					} // Each pathitem
				} // 2 or more arguments
			} 
			else  {
				ShowHelp();
				return 1;
			}

			if (args.Length > 2) 
			{
				for (int i = 2; i<args.Length; i++) 
				{
					myArgs = myArgs + " " + args[i];
				}
			}
			//Console.Out.WriteLine("Arglist is:" + myArgs);
			//Console.Out.WriteLine(cmdPath + " /C " + myCMDPath);
			Process myCommand = new Process();
			// Hook up plumbing
			myCommand.StartInfo.FileName = cmdPath;
			myCommand.StartInfo.CreateNoWindow = true;
			myCommand.StartInfo.Arguments = "/C " + myCMDPath + myArgs;
			myCommand.StartInfo.UseShellExecute = false;
			myCommand.StartInfo.RedirectStandardInput = true;
			myCommand.StartInfo.RedirectStandardOutput = true;
			myCommand.StartInfo.RedirectStandardError = true;

		
			// Run process
			try 
			{
				if (myCommand.Start()) 
				{
					if (waitTime == 0) myCommand.WaitForExit();
					else myCommand.WaitForExit(waitTime * 1000);
					if (! myCommand.HasExited) 
					{
						myCommand.Kill();
						Console.Error.WriteLine("attempt: Program timed out.  Partial output follows:");
						Console.Out.Write(myCommand.StandardOutput.ReadToEnd());
						Console.Error.Write(myCommand.StandardError.ReadToEnd());
						myCommand.Dispose();
						return 1;
					} 
					else 
					{
						Console.Out.Write(myCommand.StandardOutput.ReadToEnd());
						Console.Error.Write(myCommand.StandardError.ReadToEnd());
						int myExitCode = myCommand.ExitCode;
						myCommand.Dispose();
						return myExitCode;
					}
				}
				else 
				{
					Console.Error.WriteLine("Failed with code: " + myCommand.ExitCode.ToString());
					return 1;
				}
			} // try
			catch (Exception e) 
			{
				Console.Error.WriteLine("Command failed with: " + e.Message);
				return 1;
			} // catch

            // At this point, all is good.  Return happened above before catch.
		}
		static void ShowHelp() 
		{
			Console.Out.WriteLine("  Usage:  attempt SECONDS COMMAND [ARGUMENTS]...");			
			Console.Out.WriteLine("     or:  attempt --help  (shows this message)\n");
			Console.Out.WriteLine("Purpose:  Attempt to execute COMMAND within amount of time specified by");
			Console.Out.WriteLine("          SECONDS.  If seconds is 0, then the command will be allowed to");
			Console.Out.WriteLine("          run as long as it takes to return.  Otherwise, if the command");
			Console.Out.WriteLine("          hasn't completed within the time limit, it will be terminated");
            Console.Out.WriteLine("          gracefully if possible, or with extreme predjudice if needed.\n");
            Console.Out.WriteLine("          NOTE: Not suitable for interactive sessions.");
        }
    } // class attempt
} // namespace attempt
