/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.Protocol {

    /// <summary>
    ///  Event types used in the beacon protocol.
    /// </summary>
    public enum EventType {
        ACTION = 1,                     // Action
        VALUE_STRING = 11,              // captured string
        VALUE_INT = 12,                 // captured int
        VALUE_DOUBLE = 13,              // captured double
        NAMED_EVENT = 10,               // named event
        SESSION_END = 19,               // session end
        WEBREQUEST = 30,                // tagged web request
        ERROR = 40,                     // error
        CRASH = 50                      // crash
    }

}
