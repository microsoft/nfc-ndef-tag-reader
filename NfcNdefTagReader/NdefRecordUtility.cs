/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

using System;
using System.Collections.Generic;
using Windows.Storage.Streams;

namespace NfcNdefTagReader
{
    /// <summary>
    /// This class provides the utility methods for handling NDEF record data.
    /// </summary>
    static class NdefRecordUtility
    {      
        /// <summary>
        /// Resolves the type name format of the given record.
        /// </summary>
        /// <param name="record">NDEF record</param>
        /// <returns>The type name format matching the given record.</returns>
        static public string GetTypeNameFormat(NdefRecord record) 
        {
            var typeName = "";

            switch (record.Tnf)
            {
                case 0x00:
                    typeName = "empty";
                    break;
                case 0x01:
                    typeName = "wkt";
                    break;
                case 0x02:
                    typeName = "mimetype";
                    break;
                case 0x03:
                    typeName = "absUri";
                    break;
                case 0x04:
                    typeName = "ext";
                    break;
                case 0x05:
                    typeName = "unknown";
                    break;
                case 0x06:
                    typeName = "unchanged";
                    break;
                case 0x07:
                    typeName = "reserved";
                    break;
                default:
                    typeName = "error";
                    break;
            }

            return typeName;
        }

        /// <summary>
        /// Extracts the data from the given record and places it into the
        /// given URI RTD.
        /// </summary>
        /// <param name="record">The record containing the URI data.</param>
        /// <param name="uri">Where the data is placed.</param>
        static public void ReadUriRtd(NdefRecord record, NdefUriRtd uri)
        {
            byte[] buf = record.Payload;
            uri.Identifier = GetUriIdentifier(buf[0]);
            uri.Uri= System.Text.Encoding.UTF8.GetString(buf, 1, buf.Length-1);  
        }

        /// <summary>
        /// Extracts the data from the given record and places it into the
        /// given text RTD.
        /// </summary>
        /// <param name="record">The record containing the data.</param>
        /// <param name="text">Where the data is placed.</param>
        static public void ReadTextRtd(NdefRecord record, NdefTextRtd text)
        {
            byte[] buf = record.Payload;

            if (IsBitSet(buf[0], 6))
            {
                return;
            }

            var langLen = (byte)(buf[0] & 0x3f);
            var langBuf = new byte[langLen];
            System.Buffer.BlockCopy(buf, 1, langBuf, 0, langLen);
            text.Language=System.Text.Encoding.UTF8.GetString(langBuf, 0, langBuf.Length);
            var textLen = buf.Length - 1 - langLen;
            
            if (textLen <= 0)
            {
                return;
            }
            
            var textBuf = new byte[textLen];
            System.Buffer.BlockCopy(buf, 1 + langLen, textBuf, 0, textLen);
            
            if (IsBitSet(buf[0], 7))
            {
                text.Encoding = "UTF-16";
                text.Text = System.Text.Encoding.Unicode.GetString(textBuf, 0, textBuf.Length);
            }
            else
            {
                text.Encoding = "UTF-8";
                text.Text = System.Text.Encoding.UTF8.GetString(textBuf, 0, textBuf.Length);
            }
        }

        /// <summary>
        /// ReadNdefRecord reads a list of NdefRecord from DataReader
        /// </summary>
        /// <param name="buf">data buffer contains NdefRecord (s)</param>
        /// <param name="list">a list of NdefRecord as output</param>
        static public void ReadNdefRecord(DataReader buf, List<NdefRecord> list) 
        {
            ReadNdefRecord(buf, list, false);
        }

        /// <summary>
        /// Creates NDEF records based on the data in the given buffer.
        /// </summary>
        /// <param name="buf">The data buffer as input.</param>
        /// <param name="list">The list of new NDEF records as output.</param>
        /// <param name="isFirstRecordSp">Defines whether the first record is smart poster or not.</param>
        static private void ReadNdefRecord(DataReader buf, List<NdefRecord> list, bool isFirstRecordSp)
        {
            var record = new NdefRecord();
            byte header = buf.ReadByte();
            record.Mb = IsBitSet(header, 7);
            record.Me = IsBitSet(header, 6);
            record.Cf = IsBitSet(header, 5);
            record.Sr = IsBitSet(header, 4);
            record.Il = IsBitSet(header, 3);
            record.Tnf = (byte)(header & 0x07);
            record.TypeLength = buf.ReadByte();
            record.IsSpRecord = isFirstRecordSp;

            if (record.Il)
            {
                record.IdLength = buf.ReadByte();
            }
            else
            {
                record.IdLength = 0;
            }

            if (record.Sr)
            {
                record.PayloadLength = buf.ReadByte();
            }
            else
            {
                var lengthBuf = new byte[4];
                buf.ReadBytes(lengthBuf);
                record.PayloadLength = BitConverter.ToUInt32(lengthBuf, 0);
            }

            if (record.TypeLength > 0)
            {
                record.Type = new byte[record.TypeLength];
                buf.ReadBytes(record.Type);
            }

            if ((record.Il) && (record.IdLength > 0))
            {
                record.Id = new byte[record.IdLength];
                buf.ReadBytes(record.Id);
            }

            if (record.PayloadLength > 0)
            {
                if ((record.Tnf == 0x01)
                    && (System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength) == "Sp"))
                {
                    ReadNdefRecord(buf, list, true);
                    record.Payload = null;
                }
                else
                {
                    record.Payload = new byte[record.PayloadLength];
                    buf.ReadBytes(record.Payload);
                }
            }

            list.Add(record);

            if (!record.Me)
            {
                ReadNdefRecord(buf, list, isFirstRecordSp);
            }
        }

        /// <summary>
        /// Checks if a bit is set in a byte or not.
        /// </summary>
        /// <param name="b">The byte as input.</param>
        /// <param name="pos">The bit position as input.</param>
        /// <returns>True if the bit is set, false otherwise.</returns>
        static private bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        /// <summary>
        /// Resolves the URI identifier based on the given byte.
        /// </summary>
        /// <param name="abbrByte">The byte as input.</param>
        /// <returns>The identifier.</returns>
        static private string GetUriIdentifier(byte abbrByte)
        {
            var identifier = "";

            switch (abbrByte)
            {
                case 0x00: identifier = "";
                    break;
                case 0x01: identifier = "http://www.";
                    break;
                case 0x02: identifier = "https://www.";
                    break;
                case 0x03: identifier = "http://";
                    break;
                case 0x04: identifier = "https://";
                    break;
                case 0x05: identifier = "tel:";
                    break;
                case 0x06: identifier = "mailto:";
                    break;
                case 0x07: identifier = "ftp://anonymous:anonymous@";
                    break;
                case 0x08: identifier = "ftp://ftp.";
                    break;
                case 0x09: identifier = "ftps://";
                    break;
                case 0x0A: identifier = "sftp://";
                    break;
                case 0x0B: identifier = "smb://";
                    break;
                case 0x0c: identifier = "nfs://";
                    break;
                case 0x0d: identifier = "ftp://";
                    break;
                case 0x0e: identifier = "dav://";
                    break;
                case 0x0f: identifier = "news:";
                    break;
                case 0x10: identifier = "telnet://";
                    break;
                case 0x11: identifier = "imap:";
                    break;
                case 0x12: identifier = "rtsp://";
                    break;
                case 0x13: identifier = "urn:";
                    break;
                case 0x14: identifier = "pop:";
                    break;
                case 0x15: identifier = "sip:";
                    break;
                case 0x16: identifier = "sips:";
                    break;
                case 0x17: identifier = "tftp:";
                    break;
                case 0x18: identifier = "btspp://";
                    break;
                case 0x19: identifier = "btl2cap://";
                    break;
                case 0x1a: identifier = "btgoep://";
                    break;
                case 0x1b: identifier = "tepobex://";
                    break;
                case 0x1c: identifier = "irdaobex://";
                    break;
                case 0x1d: identifier = "file://";
                    break;
                case 0x1e: identifier = "urn:epc:id:";
                    break;
                case 0x1f: identifier = "urn:epc:tag:";
                    break;
                case 0x20: identifier = "urn:epc:pat:";
                    break;
                case 0x21: identifier = "urn:epc:raw:";
                    break;
                case 0x22: identifier = "urn:epc:";
                    break;
                case 0x23: identifier = "urn:nfc:";
                    break;
                default: identifier = "RFU";
                    break;
            }
            return identifier;
        }
    }
}
