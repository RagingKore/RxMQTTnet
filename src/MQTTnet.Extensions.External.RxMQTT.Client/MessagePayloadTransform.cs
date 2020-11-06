﻿using System;
using System.Reactive.Linq;

namespace MQTTnet.Extensions.External.RxMQTT.Client
{
    internal class MessagePayloadTransform<T>
    {
        private readonly IObservable<MqttApplicationMessage> source;
        private readonly Func<byte[], T> getPayloadFunc;
        private readonly bool skipOnError;

        public MessagePayloadTransform(IObservable<MqttApplicationMessage> source, Func<byte[], T> getPayloadFunc, bool skipOnError)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.getPayloadFunc = getPayloadFunc ?? throw new ArgumentNullException(nameof(getPayloadFunc));
            this.skipOnError = skipOnError;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception is forwarded to subscriber.")]
        public IObservable<T> Run()
        {
            return Observable.Create<T>(observer =>
            {
                return source.Subscribe(message =>
                    {
                        try
                        {
                            observer.OnNext(getPayloadFunc(message.Payload));
                        }
                        catch (Exception exception)
                        {
                            if (!skipOnError)
                                observer.OnError(exception);
                        }
                    },
                    exception => observer.OnError(exception),
                    () => observer.OnCompleted());
            });
        }
    }
}