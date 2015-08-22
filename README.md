iTuneService
============

Create an iTunes server by running it as a Windows Service

iTuneService makes installing and launching iTunes as a Windows Service an easy process. Run under any user credentials and take advantage of all the usual Windows Service controls.

Running iTunes as a service means that iTunes will start before anyone even logs onto the computer. You can use the [Apple Remote App](https://itunes.apple.com/us/app/remote/id284417350) to play (including AirPlay) your music without logging in. Or you can easily share your library using [Home Sharing](http://support.apple.com/en-us/HT4620) on a headless server.
 
iTuneService was formerly known as JTunes and was originally created by John Cleary ([www.johncleary.net](http://www.johncleary.net)).

Also contributing to the project is Nathan Jones ([www.nathanpjones.com](http://www.nathanpjones.com)).

#### What Can I Use This For?

**Expose a Personal Library** You have a large iTunes library on your always-on desktop computer that you want to make available to the rest of your devices without always having to remember to run iTunes.

Use iTunesService to install the background service. When you want to use iTunes on the desktop, you click the system tray icon to bring up the service manager, stop the service, and then run iTunes as normal. When you're done, close iTunes and use the same process to start the service again.

**Run on a Dedicated Server** You have an always-on server computer where you created a separate [standard](http://www.tomsguide.com/us/create-standard-user-account,news-18333.html) user account that you want to use to serve media.

You log in as an administrator and install the service under that alternate username. When you want to load new media, download a new purchase, or import from other libraries on the network you may use the service manager's "Run iTunes Interactively" feature. If you delete any files as the service user, you can empty that user's recycle bin to free up disk space.

**Always-On Music Player** In either of the above scenarios, you can connect to your server using the [Remote App](https://itunes.apple.com/us/app/remote/id284417350) and play music either through the server's speakers or AirPlay it to a compatible device.

#### Features

- Run iTunes as a service under any username.
- Easily install/uninstall or start/stop the service.
- With a single click, run iTunes in interactive mode as the service user to perform library maintenance / import.
- Provide ability to empty recycle bin of the service user for when you delete media in interactive mode.

#### Installer

Visit the releases page to [download the installer](https://github.com/clearz/iTuneService/releases/latest).

**This will require local administrator privileges to install / run.**

#### Reporting an Issue / Requesting a Feature

To report a problem or request a feature, head on over to the [Issues](https://github.com/clearz/iTuneService/issues) section and see if it's already listed. If not, go ahead and open a new issue.

If you're reporting an issue, please make sure to attach the logs found in: `%allusersprofile%\iTuneService`. For convenience, you may want to provide the files via a [gist](https://gist.github.com).
