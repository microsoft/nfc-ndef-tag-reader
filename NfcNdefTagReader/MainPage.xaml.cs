/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Controls;
using Windows.Networking.Proximity;
using Windows.Storage.Streams;

namespace NfcNdefTagReader
{
    /// <summary>
    /// The application logic implementation of the main page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // The list of records 
        private List<NdefRecord> recordList;
        
        // The subscription ID from ProximityDevice
        private long subscriptionId;

        // ProximityDevice instance
        private ProximityDevice device;

        // The log content which will be filled with the details of each
        // detected NFC tag
        private string logText;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            recordList = new List<NdefRecord>();
            device = ProximityDevice.GetDefault();
            device.DeviceArrived += DeviceArrived;
            device.DeviceDeparted += DeviceDeparted;
            SubscribeForMessage();
        }

        /// <summary>
        /// Gets called when a detected NFC device is disconnected after arrival.
        /// </summary>
        /// <param name="sender">ProximityDevice instance</param>
        private void DeviceDeparted(ProximityDevice sender)
        {
            Dispatcher.BeginInvoke(() =>
            {
                logText = logText + "\nLost at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "\n";
                AppText.Text = logText + AppText.Text; 
            });
        }

        /// <summary>
        /// Gets called when a NFC device is detected.
        /// </summary>
        /// <param name="sender">ProximityDevice instance</param>
        private void DeviceArrived(ProximityDevice sender)
        {
            Dispatcher.BeginInvoke(() => 
            {
                logText = "\nDetected at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            });
        }

        /// <summary>
        /// Gets called when a message is received. Updates the UI by adding
        /// the details into the log and scrolling the log if necessary.
        /// Note that subscription for messages needs to be redone.
        /// </summary>
        /// <param name="sender">ProximityDevice instance</param>
        /// <param name="message">A message to be handled.</param>
        private void MessageReceived(ProximityDevice sender, ProximityMessage message)
        {
            Dispatcher.BeginInvoke(() => 
            { 
                if (device != null)
                {
                    device.StopSubscribingForMessage(subscriptionId);
                }

                logText = logText + ParseNDEF(message);
                ScrollViewer.UpdateLayout();
                ScrollViewer.ScrollToVerticalOffset(0); 
                SubscribeForMessage();
            });            
        }

        /// <summary>
        /// Parses the details from the given message. The output is a string
        /// that can be appended into the log.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>The parsed details as a string.</returns>
        private string ParseNDEF(ProximityMessage message) {
            var output = "";

            using (var buf = DataReader.FromBuffer(message.Data)) {
                NdefRecordUtility.ReadNdefRecord(buf,recordList);

                for (int i = 0, recordNumber = 0, spRecordNumber = 0; i < recordList.Count; i++)
                {
                    NdefRecord record = recordList.ElementAt(i);

                    if (!record.IsSpRecord ) 
                    {
                        if (System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength) == "Sp") 
                        {
                            output = output + "\n --End of Record No." + recordNumber; spRecordNumber = 0;
                            continue;
                        }
                        else 
                        { 
                            recordNumber++; 
                            output = output + "\n --Record No." + recordNumber; 
                        }
                    }
                    else
                    {
                        if (spRecordNumber == 0) 
                        { 
                            recordNumber++; 
                            output = output + "\n --Record No." + recordNumber; 
                        }

                        spRecordNumber++;
                        output = output + "\n Sp sub-record No." + spRecordNumber; 
                    }

                    output = output + "\n MB:" + ((record.Mb) ? "1;" : "0;");
                    output = output + " ME:" + ((record.Me) ? "1;" : "0;");
                    output = output + " CF:" + ((record.Cf) ? "1;" : "0;");
                    output = output + " SR:" + ((record.Sr) ? "1;" : "0;");
                    output = output + " IL:" + ((record.Il) ? "1;" : "0;");

                    string typeName = NdefRecordUtility.GetTypeNameFormat(record);                     
                    
                    if (record.TypeLength > 0) 
                    { 
                        output = output + "\n Type: " + typeName + ":"
                            + System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength); 
                    }

                    if ((record.Il) && (record.IdLength > 0))
                    {
                        output = output + "\n Id:"
                            + System.Text.Encoding.UTF8.GetString(record.Id, 0, record.IdLength); 
                    }

                    if ((record.PayloadLength > 0) && (record.Payload != null))
                    {
                        if ((record.Tnf == 0x01)
                            && (System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength) == "U"))
                        {
                            NdefUriRtd uri = new NdefUriRtd();
                            NdefRecordUtility.ReadUriRtd(record, uri);
                            output = output + "\n Uri: " + uri.GetFullUri();
                        }
                        else if ((record.Tnf == 0x01)
                            && (System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength) == "T"))
                        {
                            NdefTextRtd text = new NdefTextRtd();
                            NdefRecordUtility.ReadTextRtd(record, text);
                            output = output + "\n Language: " + text.Language;
                            output = output + "\n Encoding: " + text.Encoding;
                            output = output + "\n Text: " + text.Text;
                        }
                        else
                        {
                            if (record.Tnf==0x01) 
                            {
                                output = output + "\n Payload:"
                                    + System.Text.Encoding.UTF8.GetString(record.Payload, 0, record.Payload.Length);
                            }
                        }
                    }

                    if (!record.IsSpRecord) 
                    { 
                        output = output + "\n --End of Record No." + recordNumber;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Subscribes for NDEF messages. This ensures that we get notified
        /// about the NFC events.
        /// </summary>
        private void SubscribeForMessage()
        {            
            if (device != null)
            {
                recordList.Clear();
                subscriptionId = device.SubscribeForMessage("NDEF", MessageReceived);
            }
        }
    }
}
