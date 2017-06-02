using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robot
{
    /// <summary>
    /// Transfer Type
    /// </summary>
    public enum TransType
    {
        /// <summary>
        /// Text transfer
        /// </summary>
        TextMessage = 0,
        /// <summary>
        /// Transfer through transparent channel
        /// </summary>
        TransBuffer = 1,
        /// <summary>
        /// Transfer through transparent channel extension
        /// </summary>
        TransBufferEx = 2
    }
}
