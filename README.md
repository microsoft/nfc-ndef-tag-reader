NFC NDEF Tag Reader
===================

A simple NFC tag reader application for Windows Phone 8. This example app
demonstrates how to use the Proximity API to read NFC Data Exchange Format
(NDEF) compliant tags.

![Screenshot](/doc/nfcndeftagreader-2.png?raw=true)

This example application is hosted in GitHub:
https://github.com/Microsoft/nfc-ndef-tag-reader

For more information on implementation and porting, visit Lumia
Developer's Library:
http://developer.nokia.com/Resources/Library/Lumia/#!code-examples/nfc-ndef-tag-reader.html

The project is compatible with Windows Phone 8. Developed with Microsoft Visual
Studio Express 2012 for Windows Phone.


1. Building, installing, and running the application
-------------------------------------------------------------------------------

You need to have Windows 8 and Windows Phone SDK 8.0 or later installed.

Using the Windows Phone 8 SDK:

1. Open the SLN file: File > Open Project, select the file
   `NfcNdefTagReader.sln`
2. Select the target 'Device'.
3. Press F5 to build the project and run it on the device.

Please see the official documentation for
deploying and testing applications on Windows Phone devices:
http://msdn.microsoft.com/en-us/library/gg588378%28v=vs.92%29.aspx


2. About implementation
-------------------------------------------------------------------------------

Important files and classes:

* `MainPage.cs`: Ties the UI and the application logic together. Contains the
  method for parsing the NDEF data. Shows the log containing the details of each
  tag read.
* `NdefRecordUtility.cs`: Contains utility methods for handling NDEF tag data.
* `NdefRecord.cs`: Container class for NDEF record data.
* `NdefRecordTypeDefinitions.cs`: Contains the implementation for two Record
  Type Definitions (RTD) classes: `NdefTextRtd` and `NdefUriRtd`. Like the names
  of the classes indicate, the first is for text and the second for URI types.


Required capabilities:

* `ID_CAP_NETWORKING`
* `ID_CAP_PROXIMITY`


3. License
-------------------------------------------------------------------------------

See the license text file delivered with this project. The license file is also
available online at
https://github.com/Microsoft/nfc-ndef-tag-reader/blob/master/Licence.txt


4. Version history
-------------------------------------------------------------------------------

* Version 1.0: The initial release.
