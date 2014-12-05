using System;
using System.Collections.Generic;

namespace iTuneServiceManager
{
 
    class Label
    {
	    private static readonly Dictionary<String, String> _messages;
		public static Dictionary<String, String> Get
        {
            get { return _messages; }
        }

        static Label(){
			_messages = new Dictionary<string, string>
			{
				{"install", "Installs and starts the iTune Windows Service."},
				{"uninstall", "Uninstalls the iTune Windows Service (Not this manager app)"},
				{"start", "Start the iTune, Windows Service."},
				{"stop", "Stop the iTune, Windows Service."},
				{"computer", "The name of the computer on the local network."},
				{"user", "Choose the account that has access to the music you want to be able to play."},
				{"pass", "The password for the Windows account selected above."},
				{"rpass", "Retype the password to confirm it."},
				{"vpass", "The password has been provisionally confirmed."},
				{"select", "Select the iTunes executable file."},
				{"default", "ITunes Windows Service manager. By John Cleary."},
				{"open", "Run the iTunes Application (Only if service is stopped or not installed)"}
			};
        }
    }
}
