using Grpc.Core;
using System;
using System.Collections.Generic;

namespace UniformRpc
{
    public class Descriptors
    {
        public class TypeDescriptor {
            public TypeDescriptor(Type t)
            {
                if (t == null && !t.IsGenericType)
                {
                    throw new ArgumentNullException(nameof(t));
                }

                IsStream = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IObservable<>);
                Type = t.IsGenericType ? t.GetGenericArguments()[0] : t;
            }
            public Type Type { get; set; }
            public bool IsStream { get; set; }
        }

        public interface IMethodDesciptorVisitor {
            void Accept<TReq, TResp>(MethodDesciptorTyped<TReq, TResp> method) 
                where TReq :class
                where TResp : class;
        }
        public abstract class MethodDesciptor
        {
            public readonly string name;
            public readonly string serviceName;

            public MethodDesciptor(string name, string serviceName, Type tReq, Type tResp)
            {
                this.name = name;
                this.serviceName = serviceName;
                Request = new TypeDescriptor(tReq);
                Response = new TypeDescriptor(tResp);
            }
            public TypeDescriptor Request { get; set; }
            public TypeDescriptor Response { get; set; }
            public MethodType GetMethodType()
            {
                if (Request.IsStream && Response.IsStream) return MethodType.DuplexStreaming;
                if (Request.IsStream) return MethodType.ClientStreaming;
                if (Response.IsStream) return MethodType.ServerStreaming;
                return MethodType.Unary;
            }
            public abstract void Visit(IMethodDesciptorVisitor visitor);
        }

        public class MethodDesciptorTyped<TReq,TResp>: MethodDesciptor
                where TReq : class
                where TResp : class
        {
            public MethodDesciptorTyped(string name, string serviceName): base(name, serviceName, typeof(TReq), typeof(TResp))
            {

            }

            public Method<TReq, TResp> GetMethod()
            {
                return new Method<TReq, TResp>(
                    type: GetMethodType(),
                    serviceName: serviceName,
                    name: name,
                    requestMarshaller: Marshallers.Create(
                        serializer: Serializer<TReq>.ToBytes,
                        deserializer: Serializer<TReq>.FromBytes),
                    responseMarshaller: Marshallers.Create(
                        serializer: Serializer<TResp>.ToBytes,
                        deserializer: Serializer<TResp>.FromBytes));
            }

            public override void Visit(IMethodDesciptorVisitor visitor)
            {
                visitor.Accept<TReq,TResp>(this);
            }
        }

        
        public static MethodDesciptor[] GetMethods<T>() {
            var res = new List<MethodDesciptor>();
            var methods = typeof(T).GetMethods();
            foreach (var m in methods)
            {
                var d1 = typeof(MethodDesciptorTyped<,>);
                Type[] typeArgs = { m.GetParameters()[0].ParameterType, m.ReturnType };
                var makeme = d1.MakeGenericType(typeArgs);
                var desc = (MethodDesciptor)Activator.CreateInstance(makeme, m.Name, typeof(T).Name);
                res.Add(desc);
            }
            return res.ToArray();
        }
        
    }

}
