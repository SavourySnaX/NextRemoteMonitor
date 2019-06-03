
Spectrum Next Remote Monitor/Debugger
-------------------------------------

Version 0.0.0.1


Instructions:
-------------

Make sure you configure the ESP to access you home network as a client - See below for how i do it.

The monitor present here only consists of a simple memory viewer/editor, there will be more to come in the future.

Assuming you have your next connecting to your home network, the next step is to run the Monitor.exe provided in the zip.

On first run (or to reconfigure the network, or update the application used by the next) :

By default the Monitor.exe hosts a tcp socket on port 9999 (configurable in the settings menu). You may need to confirm a firewall pop up if you are running a firewall on your host PC.

Click `Generate New Monitor Tape File` on the top menu bar.

This allows you to configure the IP address the spectrum next will connect to, and also to save out a tape file you can then load into the Spectrum Next. Click `Get Local IP` and it should automatically fill in the IP4 address of your host PC (if not, enter it manually). Save Tape will then prompt for a location to save the .TAP , insert your SD card into the PC and point the browser at it and confirm. 

Now put the card back into the Spectrum Next and load the file you saved via the Browser. Load it in 'S' standard mode if prompted.

At this point if all goes well, the Monitors Window title will show `Spectrum Next Remote Debugger - Connected `, if not, check your ESP settings/monitor setttings/firewall, also try restarting the Spectrum Next and reloading the monitor.

Features:
---------

Send binary data to the next (keep it to <= 8192 bytes, e.g. you could load a scr$ into bank
10 to put a picture on the screen.)
Open a MemoryView window (see Window menu) - the view will show 8192 bytes from the selected bank, you can also edit the
memory live in this view (view bank 10 and try changing bytes).
All supported windows can be added multiple times.
All window settings (and dialog) are saved for convenience.
The application supports docking of windows.

Future:
-------

Disassembler View
Registers View
Breakpoints
Stepping (into/over/etc)
Support for sending/recieving larger blocks of data.
Next Register View



Crude Guide To Configuring the ESP module to connect to your homes network:
---------------------------------------------------------------------------

We basically want to configure the ESP module in station mode (not as an AP),
Note if you have used the excellent NXtel on your Spectrum Next then it is likely you are already configured.

From the browser on the Spectrum Next, load the Demos/UART/terminal.bas

Type 'run' without quotes and press return/enter (do this for every command in quotes below)

Type 'ATE1' you should be greeted with ATE1 some blank lines and OK

If you don't see the above, seek help elsewhere (sorry)

Type 'AT+CWMODE=1' this will set the module to work as a client (aka will connect to an access point and get an IP)

Type 'AT+CWLAP' and you should see a list of Access Points - if you don't seek help elsewhere

Type 'AT+CWJAP="<SSID>","<password>"'  replace <SSID> and <password> as appropriate for your home network

If all goes well you should see WIFI CONNECTED and WIFI GOT IP. If not seek help elsewhere

At this point we are done with the configuration (the ESP module will remember this on subsequents boots)
