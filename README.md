# Protocol-Adapter-AMM-Sensor2AMI-
Protocol Adapter (AMM2AMI)

About
-----
This package contains the FINESCE DSE (developed in the context of the FINESCE WP4) deputed to manage the Landis+Gyr E350 neters.
In order to operate, it requires the GURUX DLMS/COSEM libraries, which are lGPL v2. You should download them and install by your own.


License
-------
This software is released "As Is", without any warranty and without any support. By downloading and by using it, you agree to do it at your own risk.
Since now, we decline any responsibility.
Should you decide to use it, please note that any modification made should be released based on the same license lGPL v2. 
SHould you decide to benefit from the work of the Open Source Community, we expect you will continue to support it by your work.

Synopsis
--------
This FINESCE DSE is represented by two executble files, the IAMReader.exe and the MTServer.exe, and the IAM2IDAS dynamic library. 
They are designed to take part of the FINESCE distributed architecture.
This bundle contains the compiled EXE files that can run immediately, but they require some other FINESCED modules being already in execution prior to the launch.
These modules cannot run at their own because they take part of the distributed FINESCE (domain) application.
This bundle comunicates with the FINESCE instance of IDAS and ORION, which should be already running when you launch the binaries.

This bundle uses the GURUX DLMS/COSEM libraries, which are released upon the lGPL v2 license. You will need them only if you will need to recompile the project. 
Please download separately from https://github.com/gurux the source code and follow the GURUX's instructions to 
build the following assemblies :
- Gurux.Common.dll
- Gurux.DLMS.dll
- Gurux.Net.dll
- Gurux.Serial.dll

This bundle is released as open source lGPL v2, so it contains the source statements in order to be re-compiled and built.
Add the reference to the above mentioned Gurux assemblies into the FINESCE DSE project, specifying the folder where they 
are located.


Installation
------------
This software is hardware (meter) dependant. Please refer to the Landis+Gyr documentation about the E350 meters and ZXF modules first.
This softeare is FIENSCE application dependant. Pelase refer to the FIENSCE documentation first.
This software contains the DLMS/COSEM implementation. Please refer to the GURUX documentation first.
This sofwtatre comunicates with the IDAS and ORION FIWARE components. Please refer to the FIWARE documenation first.
This sofwtare uses SensorML to comunicate with the IDAS. Please refer to the OpenGIS/IoT documentation first.

When all external dependencies are solved, you might begin the installation.
To install it in your system, do create the folders IAMReader, IAMServer, IAM2IDAS, GURUX and expand/copy the software contained in the compressed archive.
Please do ensure that the FINESCE services are running. Please configure real IP addresses of them in the configuration files. 
If (when) all the external components are available and running, you may try to run the "MTServer" first, and the "IAMReader" then.


Recompiling
-----------
This bundle comes as "open source". It was engineered by using the Microsoft Visual Studio version 2010. You may reopen it in the compatible "Visual Studio" version.
Please create your own new project and feel free to use the sources to derive your new software (dont forget that your modified software will remain subject of the lGPL v2 license).

Running
-------
1. Do run MTServer first.
2. Do run the IAMReader then; please read first about the arguments by launching "IAMReader.exe /h"

IAMReader arguments
-------------------
Type IAMReader.exe /h to display the program usage arguments:

 /a=     Authentication (None (default), Low, High).
 /host=  host name.
 /l      Loop time (-1: single run (default), 0: continuous, 1: 1 minute, 2: 2 minutes, etc...)
 /m=     manufacturer identifier (see GURUX documentation at https://github.com/gurux)
 /p=     port number or name (default: 4059).
 /pw=    Password for authentication.
 /s=     start protocol (IEC or DLMS).
 /sm=    SitesMap file (site parameters in the sitemap file override the command line arguments)
 /sp=    serial port.
 /is=    IAM server.
 /t      Trace messages.
Example:
 Read LG device using TCP/IP connection:  IAMReader /m=lgz /host=192.168.200.20 /p=4059
 Read LG device using serial port connection:  IAMReader /m=lgz /sp=COM1 /s=DLMS

Refer to GURUX documentation for details about the meaning and values of each DLMS specific parameters.


Configuring the SiteMap file
----------------------------
The IAMReader program gets from a xml-formatted configuration file (default: Sitesmap.xml) all the information needed to communicate
with DLMS meters installed into the field.
An example of this file is included in the compressed archive.
The file "Sitesmap.xml" stays in the same directory of teh IAMReader.exe.

For each meter to connect, a specific <DLMSSite> section into the sitemap file needs to be configured.

The structure of <DLMSSite> section looks like the following (note that the <DataProcessing> section is managed directly by the 
program, so do not edit it):
  <DLMSSite>
    <Name>Put here the meter site name (mandatory)</Name>
    <Description> Put here a description of site (may be empty)</Description>
    <Enabled>true to enable the data reading, false to disable</Enabled>
    <Device Type=Put here the model of meter (e.g. "ZXF") Protocol=Put here the supported protocol (e.g."DLMS") Manufacturer=See               GURUX documentation for manufacturer codes (e.g."LGZ")>
      <Host>Put here the IP address of the meter (ex. 192.168.200.2)</Host>
      <Port>4059</Port>
      <Authentication>
        <type>Put here the authentication type (None, Low, High)</type>
        <password />
      </Authentication>
    </Device>
    <ArrayOfOBIS>
      <DataObjects>
        <ActivePower>1.0.16.7.0.255</ActivePower>
        <ActiveEnergyP>1.0.1.8.0.255</ActiveEnergyP>
        <ActiveEnergyM>1.0.2.8.0.255</ActiveEnergyM>
        <ReactiveEnergyP>1.0.3.8.0.255</ReactiveEnergyP>
        <ReactiveEnergyM>1.0.4.8.0.255</ReactiveEnergyM>
      </DataObjects>
      <GenericProfileObjects>
        <LoadProfile>1.0.99.1.0.255</LoadProfile>
      </GenericProfileObjects>
    </ArrayOfOBIS>
  </DLMSSite>



Stopping
--------
1. Do terminate the IAMReader first
2. Do terminate the MTServer then.


Aborting
--------
If one of the modules terminates abnormally, you have to terminate the remaining instances.
If the external FINESCE services terminate abnormally, it could be useful to terminate all the running instances and to launch newly the clean instances then.


Assistance & support
--------------------
This software is released "as is" without any kind of support/warranty. You accepted to use it at your own risk.
This software comes without any update, without any bug fixing, and without any maintenance.


Special thanks
--------------
This work was performed in the context of the FINESCE (Future INtErnet Smart Utility ServiCEs) smart energy use case project. 
FINESCE develops an open IT-infrastructure to be used in the Future Internet solutions related to the energy sector. 
Our work was to materialize the interoperability between the AMR and the AMI by using the FI-WARE APIs and to test it at the premises of real users in Italy. 
We used cloud-based event-driven service architecture FIWARE. 
This work received funding from the European Union's 7th Framework Program under the FI.ICT-2011 Grant number 604677.
We thank the European Union for their support.


This bundle was materialised thanks to the open source availability of the GURUX DLMS/COSEM libraries. 
We thank the Open Source Community and the GURUX Developer Network for their contributions.
Please refer to their web site http://www.gurux.fi/index.php?q=gurux.dlms for further examples and source code.

Finally, we thank in advance anyone who will continue our work by downloading, modifying, re-engineering, and making available the advanced verssions of this software to the general public as open source lGPL v2.

