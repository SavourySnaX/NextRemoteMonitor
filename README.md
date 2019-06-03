### PLEASE NOTE- THIS IS WORK IN PROGRESS, MONITOR FUNCTIONALITY DOES NOT YET EXIST

# NextRemoteMonitor
A remote monitor for working with the Spectrum Next via the ESP 8266 wifi module.

The code is fixed to connect to a specific ip and port in both the PCHost and the SpectrumHost, you will need to 
replace the IP's and ports if they are not suitable for your environment. Search for '192.168.5.2' and '9999'.

# Quick Guide (or how i`m currently developing this)

Assumes you have a spectrum next (currently using a 2A dev board) and an ESP Wifi module plugged in

This guide is how I got things working, it may not be the only way.
I assume your network has DHCP, if not you will need instructions for assigning a static IP to the ESP module.

## Configure ESP (one time setup) to connect to your home access point

We basically want to configure the ESP module in station mode (not AP)

From the browser on the next, load the Demos/UART/terminal.bas

Type ```run``` and press return/enter (do this after every command)

Type ```ATE1``` you should be greeted with ATE1 some blank lines and OK

If you don't see the above, seek help elsewhere (sorry)

Type ```AT+CWMODE=1``` this will set the module to work as a client (aka will connect to an access point and get an IP)

Type ```AT+CWLAP``` and you should see a list of Access Points - if you don't seek help elsewhere

Type ```AT+CWJAP="<SSID>","<password>"```  replace ```<SSID>``` and ```<password>``` as appropriate for your home network

If all goes well you should see WIFI CONNECTED and WIFI GOT IP. If not seek help elsewhere

At this point we are done with the configuration (the ESP module will remember this on subsequents boots)

## NextOS loader (one time setup) to auto load the assembled tape when i type run

Boot into NextBasic

Type ```cd "nextzxos"``` and press return

Type ```10 .tapein remote.tap``` and press return

Type ```20 LOAD "t:"``` and press return

Type ```30 LOAD ""``` and press return

Type ```SAVE "AUTOEXEC.BAS" LINE 10``` and press return

note you could also save as RUN.BAS and use run from spectrum basic but i`m lazy

## PCHost 

I'm using VS2017 Community edition.... the folder PCHost contains a VS solution which contains a crude terminal style
app that I`m using to develop the remote. Build and run the program, and you should see it waiting for a connection.

## Assembling and deploying a new remote.tap to the sdcard

I use a FlashAir at present (but once this monitor is developed far enough, the air won't be needed). My flash air (W-04) is setup
to auto connect to my wifi and mounts itself to my O:\ drive under windows. If you don't have a flash air, you can just assemble the
tape and copy it to the SD Card and then return it to the Next.

I'm currently using PasmoNext (but I don't think i`m using any ZX80N specific instructions at present). 

The SpectrumHost folder in the repository contains remote.asm and a bash script p  (which assembles and then copys the file to my flash
air. I then hit F4 on the next keyboard and the remote.tap launches the latest version. The script assumes PasmoNexts location, but
ultimately it boils down to ```PasmoNext -1 --equ USEIPPLACEHOLDER=0 --alocal --tapbas remote.asm remote.tap```.


!!Note, at present the PCHost application needs to be running, as I don't check for errors.!!

## Next steps

at this point the connection is running, follow the instructions in the PCHost application.

## Known Issues

Sometimes after a cold boot, the remote fails to connect, F4 reset, and it should connect the second/third time, after that it seems
fine.
