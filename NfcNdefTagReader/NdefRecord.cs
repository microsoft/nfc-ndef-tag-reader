/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

using System;

namespace NfcNdefTagReader
{
    /// <summary>
    /// Container class for NDEF record information. The class property names
    /// are defined based on the NDEF specification:
    /// - Mb: Message begin
    /// - Me: Message end
    /// - Cf: Chunk flag
    /// - Sr: Short record
    /// - Il: ID length of the field present
    /// - Tnf: Type name format
    /// 
    /// For further information, see http://www.nfc-forum.org/spec
    /// </summary>
    public class NdefRecord
    {
        public bool Mb { get; set; }
        public bool Me { get; set; }
        public bool Cf { get; set; }
        public bool Sr { get; set; }
        public bool Il { get; set; }
        public byte Tnf { get; set; }

        public UInt16 TypeLength { get; set; }
        public UInt16 IdLength { get; set; }
        public UInt32 PayloadLength { get; set; }
        public byte[] Type { get; set; }
        public byte[] Id { get; set; }
        public byte[] Payload { get; set; }

        // Is Smart Poster record
        public bool IsSpRecord { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NdefRecord()
        {
            Type = null;
            Id = null;
            Payload = null;
        }
    }
}
