﻿namespace PatrynSolutions.Utilities.Monad
{
    using System;
    using System.Collections.Generic;

    public static class MaybeExtensions
    {

        #region Maybe Extensions

        /// <summary>
        /// Creates an empty <see cref="Maybe"/> with the <see cref="Exception"/> thrown.
        /// </summary>
        /// <param name="exception">The exception thrown by the callee.</param>
        /// <returns>An empty <see cref="Maybe"/> with the exception thrown in the called code.</returns>
        public static Maybe ToMaybe(this Exception exception)
        {
            return new Maybe(exception);
        }

        /// <summary>
        /// Creates an empty <see cref="Maybe"/> with a new message for the caller.
        /// </summary>
        /// <param name="message">The message for the caller.</param>
        /// <returns>An empty <see cref="Maybe"/> with a message for the caller.</returns>
        public static Maybe ToMaybe(this string message)
        {
            return new Maybe(message);
        }

        /// <summary>
        /// Converts a <see cref="Maybe"/> into a <see cref="Maybe{TValue}"/>. This conversion currently does not perform checks for 
        /// constructs that were created through the multiparameter constructors. IE a <see cref="Maybe"/> created with both a 
        /// <see cref="Maybe{TValue}.Message"/> and <see cref="Maybe{TValue}.FriendlyMessage"/> will currently only return a new 
        /// <see cref="Maybe{TValue}"/> with just a <see cref="Maybe.Message"/> value.
        /// </summary>
        /// <typeparam name="TValue">The type of the maybe being created.</typeparam>
        /// <param name="maybe">The maybe being converted.</param>
        /// <returns>A new <see cref="Maybe{TValue}"/> with the values of the current maybe.</returns>
        public static Maybe<TValue> ConvertMaybeToMaybeGeneric<TValue>(this Maybe maybe) where TValue : new()
        {
            if (maybe.HasValue)
            {
                if (typeof(TValue) == typeof(bool))
                    return new Maybe<TValue>((TValue)Convert.ChangeType(maybe.Value, typeof(TValue)));

                return new Maybe<TValue>(new TValue());
            }

            if (maybe.HasMessage && maybe.IsExceptionState)
                return new Maybe<TValue>(maybe.Message, maybe.Exception);

            if (maybe.HasMessage)
                return new Maybe<TValue>(maybe.Message);

            if (maybe.HasFriendlyMessage)
                return new Maybe<TValue>(maybe.FriendlyMessage, isFriendlyMessage: true);
            
            if (maybe.IsExceptionState && maybe.Exception != null)
                return new Maybe<TValue>(maybe.Exception);

            if (maybe.Exceptions.Count > 0)
                return new Maybe<TValue>(maybe.Exceptions);

            if (maybe.HasErrorCode)
                return new Maybe<TValue>(maybe.ErrorCode);

            return Maybe<TValue>.Empty();
        }

        #endregion Maybe Extensions

        #region Maybe Generic Extensions

        /// <summary>
        /// Wraps the value in a new <see cref="Maybe{T}"/>. If the value is null, or equal to the default of the type, 
        /// an empty maybe will be created.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<T> ToMaybe<T>(this T value)
        {
            if (value == null || (value is object && EqualityComparer<T>.Default.Equals(default(T), value)))
            {
                return Maybe<T>.Empty();
            }
            return new Maybe<T>(value);
        }

        /// <summary>
        /// Creates an empty <see cref="Maybe{T}"/> with the exception thrown in the called code.
        /// </summary>
        /// <typeparam name="T">The type the <see cref="Maybe{T}"/> is wrapping</typeparam>
        /// <param name="exception">The <see cref="Exception"/> thrown in the caller code.</param>
        /// <returns>An empty <see cref="Maybe{T}"/> with the thrown exception from the caller's code.</returns>
        public static Maybe<T> ToMaybe<T>(this Exception exception)
        {
            return new Maybe<T>(exception);
        }

        /// <summary>
        /// This one might get a bit wonky if you need to return <see cref="Maybe{string}"/> and the string you're 
        /// setting is a message, not the value.
        /// </summary>
        /// <typeparam name="T">The type of the maybe wrapper to create.</typeparam>
        /// <param name="value">The message for the caller.</param>
        /// <param name="isMessage">Helps determine if the string is a messsage, or value for string types and derivations.</param>
        /// <returns>An empty <see cref="Maybe{T}"/> with a message for the caller.</returns>
        public static Maybe<T> ToMaybe<T>(this string value, bool isMessage = false)
        {
            return new Maybe<T>(value, isMessage);
        }

        /// <summary>
        /// Eagerly enumerates the values, wrapping each value in a <see cref="Maybe{TValue}"/> of the desired collection type. 
        /// This method is not guaranteed to be thread safe.
        /// </summary>
        /// <typeparam name="TEnumerable">The enumerable type that will produce the values.</typeparam>
        /// <typeparam name="TValue">The type of the value to be wrapped.</typeparam>
        /// <typeparam name="TResult">The type of collection to be returned.</typeparam>
        /// <param name="values">The values to be enumerated and wrapped.</param>
        /// <returns>A new collection of <see cref="Maybe{TValue}"/> of the wrapped values.</returns>
        public static TResult ToEnumerableMaybe<TEnumerable, TValue, TResult>(this TEnumerable values)
            where TEnumerable : IEnumerable<TValue>
            where TResult : IEnumerable<Maybe<TValue>>, ICollection<Maybe<TValue>>, new()
        {
            var collection = new TResult();
                
            foreach (var value in values)
                collection.Add(new Maybe<TValue>(value));

            return collection;  
        }

        /// <summary>
        /// Converts a <see cref="Maybe{TValue}"/> into a <see cref="Maybe"/>. This conversion currently does not perform checks for 
        /// constructs that were created through the multiparameter constructors. IE a <see cref="Maybe{TValue}"/> created with both a 
        /// <see cref="Maybe{TValue}.Message"/> and <see cref="Maybe{TValue}.FriendlyMessage"/> will currently only return a new 
        /// <see cref="Maybe"/> with just a <see cref="Maybe.Message"/> value.
        /// </summary>
        /// <typeparam name="TValue">The type of the maybe being converted.</typeparam>
        /// <param name="maybe">The maybe being converted.</param>
        /// <returns>A new <see cref="Maybe"/> with the values of the current maybe.</returns>
        public static Maybe ConvertMaybeGenericToMaybe<TValue>(this Maybe<TValue> maybe)
        {
            if (maybe.HasValue)
                return new Maybe(true);

            if (maybe.HasMessage && maybe.IsExceptionState)
                return new Maybe(maybe.Message, maybe.Exception);

            if (maybe.HasMessage)
                return new Maybe(maybe.Message);

            if (maybe.HasFriendlyMessage)
                return new Maybe(maybe.FriendlyMessage, isFriendlyMessage: true);

            if (maybe.IsExceptionState && maybe.Exception != null)
                return new Maybe(maybe.Exception);

            if (maybe.Exceptions.Count > 0)
                return new Maybe(maybe.Exceptions);

            if (maybe.HasErrorCode)
                return new Maybe(maybe.ErrorCode);

            return Maybe.Empty();
        }

        #endregion Maybe Generic Extensions
    }
}
