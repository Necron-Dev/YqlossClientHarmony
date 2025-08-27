using System;

namespace YqlossClientHarmony.Utilities;

public class WTFException(string message = "") : Exception("What a Terrible Failure! How does this even happen! " + message);