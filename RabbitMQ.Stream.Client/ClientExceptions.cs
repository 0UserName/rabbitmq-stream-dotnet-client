using System;

namespace RabbitMQ.Stream.Client
{
    internal static class ClientExceptions
    {
        public static void MaybeThrowException(ResponseCode responseCode, string message)
        {
            if (responseCode != ResponseCode.Ok)
            {
                throw responseCode switch
                {
                    ResponseCode.VirtualHostAccessFailure => new VirtualHostAccessFailureException(message),
                    ResponseCode.SaslAuthenticationFailureLoopback => new AuthenticationFailureLoopback(message),
                    ResponseCode.AuthenticationFailure => new AuthenticationFailureException(message),
                    //TODO Implement for all the response code
                    _ => new GenericProtocolException(responseCode, message)
                };
            }
        }
    }

    public class ProtocolException : Exception
    {
        protected ProtocolException(string s) :
            base(s)
        {
        }
    }

    public class GenericProtocolException : ProtocolException
    {
        public GenericProtocolException(ResponseCode responseCode, string s) :
            base($"response code: {responseCode} - {s}")
        {
        }
    }

    public class AuthenticationFailureException : ProtocolException
    {
        public AuthenticationFailureException(string s) :
            base(s)
        {
        }
    }
    
    public class AuthenticationFailureLoopback : ProtocolException
    {
        public AuthenticationFailureLoopback(string s) :
            base(s)
        {
        }
    }
    
    public class VirtualHostAccessFailureException : ProtocolException
    {
        public VirtualHostAccessFailureException(string s) :
            base(s)
        {
        }
    }
}